using System.Security.Principal;

namespace WebTree.Api;

public class TreeNodeDTO
{
    public long id { get; set; }
    
    public string name { get; set; }

    public List<TreeNodeDTO> children{ get; set; }
    
    public TreeNodeDTO(long id, string name=null)
    {
        this.id = id;
        this.name = name;
        this.children = new List<TreeNodeDTO>();
    }
}