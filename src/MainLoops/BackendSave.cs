using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using JADE.models;
using JADE.Backend;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace JADE;

public partial class Program
{
    public static async Task<List<Product>> BackendSaveAsync(List<Product> products, BackendNavigation navigate, ILogger logger)
    {
        int count = 0;
        int skipped = 0;

        logger.LogInformation("Begin saving data to backend");
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
            logger.LogCritical($"Critical error {e.Message}");
            logger.LogCritical("Emergency data save and shutdown!");
            string filePath = Path.Combine(config.DataDir ?? "", config.SaveFile ?? "");
            var serializedProducts = JsonSerializer.Serialize(products);
            await File.WriteAllTextAsync(filePath, serializedProducts);
            throw;
        }
        logger.LogInformation($"Saving data completed, saved {count} and skipped {skipped} products");
        return products;
    }

}