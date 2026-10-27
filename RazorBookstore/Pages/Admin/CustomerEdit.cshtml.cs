using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1.Pages.Admin;

public class CustomerEditModel(AppDbContext db, UserManager<ApplicationUser> users) : PageModel
{
    [BindProperty] public CustomerInput Input { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(int id = 0) { if (id == 0) return Page(); var x = await db.Customers.FindAsync(id); if (x is null) return NotFound(); Input = new() { Id = x.Id, Name = x.Name, Email = x.Email, Phone = x.Phone, Address = x.Address }; return Page(); }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page(); if (Input.Id != 0 && !await db.Customers.AnyAsync(x => x.Id == Input.Id)) return NotFound(); if (await db.Customers.AnyAsync(x => x.Id != Input.Id && x.Email == Input.Email) || await db.Users.AnyAsync(x => x.CustomerId != Input.Id && x.Email == Input.Email)) { ModelState.AddModelError("", "Email đã tồn tại."); return Page(); }
        await using var tx = await db.Database.BeginTransactionAsync();
        var customer = new Customer(Input.Id, Input.Name.Trim(), Input.Email.Trim(), Input.Phone.Trim(), Input.Address?.Trim() ?? ""); if (Input.Id == 0) db.Customers.Add(customer); else db.Customers.Update(customer);
        var account = await db.Users.FirstOrDefaultAsync(x => x.CustomerId == Input.Id);
        if (account is not null) { account.FullName = customer.Name; account.Email = customer.Email; account.PhoneNumber = customer.Phone; var result = await users.UpdateAsync(account); if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return Page(); } await users.UpdateSecurityStampAsync(account); }
        await db.SaveChangesAsync(); await tx.CommitAsync(); TempData["Message"] = "Đã lưu khách hàng."; return RedirectToPage("Customers");
    }
}

