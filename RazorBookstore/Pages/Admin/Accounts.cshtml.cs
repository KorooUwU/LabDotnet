using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin;

public class AccountsModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }
    public List<ApplicationUser> Items { get; private set; } = [];
    public Dictionary<string, string[]> AccountRoles { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(x => x.UserName!.Contains(search) || x.FullName.Contains(search) || x.Email!.Contains(search));
        }
        Items = await query.OrderBy(x => x.UserName).ToListAsync();
        var ids = Items.Select(x => x.Id).ToArray();
        var assignments = await (from ur in db.UserRoles
                                 join role in db.Roles on ur.RoleId equals role.Id
                                 where ids.Contains(ur.UserId)
                                 select new { ur.UserId, role.Name }).ToListAsync();
        AccountRoles = assignments.GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name!).ToArray());
    }
}

