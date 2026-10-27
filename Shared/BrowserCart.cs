using System.Text.Json;
using Microsoft.AspNetCore.Http;
namespace WebApplication1;
// Chỉ mã hóa trạng thái Session; không truy cập dữ liệu hay xử lý nghiệp vụ.
public static class BrowserCart
{
    public static Dictionary<int, int> Read(ISession session) { try { return JsonSerializer.Deserialize<Dictionary<int, int>>(session.GetString("cart") ?? "{}")?.Where(x => x.Key > 0 && x.Value > 0 && x.Value <= 999).ToDictionary(x => x.Key, x => x.Value) ?? []; } catch (JsonException) { return []; } }
    public static void Write(ISession session, Dictionary<int, int> items) => session.SetString("cart", JsonSerializer.Serialize(items));
}

