using System.Linq;

using JADE.models;

namespace JADE.Utility;

public static class HtmlRendering
{
    public static string WrapDescriptionInHtml(Product product, Lang lang)
    {
        if (product.RawDescription is null)
            return string.Empty;
        else
        {
            string ret = $"<div class=\"opis_na_stronie_produktu-tabela\"><div class=\"tytul\">{lang.UsrMsg("product-description-label")}</div><table>\n";
            if (!product.RawDescription.Any(x => x.Key == $" {lang.UsrMsg("trade-index")}"))
            {
                ret += $"<tr><td>{lang.UsrMsg("trade-index")} </td><td><b>{product.TradeId}</b></td></tr>\n";
            }
            foreach (var item in product.RawDescription)
            {
                if (PassFilter(item, product))
                {
                    Prop prop = Parsing.Sanitize(item);
                    ret += $"<tr><td>{prop.Key}</td><td><b>{prop.Value}</b></td></tr>\n";
                }
            }
            ret += "</table></div>";
            return ret;
        }
    }

    private static bool PassFilter(Prop prop, Product product)
    {
        if (prop.Value is not null)
        {
            if (product.TradeId is not null && prop.Value.Contains(product.TradeId) && !prop.Key.ToLower().Contains("ean")) return false;
            if (product.ProductId is not null && prop.Value.Contains(product.ProductId)) return false;
        }
        if (prop.Key.ToLower().Contains("gwara")) return false;
        if (product.manufacturerObject is not null)
        {
            if (product.manufacturerObject.FilterKeys.Any(x => prop.Key.ToLower().Contains(x)))
                return false;
        }
        return true;
    }
}