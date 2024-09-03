using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTree.Api;
using WebTree.DbObjects;

namespace WebTree.Logic;

public class RequestEventForLog
{
    private static long globalEventId = DateTime.UtcNow.ToBinary();

    private readonly long _eventId;
    private readonly DateTime _datetime;
    private string _reqQuery;
    private string _reqBody;
    private string _message;
    private string _exMessage;
    private string _stackTrace;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    
    public RequestEventForLog(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _eventId = Interlocked.Increment(ref globalEventId);
        _datetime =  DateTime.UtcNow;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Обработать результаты выполнения запроса
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="resultCode"></param>
    /// <param name="treeName">имя дерева</param>
    /// <param name="nodeId">ид ноды или родительской нодв</param>
    /// <param name="nodeName">имя ноды при создании или переименовании</param>
    /// <returns></returns>
    public IActionResult SetResult(ControllerBase controller, OperationResult resultCode, string treeName, long nodeId, string nodeName)
    {
        switch (resultCode)
        {
            case OperationResult.Success:
                Console.WriteLine(controller.Request.Query.ToString());
                return controller.StatusCode(StatusCodes.Status200OK);
            
            case OperationResult.ErrNoTree:
                _message = $"Error. Tree {treeName} not found.";
                break;
            case OperationResult.ErrNoParrentNodeInTree:
            case OperationResult.ErrNoNodeInTree:
                _message = $"Error. Node {nodeId} not found in tree \"{treeName}\".";
                break;
            case OperationResult.ErrParrentHasNodeWithThisName:
                _message = $"Error. Node {nodeId} already have child node with name \"{nodeName}\".";
                break;
            case OperationResult.ErrNothingToRename:
                _message = $"Error. Node name {nodeName} already have child node with name \"{nodeName}\".";
                break;
            case OperationResult.ErrNodeHaveChildren:
                _message = $"Error. You have to delete all children nodes first.";
                break;
        }

        Task.Run(WriteSelfToDb);
        // ^ в базу событие пишем таской чтоб не задерживать ответ и не упасть от ошибок бд журнала
        
        return controller.StatusCode(StatusCodes.Status500InternalServerError, this.ToExceptionDto());
    }

    /// <summary>
    /// Обработать результаты запроса если он сгенерировал ошибку
    /// </summary>
    public IActionResult SetException(ControllerBase controller, Exception ex)
    {
        _message = $"Internal server error ID = {_eventId}.";
        _exMessage = ex.Message;
        
        var sb = new StringBuilder();
        foreach (var kvp in controller.Request.Query)
        {
            if(sb.Length>0)
                sb.Append(System.Environment.NewLine);
            sb.Append(kvp.Key);
            sb.Append(" = ");
            sb.Append(kvp.Value);
        }
        _reqQuery = sb.ToString();
        
        using (var reader = new StreamReader(controller.Request.Body))
        {
            var readTask = reader.ReadToEndAsync();
            _reqBody = readTask.Result;
        }
        
        _stackTrace = ex.StackTrace;

        Task.Run(WriteSelfToDb);
        // ^ в базу событие пишем таской чтоб не задерживать ответ и не упасть от ошибок бд журнала

        return controller.StatusCode(StatusCodes.Status500InternalServerError, this.ToExceptionDto());
    }

    /// <summary>
    /// записывет содержимое эвента в БД и диспоузит контекст. Чтоб 1 событи нелья было попытаться записать 2 раза
    /// </summary>
    private void WriteSelfToDb()
    {
        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            dbContext.JournalRecords.Add(this.ToDbObject());
            dbContext.SaveChanges();
        }

    }

    private ExceptionDTO ToExceptionDto()
    {
        return new ExceptionDTO("Secure", _eventId, _message);
    }
    
    private JournalRecordDbObject ToDbObject()
    {
        return new JournalRecordDbObject()
        {
            EventId = this._eventId.ToString(),
            Body = this._reqBody,
            Querry = this._reqQuery,
            Message = string.IsNullOrEmpty(this._exMessage) ? this._exMessage : this._message,
            Stacktrace = this._stackTrace,
            TimeStamp = this._datetime,
        };
    }
}