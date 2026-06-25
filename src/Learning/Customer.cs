using System.Collections.Generic;

namespace JADE.Learning;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Adress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public ICollection<Order> Orders { get; set; }
}