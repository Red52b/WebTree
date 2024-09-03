namespace WebTree.Api;

public class JournalShortDataDTO
{
    public int Skip { get; set; }
    public int Count { get; set; }
    public List<JournalShortElementDTO> Items { get; set; }
}