using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebApplication1;
// Data Annotations configure the database model, not just the edit form.
[Table("Customers")]
public sealed record Customer(
    [property: Key] int Id,
    [property: Required, MaxLength(120)] string Name,
    [property: Required, MaxLength(200)] string Email,
    [property: Required, MaxLength(20)] string Phone,
    [property: MaxLength(300)] string Address);
