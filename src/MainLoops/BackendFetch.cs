using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using JADE.models;
using JADE.Backend;

namespace JADE;

public partial class Program
{

    public static async Task<List<Product>> BackendFetchAsync(BackendNavigation navigate)
    {
        Console.WriteLine("Begin fetching data from backend");

        Console.WriteLine("Looking for already saved data");
        List<Product> products = await navigate.LoadProductsFromFile();

        products = await navigate.GetProducts(products);

        products = [.. products.DistinctBy(x => x.ProductId ?? x.SomeId)];
        products = [.. products.OrderBy(x => x.Manufactuer).ThenBy(x => x.ProductId)];
        Console.WriteLine($"Fetching complete, fetched {products.Count} products");
        return products;
    }
}