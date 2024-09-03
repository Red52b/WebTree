using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using WebTree.Api;
using WebTree.DbObjects;

namespace WebTree.Logic;

public class LogReader
{
    private IDbContextFactory<AppDbContext> _dbContextFactory;
    
    public LogReader(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public JournalShortDataDTO GetRange(int skip, int take, DateTime? from, DateTime? to, string filter = null)
    {
        using (var db = _dbContextFactory.CreateDbContext())
        {
            IQueryable<JournalRecordDbObject> query = db.JournalRecords.OrderByDescending(jr => jr.id);
            if(!string.IsNullOrWhiteSpace(filter))
                query = query.Where(jr => jr.EventId.Contains(filter.Trim()));
            return GetData(query, skip, take, from, to);
        }
    }

    public JournalFullElementDTO GetSingle(long id)
    {
        using (var db = _dbContextFactory.CreateDbContext())
        {
            var dbElement = db.JournalRecords.SingleOrDefault(jr => jr.id == id);
            
            if (dbElement == null)
                return null;

            return new JournalFullElementDTO()
            {
                id = dbElement.id,
                eventId = dbElement.EventId,
                createdAt = dbElement.TimeStamp,
                text = dbElement.GetText()
            };
        }
    }


    internal static JournalShortDataDTO GetData(IQueryable<JournalRecordDbObject> query, int skip, int take, DateTime? from, DateTime? to)
    {
        if (!from.HasValue && !to.HasValue)
            return GetShortDto(query, skip, take);

        if (!from.HasValue)
            from = DateTime.MinValue;
        if (!to.HasValue)
            to = DateTime.UtcNow;

        query = query.Where(jr => jr.TimeStamp >= from && jr.TimeStamp <= to);
        return GetShortDto(query, skip, take);
    }

    internal static JournalShortDataDTO GetShortDto(IQueryable<JournalRecordDbObject> query, int skip, int take)
    {
        JournalShortDataDTO data = new();
        data.Count = query.Count();
        data.Skip = skip > data.Count ? data.Count : skip;
        
        if (data.Skip == data.Count)
            data.Items = new List<JournalShortElementDTO>();
        else
            data.Items = query.Skip(skip).Take(take).Select( q => new JournalShortElementDTO()
            {
                Id = q.id,
                EventId = q.EventId,
                CreatedAt = q.TimeStamp
            }).ToList();
        
        return data;
    }

}