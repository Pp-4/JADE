using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

using PlaywrightSharp.models;
using PlaywrightSharp.Backend;
using System.Reflection;

namespace PlaywrightSharp;

public partial class Program
{
    static readonly string configFileName = "JadeConfig.json";
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Begin initialisation");
        IConfiguration config;
        try
        {
            config = new ConfigurationBuilder()
                .AddJsonFile(configFileName, optional: false)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"browserDir","playwright-user-data"},
                    {"imageLImit", "3"}
                })
                .AddUserSecrets<Program>()
                .Build();
        }
        catch
        {
            Console.WriteLine($"Config file ({configFileName}) not found");
            Console.WriteLine($"While this file may be empty it must be created by user for reasons xd");
            return -1;
        }
        using IPlaywright playwright = await Playwright.CreateAsync();

        await using IBrowserContext context = await playwright.Chromium.LaunchPersistentContextAsync
        (config["browserDir"],
        new()
        {
            Channel = "chromium",
            Headless = false,
            Args =
        [
            "--disable-web-security",
                "--disable-features=IsolateOrigins,site-per-process"
        ],
            ViewportSize = new ViewportSize() { Height = 1080, Width = 1920 },
            ExtraHTTPHeaders = new Dictionary<string, string> {
                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8" },
                { "Accept-Language", "pl-PL,en-US,en;q=0.5" },
                { "Connection", "keep-alive"} },
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
        });

        await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', { get: () => undefined })");
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        await page.GotoAsync("about:blank");
        BackendNavigation navigate = new(page, config);
        string filePath = Path.Combine(config["data"], config["prodData"]);

        var manufacturers = LoadManufacturers(page, config);

        Console.WriteLine("Initialisation complete");

        //Phase 1 - load products from local storage and from backend
        List<Product> products = await BackendFetchAsync(navigate);

        //Save 1 - save product manufac, tradeId and productId info to local storage
        string serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);

        //Phase 2 - fetch atributes from manufacturers
        products = await ManufacFetchAsync(products, manufacturers, config);

        //Save 2 - save products atributes to local storage
        serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);


        //Phase 3 - send the product data to backend
        products = await BackendSaveAsync(products, navigate);

        //Save 3 - save info about which products were implemented to local storage
        serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);

        Console.WriteLine("Exiting");
        return 1;
    }


    //two accepted sources
    //1st: included precompiled -> put in plugins directory
    //2nd: compiled separately in plugins directory

    //two modes
    //1st: default mode, use precompiled units only
    //2nd: work with, separately compiled units
    public static Dictionary<string, Manufacturer> LoadManufacturers(IPage page, IConfiguration config)
    {
        Console.WriteLine("Loading manufacturer assemblies");
        //get type of the base class
        var baseType = typeof(Manufacturer);

        //load internal manufacturer assemblies
        //get the assembly of the base class
        var assembly = Assembly.GetAssembly(baseType);
        //get list of types from which baseclass can be assigned
        //ie get all derived classes
        var derivedTypes = assembly
        .GetTypes()
        .Where(t => t != baseType && baseType.IsAssignableFrom(t))
        .ToList();

        //load external manufacturer assemblies
        string pluginDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (Directory.Exists(pluginDir))
        {
            var dlls = Directory.GetFiles(pluginDir, "*.dll");
            foreach (var dll in dlls)
            {
                try
                {
                    var plugin = Assembly.LoadFile(dll);
                    var types = plugin
                                .GetTypes()
                                .Where(t => t != baseType && baseType.IsAssignableFrom(t));
                    derivedTypes.AddRange(types);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load dll file, reason: {e.Message}");
                }
            }
        }


        Dictionary<string, Manufacturer> manufacturers = [];
        foreach (var type in derivedTypes)
        {
            var manufac = Activator.CreateInstance(type, [page, config]);
            if (manufac is Manufacturer)
            {
                Manufacturer temp = manufac as Manufacturer;
                foreach (var name in temp.Names)
                {
                    if (name != string.Empty && name != "EXAMPLE")
                    {
                        manufacturers.Add(name, temp);
                        Console.WriteLine($"Manufacturer {name} assembly loaded");
                    }
                }
            }
        }

        return manufacturers;
    }
}