using System.Threading.Tasks;
using System;

using Microsoft.Playwright;

using PlaywrightSharp.models;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlaywrightSharp.Backend;

public partial class BackendNavigation //: PageTest
{
    public async Task FillProductInfo(Product product)
    {
        Console.WriteLine($"Filling information about {product}");
        try
        {
            if (!page.Url.StartsWith(config["backend"]))
                await LogIn();
            await page.GotoAsync($"{config["backend"]}?indexCatalogue={product.ProductId}");
            await page.Locator("td:nth-child(3)").Filter(new() { HasTextRegex = new Regex($"^{product.ProductId}$") }).First.ClickAsync();

            await AddDescryption(product);
            await AddImages(product.ProductId);
            if (await ActivateProduct())
            {
                product.Implemented = true;
                product.Skipped = false;
                product.VoidProduct = false;
            }
            Console.WriteLine($"Completed");
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
        await page.WaitForTimeoutAsync(1000);
        await page.EvaluateAsync(@"integrateButton = document.querySelectorAll('div.absui-column div.abs-row div.abs-row>div.abs-row>div>input.absui-switch')[2]
                                       integrateButton.scrollIntoView();
                                       if(integrateButton.checked) integrateButton.click();");
        //easier to get the button to be visible with js than playwright
        try
        {
            await page.WaitForLoadStateAsync();
            //await Expect(page.Locator("span,absui-icon--mode-edit").First).ToBeVisibleAsync();
            await page.EvaluateAsync(@"invisibleEditButton = document.querySelector('span.absui-icon--mode-edit').parentElement;
                                       invisibleEditButton.style.visibility = 'visible';
                                       invisibleEditButton.scrollIntoView();
                                       invisibleEditButton.click();");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            await page.WaitForTimeoutAsync(1000);
            await page.EvaluateAsync(@"invisibleEditButton = document.querySelector('span.absui-icon--mode-edit').parentElement;
                                       invisibleEditButton.style.visibility = 'visible';
                                       invisibleEditButton.scrollIntoView();
                                       invisibleEditButton.click();");
        }
        await page.GetByRole(AriaRole.Button, new() { Name = "Pokaż więcej" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Źródło" }).ClickAsync();
        var element = page.GetByRole(AriaRole.Textbox, new() { Name = "Source code editing area" });

        await element.ClickAsync();
        var text = await element.InputValueAsync();
        string description = await CheckPresentText(product, text, product.ForceImpl);
        await element.ClearAsync();
        await element.FillAsync(description);
        var response = page.WaitForResponseAsync(r =>
                        r.Url.Contains("preview?redirect=false") &&
                        r.Request.Method == "POST" && r.Status == 200);
        //save result and wait for response
        await page.Locator("button[data-testid='decision-box-accept-button']").First.ClickAsync();
        await response;

    }
    async Task AddImages(string productId)
    {
        Console.WriteLine("Adding images to product");
        //try to add product images
        string path = Path.Combine(config["imgDir"], productId);

        int.TryParse(config["addImagesTimeoutMiliseconds"], out int timeout);
        timeout = timeout < 1000 ? 1000 : 30 * 1000;
        string[] localImg = [];
        if (Directory.Exists(path))
            localImg = Directory.GetFiles(path);
        if (localImg.Length > 0)
        {

            await page.EvaluateAsync(@"media = [...document.querySelectorAll('.absui-tabs-item__value')].filter(x => x.textContent == 'Media')[0];
                                           media.scrollIntoView();
                                           media.click();");
            //get already existing images
            var onlineImg = await page.Locator("i.absui-icon--user-files[data-testid='k2-icon']").AllAsync();

            //execute if the are more local images than online images
            if (onlineImg.Count < localImg.Length && onlineImg.Count < 3)
            {
                //locate the save button
                await page.GetByRole(AriaRole.Button, new() { Name = "Dodaj z dysku" }).ClickAsync();
                var input = page.Locator("#root > :nth-child(7) .ygGNzPF6_qnwNrjgibGg");

                //pad online images with up to 3 local ones
                Array.Sort(localImg, (x, y) => Parse(x) - Parse(y));
                var addFiles = localImg.Take(3)
                            .Where(x => Parse(x) > onlineImg.Count - 1)
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

    string FixPreviousDesc(string description)
    {
        return description;
    }
}