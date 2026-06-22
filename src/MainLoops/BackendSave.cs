using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using JADE.models;
using JADE.Backend;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace JADE;

public partial class Jade
{
    public async Task<List<Product>> BackendSaveAsync(List<Product> products, BackendNavigation navigate)
    {
        int count = 0;
        int skipped = 0;

        Logger.LogInformation("Begin saving data to backend");
        try
        {
            for (int i = 0; i < products.Count; i++)
            {
                if (!products[i].Implemented &&
                    !products[i].Skipped &&
                    !products[i].VoidProduct &&
                    products[i].RawDescription?.Count > 0 ||
                    products[i].ForceImpl)
                {
                    products[i] = await navigate.FillProductInfo(products[i]);
                    count++;
                }
                else skipped++;
            }
        }
        catch (Exception e)
        {
            Logger.LogCritical($"Critical error {e.Message}");
            Logger.LogCritical("Emergency data save and shutdown!");
            string filePath = Path.Combine(Config.DataDir ?? "", Config.SaveFile ?? "");
            var serializedProducts = JsonSerializer.Serialize(products);
            await File.WriteAllTextAsync(filePath, serializedProducts);
            throw;
        }
        Logger.LogInformation($"Saving data completed, saved {count} and skipped {skipped} products");
        return products;
    }

}