using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.Analysis;
using Microsoft.Playwright;

using JADE.models;


namespace JADE.Backend;

/// <summary>
/// This class is used to navigate wapro service
/// </summary>
/// <param name="_page">playwright page</param>
/// <param name="_config">configuration class</param>
public partial class BackendNavigation(IPage _page, IConfiguration _config)
{
    readonly IPage page = _page;
    readonly IConfiguration config = _config;
    readonly string dataStorageLocation = _config["prodData"] ?? "data.json";


    //set the default type of search id
    SearchBy searchIdType = SearchBy.TRADEID;

    /// <summary>
    /// Get initial products list with filled in prodId, tradeId and manufacturer.
    /// Requiers existance of products.txt or products.csv file with prodId (one per line)
    /// </summary>
    internal async Task<List<Product>> GetProducts(List<Product> products)
    {
        DataFrame productIds;
        string? backend = config["backend"];
        string? dataDir = config["data"];
        string? prodFile = config["prodPath"];
        if (backend is null || dataDir is null || prodFile is null)
        {
            Console.WriteLine("Missing config when fetching product list!");
            return products;
        }
        try
        {
            string path = Path.Combine(dataDir, prodFile);
            Console.WriteLine($"Loading product list from {path}");
            if (Path.GetExtension(path) == ".csv")
                productIds = DataFrame.LoadCsv(path, ',', true, dataTypes: [typeof(string)]);
            else productIds = DataFrame.LoadCsv(path, header: false, dataTypes: [typeof(string)], separator: ';');
        }
        catch (Exception e)
        {
            throw new Exception($"Error when opening product list: {e.Message}");
        }
        if (!page.Url.StartsWith(backend))
            await LogIn();

        Dictionary<string, Product> mappedProducts = [];
        foreach (Product product in products)
        {
            if (product.TradeId is not null)
                mappedProducts.Add(product.TradeId, product);
            if (product.ProductId is not null)
                mappedProducts.Add(product.ProductId, product);
            if (product.SomeId is not null && !mappedProducts.ContainsKey(product.SomeId))
                mappedProducts.Add(product.SomeId, product);
        }

        foreach (DataFrameRow row in productIds.Rows)
        {
            //might be tradeId, might be productId
            string? someId = row[0].ToString();
            //don't add if there is already a copy of it, skip #comments
            if (someId is not null && !someId.Contains('#') && !mappedProducts.ContainsKey(someId))
            {
                Product product = await GetBaseInfo(someId);
                products.Add(product);
            }
        }
        return products;
    }
}