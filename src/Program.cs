using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using TickerQ.DependencyInjection;

using JADE.Backend;
using JADE.Utility;
using JADE.models;

namespace JADE;

public partial class Program
{
    static readonly string configFileName = "JadeConfig.json";
    static Config config = null!;
    public static async Task<int> Main(string[] args)
    {
        // var webAppBuilder = WebApplication.CreateBuilder();
        // webAppBuilder.Services.AddTickerQ();
        // var webApp = webAppBuilder.Build();
        // webApp.UseTickerQ();
        // webApp.Run();

        using var factory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger logger = factory.CreateLogger("JADE");
        config = GetConfig(logger);
        var (usrMsg, sysMsg) = SelectLanguage(Lang.GetAllLanguages(), config.Language, "EN");
        logger.LogInformation(sysMsg.SysMsg("program-begin-init"));

        using IPlaywright playwright = await Playwright.CreateAsync();

        await using IBrowserContext context = await playwright.Chromium.LaunchPersistentContextAsync
        (ResourcesIO.GetPath(config, config.BrowserDataDir),
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
            UserAgent = config.UserAgent,

        });

        await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', { get: () => undefined })");
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        await page.GotoAsync("about:blank");
        BackendNavigation navigate = new(page, config, logger, usrMsg);
        string filePath = ResourcesIO.GetPath(config, config.SaveFile);

        var manufacturers = LoadManufacturers(page, config, logger);

        logger.LogInformation("Initialisation complete");
        //Phase 1 - load products from local storage and from backend
        List<Product> products = await BackendFetchAsync(navigate, logger);

        //Save 1 - save product manufac, tradeId and productId info to local storage
        string serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);

        //Phase 2 - fetch atributes from manufacturers
        products = await ManufacFetchAsync(products, manufacturers, config, logger);

        //Save 2 - save products atributes to local storage
        serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);


        //Phase 3 - send the product data to backend
        products = await BackendSaveAsync(products, navigate, logger);

        //Save 3 - save info about which products were implemented to local storage
        serializedProducts = JsonSerializer.Serialize(products);
        await File.WriteAllTextAsync(filePath, serializedProducts);

        logger.LogInformation(sysMsg.SysMsg("program-exit-message"));
        return 0;
    }


    //two accepted sources
    //1st: included precompiled -> put in plugins directory
    //2nd: compiled separately in plugins directory

    //two modes
    //1st: default mode, use precompiled units only
    //2nd: work with, separately compiled units
    public static Dictionary<string, Manufacturer> LoadManufacturers(IPage page, Config config, ILogger logger)
    {
        logger.LogInformation("Loading manufacturer assemblies");

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
                    logger.LogError($"Failed to load dll file, reason: {e.Message}");
                }
            }
        }
        foreach (var type in derivedTypes)
        {
            try
            {
                var manufac = Activator.CreateInstance(type, [page, config, logger]);
                if (manufac is Manufacturer)
                {
                    var prod = manufac as Manufacturer;
                    var names = prod!.Names.Where(x => x != string.Empty && x != "EXAMPLE") ?? [];

                    foreach (var name in names)
                    {
                        manufacturers.Add(name, prod!);
                        logger.LogInformation($"Manufacturer {name} assembly loaded");
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Manufacturer  assembly  not loaded, reason : {e.Message}");
            }
        }
        return manufacturers;
    }

    public static Config GetConfig(ILogger logger)
    {
        IConfiguration _config = new ConfigurationBuilder()
                                           .AddJsonFile(configFileName, optional: true)
                                           .AddUserSecrets<Program>().Build();
        Config config = _config.Get<Config>() ?? new();
        if (!File.Exists(configFileName))
        {
            if (ResourcesIO.GenericSave(config, "JadeConfig.json", logger))
                logger.LogCritical("No config file found! Created new example config, exiting.");
            Environment.Exit(0);
        }
        var errors = Validate(config);
        if (errors.Count() > 0)
        {
            foreach (var error in errors)
                logger.LogCritical(error);
            Environment.Exit(1);
        }
        return config;
    }

    public static IEnumerable<string> Validate(Config config)
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
        Lang sysMsg = langs.ContainsKey(textLangCode) ? langs[errorLangCode] : new Lang(new());
        return (usrMsg, sysMsg);
    }
}