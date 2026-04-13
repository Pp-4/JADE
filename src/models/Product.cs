using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using PlaywrightSharp.RegularEx;
using static PlaywrightSharp.Utility.Parsing;

namespace PlaywrightSharp.models;

public class Product(string someid)
{
#nullable enable
    public string? TradeId { get; set; }
    public string? ProductId { get; set; }
    //Initial id obtained during Phase 1
    public string SomeId { get; set; } = someid;
    [JsonIgnore]
    public string Description { get { return WrapDescriptionInHtml(); } }
    public List<Prop>? RawDescription { get; set; }
    //product manufacturer
    public string? Manufactuer { get; set; }
    [JsonIgnore]
    public Manufacturer? manufactuerObject;
    //cleaned up version of tradeId
    public string? ShortTradeId { get { return GenerateShortTradeId(); } }
    //was product skipped at any loop
    public bool Skipped { get; set; }

    //was product data saved back to backend ?
    public bool Implemented { get; set; }

    //flag product as not found in backend
    public bool VoidProduct { get; set; }
    //force reimplemntation flag (ignores Implemented flag, only set for testing on invidual products)
    //it will also overwite the description every time
    public bool ForceImpl { get; set; } = false;
    public string WrapDescriptionInHtml()
    {
        if (RawDescription is null)
            return string.Empty;
        else
        {
            string ret = "<div class=\"opis_na_stronie_produktu-tabela\"><div class=\"tytul\">Opis</div><table>\n";
            if (!RawDescription.Any(x => x.Key == " Indeks handlowy"))
            {
                ret += $"<tr><td>Indeks handlowy </td><td><b>{TradeId}</b></td></tr>\n";
            }
            foreach (var item in RawDescription)
            {
                if (PassFiler(item))
                {
                    Prop prop = Sanitize(item);
                    ret += $"<tr><td>{prop.Key}</td><td><b>{prop.Value}</b></td></tr>\n";
                }
            }
            ret += "</table></div>";
            return ret;
        }
    }

    public override string ToString()
    {
        return $"{ProductId} / {TradeId}";
    }
    private string GenerateShortTradeId()
    {
        if (TradeId is not null)
        {
            Match match = RegExpressions.GetShortTradeId().Match(TradeId);
            if (match.Success)
                return match.Groups[1].Value;
        }
        return "";
    }

    //filter out bad atribiutes
    private bool PassFiler(Prop prop)
    {
        if (TradeId is not null && prop.Value.Contains(TradeId) && !prop.Key.ToLower().Contains("ean")) return false;
        if (ProductId is not null && prop.Value.Contains(ProductId)) return false;
        if (prop.Key.ToLower().Contains("gwara")) return false;
        if (manufactuerObject is not null)
        {
            if (manufactuerObject.FilterKeys.Any(x => prop.Key.ToLower().Contains(x)))
                return false;
        }
        return true;
    }
    //correct atribiutes
    private Prop Sanitize(Prop prop)
    {
        string Key = Capitalize(prop.Key);
        Key = Key.Replace("<", "&lt;").Replace(">", "&gt;");

        string Value = prop.Value.Replace("<", "&lt;").Replace(">", "&gt;");
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
        return new(Key, Value);
    }
    //TODO group atributes into sections or smth
    private List<Prop> GroupSort(List<Prop> list)
    {
        return list;
    }
}