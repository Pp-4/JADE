using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using JADE.models;
using JADE.Backend;
using JADE.Utility;
using System.IO;

namespace JADE;

public partial class Program
{
    public static async Task<List<Product>> BackendFetchAsync(BackendNavigation navigate)
    {
        string? someIdFile = config["newProductsFile"], prodFile = config["productsFile"], directory = config["dataDir"];
        List<Product> products = [];

        Console.WriteLine("Begin loading product data");
        if (prodFile is not null && directory is not null)
        {
            string filePath = Path.Combine(directory, prodFile);
            Console.WriteLine($"Loading data form {filePath}");
            products.AddRange(await ResourcesIO.LoadProductsFromFile(filePath));
            Console.WriteLine($"Loaded {products.Count} products");
        }
        else
            Console.WriteLine($"Invalid path dir:{directory} file:{prodFile}");
        if (someIdFile is not null && directory is not null)
        {
            string filePath = Path.Combine(directory, someIdFile);
            int count = products.Count;
            Console.WriteLine("Loading data from backend service");
            products.AddRange(ProductsFromSomeIds(ResourcesIO.LoadSomeIDFromFile(filePath)));
            Console.WriteLine($"Loaded {products.Count - count} products");
        }
        else
            Console.WriteLine($"Invalid path dir:{directory} file:{prodFile}");
        products = [.. products.DistinctBy(x => x.SomeId).DistinctBy(x => x.ProductId ?? x.SomeId).DistinctBy(x => x.TradeId ?? x.SomeId)];
        products = [.. products.OrderBy(x => x.Manufactuer).ThenBy(x => x.ProductId)];
        Console.WriteLine($"Loading complete, loaded total of {products.Count} products");
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].ProductId is null || products[i].TradeId is null || products[i].Manufactuer is null)
            {
                string? someId = products[i].SomeId;
                if (someId is not null && !products[i].VoidProduct)
                    products[i] = await navigate.GetBaseInfo(someId);
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
}