using System.Threading.Tasks;
using System;

using Microsoft.Playwright;

using JADE.models;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using JADE.Utility;

namespace JADE.Backend;

public partial class BackendNavigation
{
    public async Task<Product> FillProductInfo(Product product)
    {
        Console.WriteLine($"Filling information about {product}");
        string? backend = config["backend"]!;
        string? prodId = product.ProductId;
        if (prodId is null)
        {
            Console.WriteLine($"Product {product} has no id!");
            return product;
        }
        try
        {
            await LogIn();
            await page.GotoAsync($"{backend}?indexCatalogue={prodId}");
            await page.Locator("td:nth-child(3)").Filter(new() { HasTextRegex = new Regex($"^{prodId}$") }).First.ClickAsync();
            await AddDescryption(product);
            await AddImages(prodId);
            if (await ActivateProduct())
                product.MarkAsImplemented();
            Console.WriteLine($"Completed");
            return product;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't fill the {product} , reason :");
            Console.WriteLine(e.Message);
            throw;
        }
    }
    async Task AddDescryption(Product product)
    {
        await page.GetByText("Tłumaczenia").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var checkbox = page.GetByRole(AriaRole.Checkbox).Nth(5);
        if (await checkbox.IsCheckedAsync())
            await checkbox.ClickAsync();

        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.Locator(".K1YXcinUNlAHTpehalfm").HoverAsync();
        await page.Locator(".K1YXcinUNlAHTpehalfm button").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForTimeoutAsync(100);
        await page.GetByRole(AriaRole.Button, new() { Name = "Show more items" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForTimeoutAsync(100);
        await page.GetByRole(AriaRole.Button, new() { Name = "Source" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var element = page.GetByRole(AriaRole.Textbox, new() { Name = "Source code editing area" });
        await element.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var text = await element.InputValueAsync();
        string description = await CheckPresentText(product, text, product.ForceImpl);
        await element.ClearAsync();
        await element.FillAsync(description);
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.EvaluateAsync("document.querySelectorAll('.k-pane.k-pane-static')[0].style.height = '90%'");
        var response = page.WaitForResponseAsync(r =>
                        r.Url.Contains("preview?redirect=false") &&
                        r.Request.Method == "POST" && r.Status == 200);
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        //save result and wait for response
        await page.Locator("button[data-testid='decision-box-accept-button']").First.ClickAsync(new LocatorClickOptions() { Force = true });
        await response;

    }
    async Task AddImages(string productId)
    {
        Console.WriteLine("Adding images to product");
        string imgDir = config["imgDir"] ?? throw new KeyNotFoundException("Missing image directory path!");
        //try to add product images
        string path = Path.Combine(imgDir, productId);

        int.TryParse(config["addImagesTimeoutMiliseconds"], out int timeout);
        timeout = timeout < 1000 ? 1000 : 30 * 1000;
        string[] localImg = [];
        if (Directory.Exists(path))
            localImg = Directory.GetFiles(path);
        if (localImg.Length > 0)
        {
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.EvaluateAsync(@"media = [...document.querySelectorAll('.absui-tabs-item__value')].filter(x => x.textContent == 'Media')[0];
                                           media.scrollIntoView();
                                           media.click();");
            //get already existing images
            await page.GetByRole(AriaRole.Listitem).GetByText("Zdjęcia").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            var div = page.Locator("div:has(>span:text('Zdjęcia')) div >> div");
            await div.WaitForAsync();
            string images = await div.InnerTextAsync();
            int.TryParse(images, out int onlineImgCount);
            //var onlineImg = await page.Locator("i.absui-icon--user-files[data-testid='k2-icon']").AllAsync();

            //execute if the are more local images than online images
            if (onlineImgCount < localImg.Length && onlineImgCount < 3)
            {
                //locate the save button
                await page.GetByRole(AriaRole.Button, new() { Name = "Dodaj z dysku" }).ClickAsync();
                var input = page.Locator("#root > :nth-child(7) .ygGNzPF6_qnwNrjgibGg");

                //pad online images with up to 3 local ones
                Array.Sort(localImg, (x, y) => Parsing.GetFileNumber(x) - Parsing.GetFileNumber(y));
                var addFiles = localImg.Take(3)
                            .Where(x => Parsing.GetFileNumber(x) > onlineImgCount - 1)
                            .ToArray();
                await input.SetInputFilesAsync(addFiles);
                await page.GetByRole(AriaRole.Button, new() { Name = "Zapisz" }).ClickAsync(new() { Force = true });

                //wait till confirmation box shows up
                await page.GetByText("Pomyślnie dodano").ClickAsync(new() { Timeout = timeout });
                Console.WriteLine($"Added {addFiles.Length} images");
            }
        }
        else
            Console.WriteLine($"No images added");
    }
    async Task<bool> ActivateProduct()
    {
        return true;
    }
    async Task<string> CheckPresentText(Product product, string alternativeText, bool forceReplace = false)
    {
        //#if DEBUG
        //Debug.Assert(alternativeText.Contains(product.ShortTradeId));
        //#endif

        if (!forceReplace && (alternativeText.Contains("table") || product.RawDescription?.Count < 1))
        {
            Console.WriteLine("Preserving previous description");
            return alternativeText;
        }
        if (product.ForceImpl)
            Console.WriteLine("Forced description rewrite");
        else
            Console.WriteLine("Writing new description");
        return product.Description;
    }
}