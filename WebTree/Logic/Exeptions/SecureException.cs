namespace WebTree.Logic.Exeptions;

public class SecureException : Exception
{
    public SecureException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// пример кастомной ошибки
/// </summary>
public class NameDuplicateException : SecureException
{
    /// <summary>
    /// Я не понял зачем они нужны, мне привычнее через код результата операции делать.
    /// Но раз в тз есть требование наследоваться от ошибки, пожалуйста.
    /// </summary>
    public NameDuplicateException(long parrentNodeId, string nodeName, Exception? innerException)
        : base($"Error. Node {parrentNodeId} already have child node with name \"{nodeName}\".", innerException)
    {
    }
}