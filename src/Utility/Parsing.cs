using System.Text.RegularExpressions;
using JADE.models;
using JADE.RegularEx;

namespace JADE.Utility;

public static class Parsing
{
    //TODO, add more SI system units
    public static string FixSiSize(string siUnit)
    {
        return siUnit.ToLower() switch
        {
            //distance
            "mm" or "m" or "dm" or "km" => siUnit,
            //area, volume
            "mm2" or "mm3" or "cm2" or "cm3" or "dm2" or "dm3" or "m2" or "m3" or "km2" or "km3" or "l" => siUnit,
            //mass
            "mcg" or "mg" or "g" or "dkg" or "kg" => siUnit,
            //time
            "h" or "min" or "s" or "ms" or "µs" => siUnit,
            //various
            "lm" or "lx" or "lux" or "mol" => siUnit,
            "m/s" or "m/s2" or "kg/m2" or "kg/m3" or "m2/kg" or "m3/kg" => siUnit,
            //frequency
            "hz" or "khz" or "mhz" or "ghz" or "thz" => Capitalize(siUnit, siUnit.Length - 1),
            "dba" or "db(a)" => "dBA",
            //energy
            "kw" or "mw" or "gw" or "tw" or "ka" or "ma" or "ga" or "kj" or "mj" or "gj" or "tj" => Socialise(siUnit),
            _ => siUnit.ToUpper(),
        };
    }
    //first N letters to upper, rest to lower
    public static string Capitalize(string str, int firstNCharacters = 1)
    {
        str = str.Trim();
        return str == string.Empty ? str : $"{str[..firstNCharacters].ToUpper()}{str[firstNCharacters..].ToLower()}"; ;
    }
    //first letter to lower, rest to upper
    public static string Socialise(string str)
    {
        str = str.Trim();
        return str == string.Empty ? str : $"{char.ToLowerInvariant(str[0])}{str[1..].ToString().ToUpper()}"; ;
    }
    /// <summary>
    /// return the number from the path ie. example/ex/img_123.jpg returns 123
    /// non valid string will return 0
    /// </summary>
    /// <returns>number from string</returns>
    public static int GetFileNumber(string nbr)
    {
        string temp = System.IO.Path.GetFileNameWithoutExtension(nbr).Split('_')[^1];
        int.TryParse(temp, out int ret);
        return ret;
    }    //correct atribiutes
    public static Prop Sanitize(Prop prop)
    {
        string Key = Capitalize(prop.Key);
        Key = Key.Replace("<", "&lt;").Replace(">", "&gt;");

        string Value = prop.Value
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;").Replace("…"," do ");
        if (Value.ToLower() == "no")
            Value = "Nie";

        Match match = RegExpressions.GetTextInBrackets().Match(Key);
        if (match.Success)
        {
            //turn something like this:
            //size [mm] : 13
            //into this:
            //size : 13 mm
            Key = Key.Replace(match.Groups[0].Value, "");
            Value = $"{Value} {FixSiSize(match.Groups[1].Value)}";
        }
        Key = Key.Replace(" ip", " IP").Replace(" ik", " IK");
        Value = Value.Replace(" ip", " IP").Replace(" ik", " IK");
        return new Prop(Key, Value);
    }
}