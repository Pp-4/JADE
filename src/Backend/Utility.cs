using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System;

using JADE.models;

namespace JADE.Backend;

public partial class BackendNavigation
{
    public async Task SaveProductsToFile(List<Product> products)
    {
        Console.WriteLine($"Saving product data to {dataStorageLocation}");
        string? dataPath = config["data"];
        if (dataPath is not null)
        {
            string filePath = Path.Combine(dataPath, dataStorageLocation);
            string content = JsonSerializer.Serialize(products);
            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine("Product data saved.");
        }
        else
            Console.WriteLine("Invalid or missing data directory path in config file!");
    }
    public async Task<List<Product>> LoadProductsFromFile()
    {
        List<Product> products = [];
        string? dataPath = config["data"];
        if (dataPath is not null)
        {
            string filePath = Path.Combine(dataPath, dataStorageLocation);
            if (File.Exists(filePath))
            {
                Console.WriteLine($"Loading product data from {dataStorageLocation}");
                string jsonContent = File.ReadAllText(filePath);
                if (jsonContent.Length == 0)
                    return products;
                try
                {
                    var des = JsonSerializer.Deserialize<IEnumerable<Product>>(jsonContent);
                    if (des is not null)
                        products.AddRange(des);
                    else
                        Console.WriteLine($"No products loaded, skipping");
                }
                catch (JsonException e)
                {
                    Console.WriteLine($"Error when loading local product data: {e.Message}, skipping");
                    return products;
                }
            }
        }
        else
            Console.WriteLine("Invalid or missing data directory path in config file!");
        return products;
    }
    /// <summary>
    /// return the number from the path ie. example/ex/img_123.jpg returns 123
    /// non valid string will return 0
    /// </summary>
    /// <returns>number from string</returns>
    static int Parse(string nbr)
    {
        string temp = Path.GetFileNameWithoutExtension(nbr).Split('_')[^1];
        int.TryParse(temp, out int ret);
        return ret;
    }
}