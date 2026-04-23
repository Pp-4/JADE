using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using JADE.RegularEx;
using JADE.Utility;
using System;

namespace JADE.models;

public class Product(string? someid)
{
#nullable enable
    public string? TradeId { get; set; }
    public string? ProductId { get; set; }
    //Initial id obtained during Phase 1
    public string? SomeId { get; set; } = someid;
    [JsonIgnore]
    public string Description { get { return WrapDescriptionInHtml(); } }
    public List<Prop>? RawDescription { get; set; }
    //product manufacturer
    public string? Manufacturer { get; set; }
    [JsonIgnore]
    public Manufacturer? manufactuerObject;
    //cleaned up version of tradeId
    public string? ShortTradeId { get { return GenerateShortTradeId(); } }
    //was product skipped at any loop
    public bool Skipped { get; set; }
    //how many times was product skipped
    public int SkipCount { get; set; } = 0;

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
                    Prop prop = Parsing.Sanitize(item);
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
            {
                string ShortTradeId = match.Groups[1].Value;
                if (ShortTradeId.Length < 4 && TradeId.Count(x => x == '\\') == 1)
                    return TradeId;
                return ShortTradeId;
            }
            return TradeId;
        }
        return "";
    }

    //filter out bad atribiutes
    private bool PassFiler(Prop prop)
    {
        if (prop.Value is not null)
        {
            if (TradeId is not null && prop.Value.Contains(TradeId) && !prop.Key.ToLower().Contains("ean")) return false;
            if (ProductId is not null && prop.Value.Contains(ProductId)) return false;
        }
        if (prop.Key.ToLower().Contains("gwara")) return false;
        if (manufactuerObject is not null)
        {
            if (manufactuerObject.FilterKeys.Any(x => prop.Key.ToLower().Contains(x)))
                return false;
        }
        return true;
    }
    //TODO group atributes into sections or smth
    private List<Prop> GroupSort(List<Prop> list)
    {
        return list;
    }

    public bool Equals(Product? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (this.SomeId is not null && other.SomeId is not null)
            return this.SomeId == other.SomeId;
        if (this.SomeId is null && other.SomeId is null)
            return this.ProductId == other.ProductId;
        if (this.SomeId is null && other.SomeId is not null)
            return this.ProductId == other.SomeId || this.TradeId == other.SomeId || this.ShortTradeId == other.SomeId;
        return this.SomeId == other.ProductId || this.SomeId == other.TradeId || this.SomeId == other.ShortTradeId;
    }

    public Product MarkAsImplemented()
    {
        this.SkipCount = 0;
        this.Skipped = false;
        this.Implemented = true;
        this.VoidProduct = false;
        return this;
    }
    public Product MarkAsVoid()
    {
        this.Skipped = true;
        this.VoidProduct = true;
        this.Implemented = false;
        this.Manufacturer = null;
        this.ProductId = null;
        this.TradeId = null;
        return this;
    }
    public Product Resolve(string ProductId, string TradeId, string Manufacturer)
    {
        this.ProductId = ProductId;
        this.TradeId = TradeId;
        this.Manufacturer = Manufacturer;
        this.Implemented = false;
        this.VoidProduct = false;
        this.SomeId = null;
        return this;
    }
    public Product MergeProduct(Product other)
    {
        this.SomeId ??= other.SomeId;
        this.ProductId ??= other.ProductId;
        this.TradeId ??= other.TradeId;
        this.Manufacturer ??= other.Manufacturer;
        this.manufactuerObject ??= other.manufactuerObject;
        this.RawDescription ??= other.RawDescription;
        this.ForceImpl |= other.ForceImpl;
        this.VoidProduct |= other.VoidProduct;
        this.Implemented |= other.Implemented;
        this.Skipped |= other.Skipped;
        this.SkipCount = this.SkipCount > other.SkipCount ? this.SkipCount : other.SkipCount;
        if (this.ProductId is not null) this.SomeId = null;
        return this;
    }
}