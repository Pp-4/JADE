using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using JADE.models;

namespace JADE.Learning;

public class Manufactrurer
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string WebAddres { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = [];

}