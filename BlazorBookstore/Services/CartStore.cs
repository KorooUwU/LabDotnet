using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using System.Text.Json;
using Microsoft.JSInterop;
namespace WebApplication1;

// Scoped to a Blazor circuit; persistent data belongs to this browser tab.
public sealed class CartStore(IJSRuntime js, IDbContextFactory<AppDbContext> factory)
{
    private IReadOnlyList<Product> Products { get {using var db=factory.CreateDbContext();return db.Products.AsNoTracking().ToArray();} }
    private Dictionary<int, int> _items = [];
    private bool _loaded;
    private Task? _loadTask;
    public event Action? Changed;
    public int Count => Lines.Sum(x => x.Quantity);
    public string? Error { get; private set; }
    public IReadOnlyList<CartLine> Lines => Products.Where(p => _items.ContainsKey(p.Id)).Select(p => new CartLine(p, _items[p.Id])).ToArray();
    public Task LoadAsync() => _loadTask ??= LoadCoreAsync();
    private async Task LoadCoreAsync()
    {
        if (_loaded) return;
        try
        {
            var json = await js.InvokeAsync<string?>("mocCart.read");
            var values = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Dictionary<int, int>>(json);
            _items = values?.Where(x => x.Key > 0 && x.Value > 0 && x.Value <= 999).ToDictionary(x => x.Key, x => x.Value) ?? [];
        }
        catch (JsonException) { _items = []; }
        catch (JSException) { Error = "Trình duyệt không cho phép lưu giỏ hàng. Giỏ hàng chỉ được giữ trong lần truy cập này."; }
        _loaded = true; Changed?.Invoke();
    }
    public async Task AddAsync(int id)
    {
        await LoadAsync();
        if (!Products.Any(p => p.Id == id)) return;
        var count = _items.GetValueOrDefault(id);
        if (count < 999) _items[id] = count + 1;
        await PersistAsync();
    }
    public async Task RemoveAsync(int id) { await LoadAsync(); _items.Remove(id); await PersistAsync(); }
    public async Task ClearAsync() { await LoadAsync(); _items.Clear(); await PersistAsync(); }
    public async Task SetQuantityAsync(int id, int quantity) { await LoadAsync(); if (quantity < 1) _items.Remove(id); else if (quantity <= 999 && Products.Any(x => x.Id == id)) _items[id] = quantity; await PersistAsync(); }
    private async Task PersistAsync()
    {
        Changed?.Invoke();
        try { await js.InvokeVoidAsync("mocCart.write", JsonSerializer.Serialize(_items)); }
        catch (JSException) { Error = "Không thể lưu giỏ hàng trong trình duyệt. Giỏ hàng chỉ được giữ trong lần truy cập này."; }
    }
    public sealed record CartLine(Product Product, int Quantity) { public decimal Total => Product.Price * Quantity; }
}



