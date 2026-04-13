using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.Analysis;
using Microsoft.Playwright;

using PlaywrightSharp.models;


namespace PlaywrightSharp.Backend;

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
        try
        {
            string path = Path.Combine(config["data"], config["prodPath"]);
            Console.WriteLine($"Loading product list from {path}");
            if (Path.GetExtension(path) == ".csv")
                productIds = DataFrame.LoadCsv(path, ',', true, dataTypes: [typeof(string)]);
            else productIds = DataFrame.LoadCsv(path, header: false, dataTypes: [typeof(string)]);
        }
        catch (Exception e)
        {
            throw new Exception($"Error when opening product list: {e.Message}");
        }

        if (!page.Url.StartsWith(config["backend"]))
            await LogIn();
        foreach (DataFrameRow row in productIds.Rows)
        {
            //might be tradeId, might be productId
            string someId = row[0].ToString();

            //don't add if there is already a copy of it
            if (!products.Any(x => x.SomeId == someId))
            {
                Product product = await GetBaseInfo(someId);
                products.Add(product);
            }
        }
        return products;
    }
}