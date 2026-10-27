using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace WebApplication1;

public static class ProductImages
{
    public static string Root(string contentRoot)
    {
        var directory = new DirectoryInfo(contentRoot); while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "Shared"))) directory = directory.Parent;
        return directory is null ? Path.Combine(contentRoot, "App_Data", "product-images") : Path.Combine(directory.FullName, "App_Data", "product-images");
    }
    public static async Task<string> SaveAsync(IFormFile file, string root)
    {
        if (file.Length < 1 || file.Length > 2 * 1024 * 1024) throw new InvalidDataException("Ảnh tối đa 2 MB.");
        using var memory = new MemoryStream(); await file.CopyToAsync(memory); var b = memory.ToArray();
        var ext = b.Length >= 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ? ".png" : b.Length >= 3 && b[0] == 255 && b[1] == 216 && b[2] == 255 ? ".jpg" : b.Length >= 12 && System.Text.Encoding.ASCII.GetString(b, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(b, 8, 4) == "WEBP" ? ".webp" : throw new InvalidDataException("Chỉ chấp nhận JPG, PNG hoặc WebP hợp lệ.");
        Directory.CreateDirectory(root); var name = Guid.NewGuid().ToString("N") + ext; await File.WriteAllBytesAsync(Path.Combine(root, name), b); return "/product-images/" + name;
    }
    public static void MapProductImages(this WebApplication app)
    {
        app.MapGet("/product-images/{name}", (string name) =>
        {
            if (Path.GetFileName(name) != name || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _) || Path.GetExtension(name) is not (".png" or ".jpg" or ".webp")) return Results.NotFound();
            var path = Path.Combine(Root(app.Environment.ContentRootPath), name);
            return File.Exists(path) ? Results.File(path, Path.GetExtension(path) switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" }) : Results.NotFound();
        });
    }
}

