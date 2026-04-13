using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using PlaywrightSharp.models;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PlaywrightSharp;

public partial class Program
{

    public static async Task<List<Product>> ManufacFetchAsync(List<Product> products, Dictionary<string, Manufacturer> manufacturers, IConfiguration config)
    {
        int count = 0;
        int skipped = 0;
        int preFilled = 0;
        int notFound = 0;

        Console.WriteLine("Begin fetching data from manufacturers");
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].VoidProduct)
            {//skip products that were not found during initial backend fetch
                notFound++;
                continue;
            }
            string path = Path.Combine(config["imgDir"], products[i].ProductId);
            //dont look for products that are implemented or void, unless forceImplemented flag is set to 1
            if (!products[i].Implemented &&
                !products[i].VoidProduct && (
                products[i].RawDescription == null ||
                products[i].RawDescription.Count == 0 ||
                !Directory.Exists(path) ||
                Directory.GetFiles(path).Length == 0) ||
                products[i].ForceImpl)
            {
                count++;
                if (manufacturers.ContainsKey(products[i].Manufactuer))
                    products[i] = await manufacturers[products[i].Manufactuer].GetProductData(products[i]);
                else
                {
                    products[i].Skipped = true;
                    count--;
                    skipped++;
                }
            }
            else preFilled++;
        }
        Console.WriteLine($"Local detail data already exists for {preFilled} products, skipping");
        if (notFound > 0)
            Console.WriteLine($"{notFound} product{(notFound > 1 ? "s" : "")} not found");
        Console.WriteLine($"Fetching complete, fetched {count} and skipped {skipped + preFilled + notFound} products");
        return products;
    }
}