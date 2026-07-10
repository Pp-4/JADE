using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using JADE.RegularEx;


namespace JADE.models;

public class Product
{
    public Product(string ProductId) => this.ProductId = ProductId;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ProductId { get; set; } = null!;
    public List<Prop> RawDescription { get; set; } = [];
    [ForeignKey("Manufactrurer")]
    public string? TradeId { get; set; }
    public int ManufactrurerId { get; set; } = 0;
    public int SkipCount { get; set; } = 0;         //how many times was product skipped
    public bool Skipped { get; set; } = false;      //was product skipped at any loop
    public bool Implemented { get; set; } = false;  //was product data saved back to backend ?
    public bool Void { get; set; } = false;         //flag product as not found in backend
    public bool ForceImpl { get; set; } = false;    //force reimplemntation flag (ignores Implemented flag, only set for testing on invidual products, rewrites backend description)

    [NotMapped]
    public Manufacturer? manufacturerObject { get; set; }
    [NotMapped]
    public string? ShortTradeId => GenerateShortTradeId();

    public string Manufacturer { get; set; } = "unknown";
    public override string ToString() => $"{ProductId} / {TradeId}";
    public override bool Equals(object? obj) => obj is Product other && other.ProductId == ProductId;
    public override int GetHashCode() => ProductId.GetHashCode();

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

    public Product MarkAsImplemented()
    {
        this.SkipCount = 0;
        this.Skipped = false;
        this.Implemented = true;
        this.Void = false;
        return this;
    }
    public Product MarkAsVoid()
    {
        this.Skipped = true;
        this.Void = true;
        this.Implemented = false;
        this.ManufactrurerId = 0;
        this.TradeId = null;
        return this;
    }
}