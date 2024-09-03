using System.Text;

namespace WebTree.DbObjects;

public class JournalRecordDbObject
{
    public long id{ get; set; }
    public string EventId{ get; set; }
    public DateTime TimeStamp{ get; set; }
    public string Querry{ get; set; }
    public string Body{ get; set; }
    public string Stacktrace{ get; set; }
    public string Message { get; set; }

    public string GetText()
    {
        StringBuilder sb = new();
        sb.Append(Message);

        AppendNext(sb, Querry);
        AppendNext(sb, Body);
        AppendNext(sb, Stacktrace);

        return sb.ToString();
    }

    private static void AppendNext(StringBuilder sb, string field)
    {
        if (string.IsNullOrEmpty(field))
            return;
        
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        sb.Append(field);
    }
}