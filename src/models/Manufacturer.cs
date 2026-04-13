using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using System.IO;
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace PlaywrightSharp.models;

public abstract class Manufacturer(IPage _page, IConfiguration _config)
{
    protected IPage page = _page;
    protected IConfiguration config = _config;
    //put here primary name and aliases
    public abstract string[] Names { get; }
    //server address
    protected abstract string WebPage { get; }
    protected virtual int MaxImgSize => 256 * 1024;
    protected abstract string JsSnippet { get; }

    //use this list for addidtional filtering
    internal protected abstract List<string> FilterKeys { get; }

    public async Task<Product> GetProductData(Product product)
    {
        try
        {
            if (product.ForceImpl)
                Console.WriteLine("Forceful reimplementation of a product!");
            Console.WriteLine($"Fetching {product} data from {WebPage}");
            if (!page.Url.Contains(WebPage))
                await page.GotoAsync(WebPage);
            await LocateProduct(product);
            Console.WriteLine($"Product site located");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while searching: {ex.Message}");
            product.RawDescription = null;
            product.Skipped = true;
            //product.Implemented = false;
            return product;
        }

        // find product atributes
        // TODO modify to LoadResource to look inside asembly for it
        var script = Program.LoadResource(JsSnippet);
        await page.AddScriptTagAsync(new() { Content = script });

        string result = await page.EvaluateAsync<string>("runScript()");
        List<Prop> parsed = JsonSerializer.Deserialize<List<Prop>>(result);
        parsed = FilterAttributes(parsed);
        product.RawDescription = parsed;
        Console.WriteLine($"Found {product.RawDescription.Count} product properties");

        // find product images
        try
        {
            Directory.CreateDirectory(config["imgDir"]);
            Console.WriteLine("Looking for product images");
            List<string> links = await LocateImages(product);
            if (links.Count == 0)
                Console.WriteLine($"No images of product found");
            else
            {
                string path = Path.Combine(config["imgDir"], $"{product.ProductId}/");
                int imageNumber = 0;
                int localImgCount = 0;
                if (Directory.Exists(path))
                {
                    if (product.ForceImpl)
                    {
                        var files = Directory.GetFiles(path);
                        foreach (var file in files)
                            File.Delete(file);
                    }
                    else
                        localImgCount = Directory.GetFiles(path).Length;
                }

                if (localImgCount < links.Count)
                {
                    Directory.CreateDirectory(path);
                    localImgCount = Directory.GetFiles(path).Length;
                    using var client = new HttpClient();
                    foreach (var link in links)
                    {
                        //TODO this whole needs refactor ,exclude base64 data images
                        var bytes = await client.GetByteArrayAsync(link);
                        string imgType = Utility.ImageData.GetImageFileType(bytes);
                        //TODO refactor - check size before downloading, as this may bite you in the ass
                        if (bytes.Length < MaxImgSize && imgType != string.Empty)
                        {
                            string imgPath = Path.Combine(path, $"{product.ProductId}_{imageNumber}{imgType}");
                            await File.WriteAllBytesAsync(imgPath, bytes);
                            imageNumber++;
                        }
                    }
                }
                Console.WriteLine($"Found {links.Count} images, saved {imageNumber} of them to {path}");
                if (imageNumber == 0 && localImgCount != 0)
                    Console.WriteLine($"({localImgCount} image{(localImgCount > 1 ? "s" : "")} already exist{(localImgCount > 1 ? " " : "s")}) ");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when trying to get the images: {e.Message}");
            product.Skipped = true;
        }
        product.Skipped = false;
        product.Implemented = false;
        product.VoidProduct = false;
        return product;
    }

    private List<Prop> FilterAttributes(List<Prop> parsed)
    {
        //remove duplicate key value pairs
        var temp = parsed.GroupBy(x => (x.Key, x.Value)).Select(x => x.First());
        var keys = FilterKeys.Select(x => x.Trim().ToLower());
        //filter out banned keys
        if (FilterKeys.Count > 0)
            return [.. temp.Where(x => !keys.Any(y => x.Key.ToLower().Contains(y)))]; ;
        return [.. temp];
    }

    //magic happens here:
    //navigate the page with search bar to find product
    //return when apropriate page was found
    //if for whatever product page cannot be reached throw exception
    protected abstract Task LocateProduct(Product product);

    //more magic here:
    //assuming that current product page is displayed
    //try to save relevant product images:
    //get all relevant products images links (they will be filtered later)
    protected abstract Task<List<string>> LocateImages(Product product);


    //usefull function, if images can be located by single querrySelector
    //ie:
    // var imageElements = await page.Locator(".modal-vanilla-container .owl-item img").AllAsync();
    // the qS variable must be a valid js/css querrySelector / querrySelectorAll
    protected async Task<List<string>> SimpleLocateImages(string qS)
    {
        var imageElements = await page.Locator(qS).AllAsync();
        List<string> sources = [];
        for (int i = 0; i < imageElements.Count; i++)
        {
            string src = await imageElements[i].EvaluateAsync<string>("img => img.src");
            sources.Add(src);
        }
        return sources;
    }
    /// <summary>
    /// Optional part, return a descryption, useful when manufacturer provides sparse/no attribute list
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<List<string>> LocateDescription() => [];
}