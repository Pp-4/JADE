using System.Threading.Tasks;
using System.Linq;
using System;

using JADE.models;

namespace JADE.Backend;

public partial class BackendNavigation
{

    async Task<Product> GetBaseInfo(string someId)
    {
        Console.WriteLine($"Finding data about {someId}");

        if (!page.Url.StartsWith(config["backend"]))
            await LogIn();
        try
        {
            searchIdType = await GoToProduct(someId, searchIdType);
        }
        catch // product was not found at all
        {
            Console.WriteLine($"{someId} not found in backend, marking as void");
            return new(someId)
            {
                Manufactuer = null,
                ProductId = null,
                TradeId = null,
                VoidProduct = true,
                Skipped = true,
            };
        }
        await page.Locator(".absui-icon.absui-icon--keyboard-arrow-right2").ClickAsync();
        await page.GetByText("Atrybuty").First.ClickAsync();
        string productID = await page.Locator("//tr[td[normalize-space()='Indeks katalogowy']]/td[position()=3]").TextContentAsync();
        string tradeID = await page.Locator("//tr[td[normalize-space()='Indeks handlowy']]/td[position()=3]").TextContentAsync();
        string manufactuer = await page.Locator("//tr[td[normalize-space()='Producent']]/td[position()=3]").TextContentAsync();
        Product product = new(someId)
        {
            Manufactuer = string.Join(' ', manufactuer.Split(' ').Take(2)),
            ProductId = productID,
            TradeId = tradeID
        };
        Console.WriteLine($"Data found: {product}, Manufacturer: {product.Manufactuer}");

        return product;
    }
    enum SearchBy
    {
        TRADEID,
        PRODUCTID,
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
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                return SearchBy.TRADEID;
            }
            catch
            {   //if not found, try search by productID
                await page.GotoAsync($"{config["backend"]}?indexCatalogue={urlEncodedId}");
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
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                return SearchBy.PRODUCTID;
            }
            catch
            {   //if not found, try search by tradeID
                await page.GotoAsync($"{config["backend"]}?tradeIndex={urlEncodedId}");
                await page.Locator("td:nth-child(3)").First.ClickAsync(new() { Timeout = 5000 });
                Console.WriteLine("Product was being searching by ProductId, but was found by TradeId, switching further searches");
                return SearchBy.TRADEID;
            }
        }
        else throw new Exception("How ? If new type of search id was added it must also be implemented here!");
    }
}