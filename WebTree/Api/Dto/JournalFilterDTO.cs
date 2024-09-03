namespace WebTree.Api;

public class JournalFilterDTO
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Search { get; set; }
}