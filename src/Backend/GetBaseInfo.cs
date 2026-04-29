using System.Threading.Tasks;
using System.Linq;
using System;

using JADE.models;

namespace JADE.Backend;

public partial class BackendNavigation
{

    public async Task<Product> GetBaseInfo(Product product)
    {
        string someId = product.ToString();
        await LogIn();
        try
        {
            someId = product.SomeId ?? product.ProductId ?? product.TradeId ?? throw new Exception();
            Console.WriteLine($"Finding data about {someId}");
            searchIdType = await GoToProduct(someId, searchIdType);
            product.MergeProduct(await SelectBestMatch(someId, searchIdType));
            Console.WriteLine($"Data found: {product}, Manufacturer: {product.Manufacturer}");
            return product;
        }
        catch // product was not found at all
        {
            Console.WriteLine($"{someId} not found in backend, marking as void");
            return product.MarkAsVoid();
        }
    }

    //this is in case of recieving list with wrong type of id
    //function returns which type of id worked
    async Task<SearchBy> GoToProduct(string someId, SearchBy idType)
    {
        string urlEncodedId = Uri.EscapeDataString(someId);

        if (idType == SearchBy.TRADEID)
        {
            try
            {   //search by tradeId first
                await page.GotoAsync($"{config["backend"]}?tradeIndex={urlEncodedId}");
                await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                return SearchBy.TRADEID;
            }
            catch
            {   //if not found, try search by productID
                await page.GotoAsync($"{config["backend"]}?indexCatalogue={urlEncodedId}");
                await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                Console.WriteLine("Product was being searching by TradeId, but was found by ProductId, switching further searches");
                return SearchBy.PRODUCTID;
            }
        }
        else if (idType == SearchBy.PRODUCTID)
        {
            try
            {   //search by productId first
                await page.GotoAsync($"{config["backend"]}?indexCatalogue={urlEncodedId}");
                await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                return SearchBy.PRODUCTID;
            }
            catch
            {   //if not found, try search by tradeID
                await page.GotoAsync($"{config["backend"]}?tradeIndex={urlEncodedId}");
                await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                Console.WriteLine("Product was being searching by ProductId, but was found by TradeId, switching further searches");
                return SearchBy.TRADEID;
            }
        }
        else throw new Exception("How ? If new type of search id was added it must also be implemented here!");
    }

    async Task<Product> SelectBestMatch(string someId, SearchBy idType)
    {
        var products = page.Locator("#products-grid .k-table-tbody > tr > :nth-child(2)");
        int count = await products.CountAsync();
        double bestScore = 0;
        Product product = new(null);
        for (int i = 0; i < count; i++)
        {
            var element = products.Nth(i);
            await element.ClickAsync();
            await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
            await page.Locator(".absui-icon.absui-icon--keyboard-arrow-right2").ClickAsync();
            await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);
            await page.GetByText("Atrybuty").First.ClickAsync();
            await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.DOMContentLoaded);

            string? productId = await page.Locator("//tr[td[normalize-space()='Indeks katalogowy']]/td[position()=3]").TextContentAsync();
            string? tradeId = await page.Locator("//tr[td[normalize-space()='Indeks handlowy']]/td[position()=3]").TextContentAsync();
            string? manufacturer = await page.Locator("//tr[td[normalize-space()='Producent']]/td[position()=3]").TextContentAsync();

            if (productId is null || tradeId is null || manufacturer is null)
                throw new ArgumentNullException();
            manufacturer = string.Join(' ', manufacturer.Split(' ').Take(2));

            double tempScore = idType == SearchBy.PRODUCTID ?
                 (double)someId.Length / productId.Length :
                 (double)someId.Length / tradeId.Length;

            if (tempScore > bestScore)
            {
                bestScore = tempScore;
                product.Resolve(productId, tradeId, manufacturer);
            }
        }
        return product;
    }
}