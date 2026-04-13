namespace PlaywrightSharp.Utility;

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
            "mg" or "g" or "dkg" or "kg" => siUnit,
            //time
            "h" or "min" or "s" => siUnit,
            //various
            "lm" or "lx" or "lux" or "mol" => siUnit,
            "m/s" or "m/s2" or "kg/m2" or "kg/m3" or "m2/kg" or "m3/kg" => siUnit,
            //frequency
            "hz" or "khz" or "mhz" or "ghz" or "thz" => Capitalize(siUnit, siUnit.Length - 1),
            "dba" or "db(a)" => "dBA",
            "kw" or  "mw" or "gw" or "tw" or "ka" or "ma" or "ga" or "kj" or "mj" or "gj" or "tj" => Socialise(siUnit),
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

}