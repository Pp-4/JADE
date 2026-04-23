using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.IO;
using System;

using JADE.models;
using System.Linq;

namespace JADE.Utility;

public static class ResourcesIO
{

    //private static Assembly[] AllAssembliesOfCurrentAppDomain => System.AppDomain.CurrentDomain.GetAssemblies();
    internal static string LoadResource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        //list all resources in current assembly
        //Assembly.GetExecutingAssembly().GetManifestResourceNames();
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream is not null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        else throw new FileNotFoundException($"{name} resource not found!");
    }
    public static async Task<List<Product>> LoadProductsFromFile(string filePath)
    {
        List<Product> products = [];

        //string filePath = Path.Combine(dataPath, dataStorageLocation);
        if (File.Exists(filePath))
        {
            Console.WriteLine($"Loading product data from {filePath}");
            string jsonContent = File.ReadAllText(filePath);
            if (jsonContent.Length == 0)
                return products;
            try
            {
                var deserialized = JsonSerializer.Deserialize<IEnumerable<Product>>(jsonContent);
                if (deserialized is not null)
                    products.AddRange(deserialized);
                else
                    Console.WriteLine($"No products loaded, skipping");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error when loading local product data: {e.Message}, skipping");
                return products;
            }
        }
        else
            Console.WriteLine("Couldn't load product data, missing save file path!");
        return products;
    }
    public static IEnumerable<string> LoadSomeIDFromFile(string filePath)
    {
        Console.WriteLine($"Loading data from {filePath}");
        string content = File.ReadAllText(filePath);
        return content.Split('\n').Where(x => x.First() != '#');
    }
    public static async Task SaveProductsToFile(List<Product> products, string filePath)
    {
        if (filePath is not null)
        {
            Console.WriteLine($"Saving product data to {filePath}");
            string content = JsonSerializer.Serialize(products);
            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine("Product data saved.");
        }
        else
            Console.WriteLine("Couldn't save product data, missing save file path!");
    }
}