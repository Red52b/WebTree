using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTree.DbObjects;

namespace WebTree.Logic;

/// <summary>
/// Лес деревьев. Каждое дерево - кэшированный дбКонтекст.
/// С кэшем использует меньше памяти при нагрузке чем контекст на запрос.
/// Лок дерева на запись для 50 одновременных коннектов не показал заметного снижения производительности.
/// </summary>
public class Forest : IDisposable
{
    private bool _stillWorking = true;
    private readonly Thread _cleaningThread;
    private readonly int _treeCacheLifetime;
    
    private IDbContextFactory<AppDbContext> _dbContextFactory;
    private ConcurrentDictionary<string, Tree> _trees = new();

    /// <summary>
    /// поолучает дерево из кэша или из базы по имени
    /// </summary>
    /// <param name="name"></param>
    public Tree this [string name] => _trees.TryGetValue(name, out Tree result) ? result.Get() : _DbGetTreeOrCreate(name);
    
    public Forest([FromServices] IConfiguration configuration, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        if (!int.TryParse(configuration["TreeCacheLifeTimeSeconds"], out _treeCacheLifetime))
            _treeCacheLifetime = 60;
        
        _dbContextFactory = dbContextFactory;
        
        _cleaningThread = new Thread(ClenThread){ IsBackground = true };
        _cleaningThread.Start();
    }

    /// <summary>
    /// Проверяет наличие дерева в базе.
    /// Если дерево есть, добавляет его в кэш. Т.к. далее ожидается обращение к этому дереву
    /// </summary>
    /// <param name="treeName"></param>
    /// <returns></returns>
    public bool CheckExist(string treeName)
    {
        if (_trees.ContainsKey(treeName))
            return true;
        else
            return _DbHasTree(treeName);
    }

    private bool _DbHasTree(string treeName)
    {
        TreeNameDbObject treeRecord;
        using (var db = _dbContextFactory.CreateDbContext())
        {
            treeRecord = db.Names.SingleOrDefault(n => n.TreeName == treeName);
        }
        
        if (treeRecord != null)
            _trees.TryAdd(treeRecord.TreeName, new Tree(treeRecord.TreeId, treeRecord.TreeName, _dbContextFactory.CreateDbContext()));
        
        return treeRecord != null;
    }

    private Tree _DbGetTreeOrCreate(string treeName)
    {
        TreeNameDbObject treeRecord;
        using (var db = _dbContextFactory.CreateDbContext())
        {
            treeRecord = db.Names.SingleOrDefault(n => n.TreeName == treeName);
            if (treeRecord == null)
            {
                var newName = new TreeNameDbObject(treeName);
                db.Names.Add(newName);
                db.SaveChanges();
                treeRecord = db.Names.SingleOrDefault(n => n.TreeName == treeName);
            }
        }

        if (treeRecord == null)
            throw new NullReferenceException();
        
        _trees.TryAdd(treeRecord.TreeName, new Tree(treeRecord.TreeId, treeRecord.TreeName, _dbContextFactory.CreateDbContext()));
        return _trees[treeName];
    }

    private void ClenThread()
    {
        while (_stillWorking)
        {
            Thread.Sleep(5000);
            DateTime border = DateTime.UtcNow.AddSeconds(-_treeCacheLifetime);
            foreach (var kvp in _trees)
            {
                if (kvp.Value.LastAccesTime < border && _trees.TryRemove(kvp.Key, out var tree))
                    tree.Dispose();
            }
        }
    }

    
    public void Dispose()
    {
        _stillWorking = false;
        foreach (var kvp in _trees)
        {
            kvp.Value.Dispose();
        }
    }
}