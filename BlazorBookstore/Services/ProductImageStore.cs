namespace WebApplication1;
public sealed class ProductImageStore(IWebHostEnvironment environment)
{
    public const int MaxBytes = 2 * 1024 * 1024;
    private string DirectoryPath => ProductImages.Root(environment.ContentRootPath);
    public static (string Extension, string ContentType) Detect(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes.AsSpan(0,8).SequenceEqual(new byte[] { 137,80,78,71,13,10,26,10 })) return (".png", "image/png");
        if (bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255) return (".jpg", "image/jpeg");
        if (bytes.Length >= 12 && System.Text.Encoding.ASCII.GetString(bytes,0,4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes,8,4) == "WEBP") return (".webp", "image/webp");
        throw new InvalidDataException("Vui lòng chọn ảnh JPG, PNG hoặc WebP hợp lệ.");
    }
    public string Save(byte[] bytes)
    {
        if (bytes.Length == 0 || bytes.Length > MaxBytes) throw new InvalidDataException("Ảnh phải nhỏ hơn hoặc bằng 2 MB.");
        var (extension, _) = Detect(bytes);
        Directory.CreateDirectory(DirectoryPath);
        var filename = Guid.NewGuid().ToString("N") + extension;
        File.WriteAllBytes(Path.Combine(DirectoryPath,filename),bytes);
        return "/product-images/" + filename;
    }
    public string? Resolve(string filename)
    {
        if (!Guid.TryParseExact(Path.GetFileNameWithoutExtension(filename), "N", out _) || Path.GetFileName(filename) != filename || Path.GetExtension(filename) is not (".png" or ".jpg" or ".webp")) return null;
        var path = Path.Combine(DirectoryPath, filename);
        return File.Exists(path) ? path : null;
    }
    // Only rollback a freshly uploaded image if saving the product fails.
    public void RemoveNew(string url)
    {
        var path = Resolve(url.Split('/').Last());
        if (path is not null) File.Delete(path);
    }
}

