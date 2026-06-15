using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using JADE.models;
using JADE.Backend;
using JADE.Utility;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace JADE;

public partial class Program
{
    public static async Task<List<Product>> BackendFetchAsync(BackendNavigation navigate, ILogger logger)
    {
        string oldDataFile = ResourcesIO.GetPath(config, config.SaveFile);
        string newDataFile = ResourcesIO.GetPath(config, config.InputFile);
        List<Product> products = [];

        logger.LogInformation("Begin loading product data");
        if (File.Exists(oldDataFile))
        {
            logger.LogInformation($"Loading data form {oldDataFile}");
            products.AddRange(await ResourcesIO.LoadProductsFromFile(oldDataFile, logger));
            logger.LogInformation($"Loaded {products.Count} products");
        }
        else
            logger.LogError($"Invalid path: {oldDataFile}");

        if (File.Exists(newDataFile))
        {
            int count = products.Count;
            logger.LogInformation("Loading data from backend service");
            products.AddRange(ProductsFromSomeIds(ResourcesIO.LoadSomeIDFromFile(newDataFile, logger)));
            logger.LogInformation($"Loaded {products.Count - count} products");
        }
        else
            logger.LogError($"Invalid path {newDataFile}");

        DeDuplicate(ref products);
        products = [.. products.OrderBy(x => x.Manufacturer).ThenBy(x => x.ProductId)];
        logger.LogInformation($"Loading complete, loaded total of {products.Count} products");
        for (int i = 0; i < products.Count; i++)
        {
            if (!products[i].HasBasicInfo() && !products[i].VoidProduct)
            {
                try
                {
                    //reading data from backend
                    products[i] = await navigate.GetBaseInfo(products[i]);
                }
                catch (Exception e)
                {
                    logger.LogCritical($"Error during reading base info {e.Message}");
                    logger.LogCritical("Emergency data save and shutdown!");
                    string filePath = Path.Combine(config.DataDir, config.SaveFile);
                    var serializedProducts = JsonSerializer.Serialize(products);
                    await File.WriteAllTextAsync(filePath, serializedProducts);
                    throw;
                }
            }
        }
        return products;
    }
    public static IEnumerable<Product> ProductsFromSomeIds(IEnumerable<string> someIds)
    {
        foreach (string someId in someIds)
        {
            yield return new Product(someId);
        }
    }
    public static void DeDuplicate(ref List<Product> products)
    {
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].ProductId is not null)
                products[i].SomeId = null;

            for (int j = 0; j < products.Count; j++)
            {
                if (i != j && products[i].Equals(products[j]))
                {
                    products[i] = products[i].MergeProduct(products[j]);
                    products[j] = new(null);
                }
            }
        }
        products = [.. products.Where(x => x.SomeId is not null || x.ProductId is not null)];
    }
}