using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;

using JADE.models;

namespace JADE.Utility;

public class Lang(LangRecord data)
{
    private readonly LangRecord _data = data;
    public string Name => _data.Name;
    //return key if value not found
    public string Text(string key) => _data.Texts.TryGetValue(key, out string? value) ? value : key;
    public string Error(string key) => _data.Errors.TryGetValue(key, out string? value) ? value : key;

    public static IEnumerable<Lang> GetAllLanguages()
    {
        var a = LoadLangFiles();
        foreach (var b in a)
            if (b.Name != string.Empty)
                yield return new Lang(b);
    }

    static List<LangRecord> LoadLangFiles(string path = "LangData")
    {
        List<LangRecord> languages = [];
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var binary = File.ReadAllBytes(file);
                languages.Add(JsonSerializer.Deserialize<LangRecord>(binary) ?? new LangRecord());
            }
            return languages.Count() > 0 ? languages : [new LangRecord()];
        }
        else
            return [new LangRecord()];
    }
}