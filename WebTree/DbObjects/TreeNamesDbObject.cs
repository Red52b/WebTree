namespace WebTree.DbObjects;

public class TreeNameDbObject
{
    public long TreeId { get; set; }
    public string TreeName { get; set; }
    
    public  ICollection<TreeNodeDbObject> TreeNodes { get; set; }

    public TreeNameDbObject() { }
    public TreeNameDbObject(string name)
    {
        TreeName = name;
    }

}