using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

using Microsoft.Extensions.Logging;

using JADE.Learning;
using JADE.Utility;
using JADE.models;
using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using JADE;

class DbWorkflows(Config config, ILogger logger)
{
    readonly Config config = config;
    readonly ILogger Logger = logger;
    public async Task AddNewIdsToDb()
    {
        string newIdsLocation = ResourcesIO.GetPath(config, config.InputFile);
        if (File.Exists(newIdsLocation))
        {
            Logger.LogInformation("Loading new ids");
            using JadeDbContext context = new(config);
            var allIds = ResourcesIO.LoadSomeIDFromFile(newIdsLocation, Logger);
            var alreadySeen = context.Seen.Select(x => x.SeenId);
            List<Seen> newSeens = [];
            foreach (string id in allIds)
                if (!alreadySeen.Contains(id))
                    newSeens.Add(new(id));
            await context.Seen.AddRangeAsync(newSeens);
            await context.SaveChangesAsync();
            Logger.LogInformation($"Added {newSeens.Count()} new ids to Seen table");
        }
        else
            Logger.LogError($"Invalid path {newIdsLocation}");
    }
    public async Task<List<JADE.Learning.Product>> LoadProductsFromDb()
    {
        using JadeDbContext context = new(config);
        List<JADE.Learning.Product> products = [];

        Logger.LogInformation("Begin loading product data from db");
        products.AddRange(await context.Products.ToListAsync());
        Logger.LogInformation($"Loaded {products.Count} products");
        return products;
    }
    public async Task SaveProductsToDb(List<JADE.Learning.Product> products)
    {
        using JadeDbContext context = new(config);

        Logger.LogInformation("Saving product data to db");
        try
        {
            foreach(var product in products)
            {
                //todo - implement upsert operation
            }
            await context.Products.AddRangeAsync(products);
            context.SaveChanges();
        }
        catch (Exception e)
        {
            Logger.LogError("Couldn't save product data to db!");
            Logger.LogError(e.Message);
            throw;
        }
        Logger.LogInformation($"Saved {products.Count} products");
    }
    public async Task<List<JADE.Learning.Product>> LoadProductsFromJson()
    {
        List<JADE.models.Product> products = [];
        string productsFile = ResourcesIO.GetPath(config, config.SaveFile);

        Logger.LogInformation("Begin loading product data");
        if (File.Exists(productsFile))
        {
            Logger.LogInformation($"Loading data form {productsFile}");
            products = await ResourcesIO.LoadProductsFromFile<JADE.models.Product>(productsFile, Logger);
            Logger.LogInformation($"Loaded {products.Count} products");
        }
        else
            Logger.LogError($"Invalid path: {productsFile}");
        return [.. MapToNew(products)];
    }
    IEnumerable<JADE.Learning.Product> MapToNew(IEnumerable<JADE.models.Product> oldTypes)
    {
        List<JADE.Learning.Product> newTypes = [];
        using JadeDbContext context = new(config);
        foreach (var oldProduct in oldTypes)
        {
            if (oldProduct.ProductId is null)
                continue;
            JADE.Learning.Product product = new()
            {
                Description = JsonSerializer.Serialize(oldProduct.RawDescription),
                RawDescription = oldProduct.RawDescription,
                ProductId = oldProduct.ProductId,
                TradeId = oldProduct.TradeId,
                Implemented = oldProduct.Implemented,
                ForceImpl = oldProduct.ForceImpl,
                Skipped = oldProduct.Skipped,
                Void = oldProduct.VoidProduct
            };
            var res = context.Manufactrurers.Where(x => x.Name == oldProduct.Manufacturer).ToList();
            if (res.Count == 0)
            {
                JADE.Learning.Manufactrurer manufactrurer = new()
                {
                    Name = oldProduct.Manufacturer ?? "unknown",
                    WebAddres = "unknown"                    
                };
                context.Manufactrurers.Add(manufactrurer);
                context.SaveChanges();
                product.ManufactrurerId = context.Manufactrurers.OrderBy(x => x.Id).Last().Id;
            }
            else
                product.ManufactrurerId = res[0].Id;

            newTypes.Add(product);
        }
        return newTypes;
    }
}