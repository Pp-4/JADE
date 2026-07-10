using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JADE.Learning;

public enum Status
{
    NotYetResolved = 0,
    ResolvedAsProductId = 1,
    ResolvedAsTradeId = 2,
    NotPresentInBackend = 3,
}

public class Seen()
{
    public Seen(string id) : this()
    {
        SeenId = id;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string SeenId { get; set; } = null!;
    public Status Status { get; set; } = Status.NotYetResolved;
}