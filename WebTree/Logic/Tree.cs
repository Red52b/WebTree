using WebTree.Api;
using WebTree.DbObjects;

namespace WebTree.Logic;

public class Tree : IDisposable
{
    private const long ROOT_NODE_ID = 0;
    
    private object dbLock = new object();
    public DateTime LastAccesTime;


    private long _id;
    private string _name;
    private AppDbContext _dbContext;
    
    public Tree(long id, string name, AppDbContext dbContext)
    {
        _id = id;
        _name = name;
        _dbContext = dbContext;
        LastAccesTime = DateTime.UtcNow;
    }

    public void Dispose()
    {
        lock (dbLock)
        {
            _dbContext.Dispose();
        }
    }
    
    public Tree Get()
    {
        LastAccesTime = DateTime.UtcNow;
        return this;
    }
    
    public OperationResult GetTree(out TreeNodeDTO rootNode)
    {
        List<TreeNodeDbObject> nodes;
        
        lock (dbLock)
        {
            nodes = _dbContext.Nodes.Where(n => n.TreeId == _id).OrderByDescending(n => n.ParentNodeId).ToList();
        }
        
        if (nodes.Count == 0)
        {   // пустое дерево, только корень
            rootNode = new TreeNodeDTO(ROOT_NODE_ID, _name);
            return OperationResult.Success;
        }
        
        //Строим дерево от ветвей к корню через висящие ветви.
        // Корректное дерево должно сойтись к корню (нода 0).
        // Немного некорректное может иметь висячие ноды, которые нужно удалить.
        // Совсем некорректное не сойдется к корню и подлежит полному удалению
        var hangingBranches = new Dictionary<long, TreeNodeDTO>();
        var parrentNodeDto = new TreeNodeDTO(-1);
        foreach (var node in nodes)
        {
            if (node.ParentNodeId != parrentNodeDto.id)
            {
                parrentNodeDto = new TreeNodeDTO(node.ParentNodeId);
                hangingBranches.Add(parrentNodeDto.id, parrentNodeDto);
            }

            if (!hangingBranches.Remove(node.NodeId, out var thisNodeDto))
                thisNodeDto = new TreeNodeDTO(node.NodeId);

            thisNodeDto.name = node.NodeName;
            parrentNodeDto.children.Add(thisNodeDto);
        }

        if (hangingBranches.Remove(ROOT_NODE_ID, out var rootNodeDto))
        {
            rootNodeDto.name = _name;
            rootNode = rootNodeDto;

            if (hangingBranches.Count > 0)  // дерево сошлось, но есть висячие ветви
                lock (dbLock)
                {                           // удаляем текущие виясяки, за несколько проходов удалиться всё
                    _dbContext.Nodes.Where(n => n.TreeId == _id && hangingBranches.Keys.Contains(n.ParentNodeId)).DeleteFromQuery();
                }
        }
        else
        { // дерево НЕ сошлось, удаляем все ветви, возвращаем пустое дерево
            lock (dbLock)
            {
                _dbContext.Nodes.Where(n => n.TreeId == _id).DeleteFromQuery();
                _dbContext.Names.Where(n => n.TreeId == _id).DeleteFromQuery();
            }
            rootNode = new TreeNodeDTO(ROOT_NODE_ID, _name);
        }
        return OperationResult.Success;
        
    }

    
    public OperationResult AddNode(long parrentNodeId, string newNodeName)
    {
        lock (dbLock)
        {
            if (parrentNodeId != ROOT_NODE_ID)
            {
                var parentNode = _dbContext.Nodes.SingleOrDefault(n => n.TreeId == _id && n.NodeId == parrentNodeId);
                if (parentNode == null)
                    return OperationResult.ErrNoParrentNodeInTree;
            }

            var siblingNodeNames = _GetSiblingsNodeNames(parrentNodeId);
            if (siblingNodeNames.Contains(newNodeName))
                return OperationResult.ErrParrentHasNodeWithThisName;
            
            try
            {
                _dbContext.Nodes.Add(new TreeNodeDbObject(_id, newNodeName, parrentNodeId));
                _dbContext.SaveChanges();
            }
            catch (InvalidOperationException invalidOperEx)
            {   // может происходить когда N одновременных запросов попробуют создать ноды с одинаковым именем внутри одного родителя
                throw new Exeptions.NameDuplicateException(parrentNodeId, newNodeName, invalidOperEx);
            }
        }

        return OperationResult.Success;
    }

    public OperationResult RenameNode( long nodeId, string nodeNewName)
    {
        lock (dbLock)
        {
            var nodeToRename = _dbContext.Nodes.SingleOrDefault(n => n.TreeId == _id && n.NodeId == nodeId);
            if (nodeToRename == null)
                return OperationResult.ErrNoNodeInTree;

            if (nodeNewName == nodeToRename.NodeName)
                return OperationResult.ErrNothingToRename;
            
            var siblingNodeNames = _GetSiblingsNodeNames(nodeToRename.ParentNodeId);
            if (siblingNodeNames.Contains(nodeNewName))
                return OperationResult.ErrParrentHasNodeWithThisName;

            nodeToRename.NodeName = nodeNewName;
            
            try
            {
                _dbContext.Nodes.Update(nodeToRename);
                _dbContext.SaveChanges();
            }
            catch (InvalidOperationException invalidOperEx)
            {   // может происходить когда N одновременных запросов попробуют переименовать разные ноды в одно имя внутри одного родителя
                throw new Exeptions.NameDuplicateException(nodeToRename.ParentNodeId, nodeNewName, invalidOperEx);
            }
        }

        return OperationResult.Success;
    }
    
    public OperationResult DeleteNode(long nodeId)
    {
        lock (dbLock)
        {
            var nodeToDelete = _dbContext.Nodes.SingleOrDefault(n => n.TreeId == _id && n.NodeId == nodeId);
            if (nodeToDelete == null)
                return OperationResult.ErrNoNodeInTree;
            
            var childrenNodeNames = _GetSiblingsNodeNames(nodeToDelete.NodeId);
            if (childrenNodeNames.Count > 0)
                return OperationResult.ErrNodeHaveChildren;
        
            _dbContext.Nodes.Remove(nodeToDelete);
            _dbContext.SaveChanges();
        }
        
        return OperationResult.Success;
    }

    private List<string> _GetSiblingsNodeNames(long parrentNodeId)
    {
        var list = _dbContext.Nodes
            .Where(n => n.TreeId == _id && n.ParentNodeId == parrentNodeId);
         return list.Select(n=> n.NodeName).ToList();
    }
    
}