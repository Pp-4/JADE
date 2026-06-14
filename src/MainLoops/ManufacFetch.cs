using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;

using JADE.models;
using JADE.Utility;

namespace JADE;

public partial class Program
{

    public static async Task<List<Product>> ManufacFetchAsync(List<Product> products, Dictionary<string, Manufacturer> manufacturers, Config config)
    {
        int count = 0;
        int skipped = 0;
        int preFilled = 0;
        int notFound = 0;

        string? prodId;
        string? manufac;

        Console.WriteLine("Begin fetching data from manufacturers");
        for (int i = 0; i < products.Count; i++)
        {
            prodId = products[i].ProductId;
            if (products[i].VoidProduct || prodId is null)
            {//skip products that were not found during initial backend fetch
                notFound++;
                continue;
            }
            string dirPath = ResourcesIO.GetPath(config, config.ImgDir);
            string imgPath = Path.Combine(dirPath, prodId);

            manufac = products[i].Manufacturer;
            if (manufac is not null && manufacturers.ContainsKey(manufac))
                products[i].manufacturerObject = manufacturers[manufac];

            //dont look for products that are implemented or void, unless forceImplemented flag is set to 1
            if (!products[i].Implemented &&
                products[i].SkipCount < 3 &&
                !products[i].VoidProduct && (
                (products[i].RawDescription?.Count ?? 0) == 0 ||
                !Directory.Exists(imgPath) ||
                Directory.GetFiles(imgPath).Length == 0) ||
                products[i].ForceImpl)
            {
                count++;
                if (products[i].manufacturerObject is not null)
                {   //TODO verify that this won't break
                    await products[i].manufacturerObject!.GetProductData(products[i]);
                }
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


