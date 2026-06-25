using JADE.models;
using JADE.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JADE.Learning;

/// <summary>
/// this class is used by ef tool
/// </summary>
public class ContextFactory : IDesignTimeDbContextFactory<JadeDbContext>
{
    public JadeDbContext CreateDbContext(string[] args)
    {
        Jade jade = new();
        return new JadeDbContext(jade.GetConfig());
    }
}

public class JadeDbContext(Config _config) : DbContext
{

    readonly Config config = _config;
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        string dirPath = ResourcesIO.GetPath(config, config.DbConnectionString);
        optionsBuilder.UseSqlite($"Data Source={dirPath}");
    }
}