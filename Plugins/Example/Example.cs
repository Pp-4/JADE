using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

using PlaywrightSharp.models;

namespace Plugin.Example;

public class Example(IPage _page, IConfiguration config) : Manufacturer(_page, config)
{
    //manufacturer's webpage with catalog
    protected override string WebPage => "example.com";
    //list here attributes that are ought to be filtered out
    protected override List<string> FilterKeys => ["example"];
    //name of the js script that grabs product's attributes
    protected override string JsSnippet => "Example.js";
    //put here list of names by which the assembly will be recognized
    public override string[] Names => ["EXAMPLE"];
    //256kb default
    protected override int MaxImgSize => base.MaxImgSize;
    protected override async Task LocateProduct(Product product)
    {
        await page.WaitForLoadStateAsync();
        await page.PauseAsync();
        //put here code that can locate product page on the manufacturer's website
    }
    protected override async Task<List<string>> LocateImages(Product product)
    {
        //put here code that can locate image's src on the product's page
        return [];
        // you can yous this if they can be find by a locator
        return await SimpleLocateImages("DIV.example#locator example");
    }
}