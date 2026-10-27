namespace WebApplication1;

public sealed record Product(
    int Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    string Emoji,
    string ColorClass,
    string? ImageUrl = null,
    string Author = "",
    string Publisher = "",
    int PublicationYear = 0,
    string Isbn = "",
    int PageCount = 0)
{ public int? CategoryId { get; init; } }



