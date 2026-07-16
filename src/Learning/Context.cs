using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JADE.models;
using JADE.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    public DbSet<Seen> Seen { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        string dirPath = ResourcesIO.GetPath(config, config.DbConnectionString);
        optionsBuilder.UseSqlite($"Data Source={dirPath}");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var options = new JsonSerializerOptions();

        var converter = new ValueConverter<List<Prop>, string>(
            x => JsonSerializer.Serialize(x, options),
            x => JsonSerializer.Deserialize<List<Prop>>(x, options) ?? new());

        var comparer = new ValueComparer<List<Prop>>(
            (x, y) => x.SequenceEqual(y),
            x => x.Aggregate(0, (hash, prop) => HashCode.Combine(hash, prop.Key, prop.Value)),
            x => x.ToList());
        modelBuilder.Entity<Product>()
        .Property(p => p.RawDescription)
        .HasConversion(converter, comparer).HasColumnName("Descryption");
        
    }
}
