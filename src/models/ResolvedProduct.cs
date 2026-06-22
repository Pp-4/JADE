using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JADE.models;

public class ResolvedProduct(string _ProductId) : IEquatable<ResolvedProduct>
{
    [Key]
    public string ProductId { get; set; } = _ProductId;

    public string? TradeId { get; set; }

    public ICollection<Prop> Properties { get; set; } = [];
    public bool Equals(ResolvedProduct? other) => other is not null && ProductId == other.ProductId;
    public override int GetHashCode() => ProductId.GetHashCode();
}