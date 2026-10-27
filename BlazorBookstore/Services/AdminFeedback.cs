namespace WebApplication1;
public sealed class AdminFeedback
{
    public string? Message { get; set; }
    public string? Take() { var value = Message; Message = null; return value; }
}
