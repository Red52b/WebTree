namespace WebTree.DbObjects;

public class TreeNodeDbObject
{
    public long TreeId { get; set; }
    public long NodeId { get; set; }
    public long ParentNodeId { get; set; }
    public string NodeName { get; set; }
    
    public  TreeNameDbObject Tree { get; set; }

    public TreeNodeDbObject() { }

    public TreeNodeDbObject(long treeId, string nodeName, long parentNodeId)
    {
        TreeId = treeId;
        NodeName = nodeName;
        ParentNodeId = parentNodeId;
    }
}