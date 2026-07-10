using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JADE.models;

namespace JADE.Learning;


public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ProductId { get; set; } = null!;
    [ForeignKey("Manufactrurer")]
    public int ManufactrurerId { get; set; }
    public string? TradeId { get; set; }
    public string? Description { get; set; } = null;
    [NotMapped]
    public List<Prop>? RawDescription { get; set; }

    //flags
    public bool Skipped { get; set; }
    public bool Void { get; set; }
    public bool Implemented { get; set; }
    public bool ForceImpl { get; set; }
    //flags
    public override bool Equals(object? obj) => obj is Product other && other.ProductId == ProductId;
    public override int GetHashCode() => ProductId.GetHashCode();
    public override string ToString() => $"{ProductId} / {TradeId}";
}

public static class ProductMarks
{
    extension(Product product)
    {
        public Product MarkAsImplemented()
        {
            product.Skipped = false;
            product.Implemented = true;
            product.Void = false;
            return product;
        }
        public Product MarkAsVoid()
        {
            product.Skipped = true;
            product.Void = true;
            product.Implemented = false;
            product.TradeId = null;
            return product;
        }
        public bool HasBasicInfo() => !(string.IsNullOrEmpty(product.ProductId)
                            || string.IsNullOrEmpty(product.TradeId));
                            //|| string.IsNullOrEmpty(product.ManufactrurerId));
    }
}