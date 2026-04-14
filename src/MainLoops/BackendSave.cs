using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using JADE.models;
using JADE.Backend;

namespace JADE;

public partial class Program
{
    public static async Task<List<Product>> BackendSaveAsync(List<Product> products, BackendNavigation navigate)
    {
        int count = 0;
        int skipped = 0;

        Console.WriteLine("Begin saving data to backend");
        foreach (var product in products)
        {
            if (!product.Implemented &&
                !product.Skipped &&
                !product.VoidProduct &&
                product.RawDescription?.Count > 0 ||
                product.ForceImpl)
            {
                await navigate.FillProductInfo(product);
                count++;
            }
            else skipped++;
        }
        Console.WriteLine($"Saving data completed, saved {count} and skipped {skipped} products");
        return products;
    }

}