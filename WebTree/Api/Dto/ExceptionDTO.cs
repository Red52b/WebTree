namespace WebTree.Api;

public class ExceptionDTO
{
    public class ExceptionMessageDTO
    {
        public string message { get; }

        public ExceptionMessageDTO(string message)
        {
            this.message = message;
        }
    }
    
    public string type { get;  }
    public long id { get;  }
    public ExceptionMessageDTO data { get; }

    public ExceptionDTO(string type, long id, string message)
    {
        this.id = id;
        this.type = type;
        this.data = new ExceptionMessageDTO(message);
    }
}