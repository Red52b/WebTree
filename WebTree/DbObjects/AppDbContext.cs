using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebTree.DbObjects;

public class AppDbContext : DbContext
{
    private IConfiguration _configuration;
    public DbSet<TreeNodeDbObject> Nodes { set; get; }
    public DbSet<TreeNameDbObject> Names { set; get; }
    
    public DbSet<JournalRecordDbObject> JournalRecords { set; get; }
    
    public AppDbContext([FromServices] IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionStr = _configuration.GetConnectionString("DbConnection");
        optionsBuilder.UseNpgsql(connectionStr);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TreeNameDbObject>().HasKey(name => name.TreeId);
        modelBuilder.Entity<TreeNameDbObject>().Property(node => node.TreeId).ValueGeneratedOnAdd();
        modelBuilder.Entity<TreeNameDbObject>().HasAlternateKey(name => name.TreeName);
        
        modelBuilder.Entity<TreeNodeDbObject>().HasKey(node => new {node.TreeId, node.NodeId});
        modelBuilder.Entity<TreeNodeDbObject>().HasIndex(node => new {node.TreeId, node.ParentNodeId});
        modelBuilder.Entity<TreeNodeDbObject>().Property(node => node.TreeId).ValueGeneratedNever();
        modelBuilder.Entity<TreeNodeDbObject>().Property(node => node.NodeId).ValueGeneratedOnAdd();
        
        modelBuilder.Entity<TreeNodeDbObject>().HasAlternateKey(node => new {node.TreeId, node.ParentNodeId, node.NodeName});
        
        modelBuilder.Entity<TreeNameDbObject>()
            .HasMany(name => name.TreeNodes)
            .WithOne(node => node.Tree)
            .HasPrincipalKey(name => name.TreeId)
            .HasForeignKey(node => node.TreeId);
        
        modelBuilder.Entity<JournalRecordDbObject>().HasKey(jr => jr.id);
    }
    
}