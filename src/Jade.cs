using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

using JADE.Backend;
using JADE.Utility;
using JADE.models;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.EntityFrameworkCore.Customizer;

namespace JADE;

public partial class Jade
{
    const string configFileName = "JadeConfig.json";
    public readonly Config Config;
    readonly ILogger Logger;
    readonly Lang usrMsg;
    readonly Lang sysMsg;
    public Jade()
    {
        using var factory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Debug));
        Logger = factory.CreateLogger("JADE");
        Config = GetConfig();
        (usrMsg, sysMsg) = SelectLanguage(Lang.GetAllLanguages(), Config.Language, "PL");
        Logger.LogInformation(sysMsg.SysMsg("program-begin-init"));
    }

    public async Task<int> Start()
    {
        using IPlaywright playwright = await Playwright.CreateAsync();

        await using IBrowserContext context = await playwright.Chromium.LaunchPersistentContextAsync
        (ResourcesIO.GetPath(Config, Config.BrowserDataDir),
        new()
        {
            Channel = "chromium",
            Headless = false,
            Args =
        [
            "--disable-web-security",
                "--disable-features=IsolateOrigins,site-per-process"
        ],
            ViewportSize = new ViewportSize() { Height = 1016, Width = 1920 },
            ExtraHTTPHeaders = new Dictionary<string, string> {
                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8" },
                { "Accept-Language", "pl-PL,en-US,en;q=0.5" },
                { "Connection", "keep-alive"} },
            UserAgent = Config.UserAgent,

        });

        await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', { get: () => undefined })");
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        BackendNavigation navigate = new(page, Config, Logger, usrMsg);

        var manufacturers = LoadManufacturers(page);
        Logger.LogInformation(sysMsg.SysMsg("program-finish-init"));
        
        string filePath = ResourcesIO.GetPath(Config, Config.SaveFile);

        //Phase 1 - load products from local storage and from backend
        await page.GotoAsync("about:blank");
        List<Product> products = await BackendFetchAsync(navigate);

        //Save 1 - save product manufac, tradeId and productId info to local storage
        string serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);

        //Phase 2 - fetch atributes from manufacturers
        products = await ManufacFetchAsync(products, manufacturers);

        //Save 2 - save products atributes to local storage
        serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);


        //Phase 3 - send the product data to backend
        products = await BackendSaveAsync(products, navigate);

        //Save 3 - save info about which products were implemented to local storage
        serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);

        Logger.LogInformation(sysMsg.SysMsg("program-exit-message"));
        return 0;
    }
    public Dictionary<string, Manufacturer> LoadManufacturers(IPage page)
    {
        Logger.LogInformation("Loading manufacturer assemblies");

        Dictionary<string, Manufacturer> manufacturers = [];
        Type baseType = typeof(Manufacturer);
        Assembly? assembly = Assembly.GetAssembly(baseType) ?? throw new DllNotFoundException();

        //load internal manufacturer assemblies
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
                    Logger.LogError($"Failed to load dll file, reason: {e.Message}");
                }
            }
        }
        foreach (var type in derivedTypes)
        {
            try
            {
                var manufac = Activator.CreateInstance(type, [page, Config, Logger]);
                if (manufac is Manufacturer)
                {
                    var prod = manufac as Manufacturer;
                    var names = prod!.Names.Where(x => x != string.Empty && x != "EXAMPLE") ?? [];

                    foreach (var name in names)
                    {
                        manufacturers.Add(name, prod!);
                        Logger.LogInformation($"Manufacturer {name} assembly loaded");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Manufacturer  assembly  not loaded, reason : {e.Message}");
            }
        }
        return manufacturers;
    }

    public Config GetConfig()
    {
        IConfiguration _config = new ConfigurationBuilder()
                                           .AddJsonFile(configFileName, optional: true)
                                           .AddUserSecrets<Program>().Build();
        Config config = _config.Get<Config>() ?? new();
        if (!File.Exists(configFileName))
        {
            if (ResourcesIO.GenericSave(config, "JadeConfig.json", Logger))
                Logger.LogCritical("No config file found! Created new example config, exiting.");
            Environment.Exit(0);
        }
        //explicitly give config to Validate function, since global config object was not setup(not returned) yet
        var errors = Validate(config);
        if (errors.Count() > 0)
        {
            foreach (var error in errors)
                Logger.LogCritical(error);
            Environment.Exit(1);
        }
        return config;
    }

    static IEnumerable<string> Validate(Config config)
    {
        if (string.IsNullOrWhiteSpace(config.BackendAddress))
            yield return $"{nameof(config.BackendAddress)} must be set";
        else if (!Uri.TryCreate(config.BackendAddress, UriKind.Absolute, out _))
            yield return $"{nameof(config.BackendAddress)} is not a valid absolute URL";

        if (string.IsNullOrWhiteSpace(config.BackendUsername))
            yield return $"{nameof(config.BackendUsername)} must be set";
        if (string.IsNullOrWhiteSpace(config.BackendPassword))
            yield return $"{nameof(config.BackendPassword)} must be set";
        if (config.AddingImagesTimeout < 100)
            yield return $"{nameof(config.AddingImagesTimeout)} must be at least 100 ms";
    }
    //Return one language for user visible strings, and one for program logs
    public static (Lang usrMsg, Lang sysMsg) SelectLanguage(Dictionary<string, Lang> langs, string textLangCode, string errorLangCode)
    {
        Lang usrMsg = langs.ContainsKey(textLangCode) ? langs[textLangCode] : new Lang(new());
        Lang sysMsg = langs.ContainsKey(errorLangCode) ? langs[errorLangCode] : new Lang(new());
        return (usrMsg, sysMsg);
    }
}