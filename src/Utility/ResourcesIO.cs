using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

using Microsoft.Extensions.Logging;

using JADE.models;

namespace JADE.Utility;

public static class ResourcesIO
{

    //private static Assembly[] AllAssembliesOfCurrentAppDomain => System.AppDomain.CurrentDomain.GetAssemblies();
    internal static string LoadResource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        //list all resources in current assembly
        //Assembly.GetExecutingAssembly().GetManifestResourceNames();
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream is not null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        else throw new FileNotFoundException($"{name} resource not found!");
    }
    public static async Task<List<Product>> LoadProductsFromFile(string filePath, ILogger logger)
    {
        List<Product> products = [];

        //string filePath = Path.Combine(dataPath, dataStorageLocation);
        if (File.Exists(filePath))
        {
            logger.LogInformation($"Loading product data from {filePath}");
            string jsonContent = File.ReadAllText(filePath);
            if (jsonContent.Length == 0)
                return products;
            try
            {
                var deserialized = JsonSerializer.Deserialize<IEnumerable<Product>>(jsonContent);
                if (deserialized is not null)
                    products.AddRange(deserialized);
                else
                    logger.LogWarning($"No products loaded, skipping");
            }
            catch (JsonException e)
            {
                logger.LogError($"Error when loading local product data: {e.Message}, skipping");
                return products;
            }
        }
        else
            logger.LogError("Couldn't load product data, missing save file path!");
        return products;
    }
    public static IEnumerable<string> LoadSomeIDFromFile(string filePath, ILogger logger)
    {
        logger.LogInformation($"Loading data from {filePath}");
        string content = File.ReadAllText(filePath);
        return content.Split('\n').Where(x => x.First() != '#');
    }
    public static async Task<bool> SaveProductsToFile(List<Product> products, string filePath, ILogger logger)
    {
        if (filePath is not null)
        {
            logger.LogInformation($"Saving product data to {filePath}");
            string content = JsonSerializer.Serialize(products);
            await File.WriteAllTextAsync(filePath, content);
            logger.LogInformation("Product data saved.");
            return true;
        }
        else
        {
            logger.LogError("Couldn't save product data, missing save file path!");
            return false;
        }
    }

    public static bool GenericSave(object data, string fileName, ILogger logger)
    {
        logger.LogInformation($"Saving {fileName} ...");
        try
        {
            string content = JsonSerializer.Serialize(data);
            File.WriteAllText(fileName, content);
            logger.LogInformation($"{fileName} saved");
            return true;
        }
        catch (Exception e)
        {
            logger.LogError($"Couldn't save {fileName}! Reason: {e.Message}");
            return false;
        }
    }
    /// <summary>
    /// Points to file or directory in data dir
    /// </summary>
    /// <param name="config"></param>
    /// <param name="DirOrFile"></param>
    /// <returns></returns>
    public static string GetPath(Config config, string DirOrFile)
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, config.DataDir, DirOrFile));
    }
    internal static Dictionary<string, Lang> LoadLangFiles(string path = "LangData/")
    {
        Dictionary<string, Lang> languages = [];
        path = Path.Combine(AppContext.BaseDirectory,path);
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var binary = File.ReadAllBytes(file);
                var lang = JsonSerializer.Deserialize<LangRecord>(binary) ?? new LangRecord();
                languages.TryAdd(lang.Code, new(lang));
            }
        }
        if (languages.Keys.Count < 1)
            languages.Add("default", new Lang(new LangRecord()));
        return languages;
    }
}