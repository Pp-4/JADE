using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using JADE.models;
using JADE.Backend;
using System.Text.Json;
using System.IO;

namespace JADE;

public partial class Program
{
    public static async Task<List<Product>> BackendSaveAsync(List<Product> products, BackendNavigation navigate)
    {
        int count = 0;
        int skipped = 0;

        Console.WriteLine("Begin saving data to backend");
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
            Console.WriteLine($"Critical error {e.Message}");
            Console.WriteLine("Emergency data save and shutdown!");
            string filePath = Path.Combine(config["data"] ?? "", config["prodData"] ?? "");
            var serializedProducts = JsonSerializer.Serialize(products);
            await File.WriteAllTextAsync(filePath, serializedProducts);
            throw;
        }
        Console.WriteLine($"Saving data completed, saved {count} and skipped {skipped} products");
        return products;
    }

}