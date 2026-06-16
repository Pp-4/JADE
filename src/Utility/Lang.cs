using System.Collections.Generic;

using JADE.models;

namespace JADE.Utility;

public class Lang(LangRecord data)
{
    private readonly LangRecord _data = data;
    public string Name => _data.Name;
    public string Code => _data.Code;
    //return key if value not found
    public string UsrMsg(string key) => _data.UserMessages.TryGetValue(key, out string? value) ? value : key;
    public string SysMsg(string key) => _data.SystemMessages.TryGetValue(key, out string? value) ? value : key;
    public static Dictionary<string, Lang> GetAllLanguages() => ResourcesIO.LoadLangFiles();
}