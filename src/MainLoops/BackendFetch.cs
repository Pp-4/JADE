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

public partial class Jade
{
    public  async Task<List<Product>> BackendFetchAsync(BackendNavigation navigate)
    {
        string oldDataFile = ResourcesIO.GetPath(Config, Config.SaveFile);
        string newDataFile = ResourcesIO.GetPath(Config, Config.InputFile);
        List<Product> products = [];

        Logger.LogInformation("Begin loading product data");
        if (File.Exists(oldDataFile))
        {
            Logger.LogInformation($"Loading data form {oldDataFile}");
            products.AddRange(await ResourcesIO.LoadProductsFromFile(oldDataFile, Logger));
            Logger.LogInformation($"Loaded {products.Count} products");
        }
        else
            Logger.LogError($"Invalid path: {oldDataFile}");

        if (File.Exists(newDataFile))
        {
            int count = products.Count;
            Logger.LogInformation("Loading data from backend service");
            products.AddRange(ProductsFromSomeIds(ResourcesIO.LoadSomeIDFromFile(newDataFile, Logger)));
            Logger.LogInformation($"Loaded {products.Count - count} products");
        }
        else
            Logger.LogError($"Invalid path {newDataFile}");

        DeDuplicate(ref products);
        products = [.. products.OrderBy(x => x.Manufacturer).ThenBy(x => x.ProductId)];
        Logger.LogInformation($"Loading complete, loaded total of {products.Count} products");
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
                    Logger.LogCritical($"Error during reading base info {e.Message}");
                    Logger.LogCritical("Emergency data save and shutdown!");
                    string filePath = Path.Combine(Config.DataDir, Config.SaveFile);
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