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
    public  async Task<List<Product>> BackendFetchAsync(BackendNavigation navigate, List<Product> products)
    {
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
}