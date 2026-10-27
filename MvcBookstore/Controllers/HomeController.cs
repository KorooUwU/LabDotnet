using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Data;
using MvcBookstore.Models;
namespace MvcBookstore.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    [HttpGet("/"), HttpGet("/Index")]
    public async Task<IActionResult> Index(string? search, string? category) { var q = db.Products.AsNoTracking(); if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.Contains(search) || x.Author.Contains(search) || x.Isbn.Contains(search)); if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category); return View(new HomeModel { Search = search, Category = category, Books = await q.ToListAsync(), Categories = await db.Categories.AsNoTracking().ToListAsync() }); }
    [HttpGet("/Books/{id:int}")] public async Task<IActionResult> BookDetails(int id) { var book = await db.Products.FindAsync(id); return book is null ? NotFound() : View(new BookModel { Book = book }); }
    [HttpGet("/About")] public IActionResult About() => View();
    [HttpGet("/Support")] public IActionResult Support() => View();
    [HttpGet("/Contact")] public IActionResult Contact() => View();
    [HttpGet("/AccessDenied")] public IActionResult AccessDenied() => View();
    [HttpGet("/Error")] public IActionResult Error() => View();
    [HttpGet("/NotFound")] public IActionResult Missing() => View("NotFound");
    [HttpGet("/Notifications")] public IActionResult Notifications() => View(new NotificationsModel { Read = NotificationsModel.All.Where(x=>HttpContext.Session.GetInt32("notice:"+x.Id)==1).Select(x=>x.Id).ToHashSet() });
 [HttpPost("/Notifications/Read")] public IActionResult Read(int id){if(NotificationsModel.All.Any(x=>x.Id==id))HttpContext.Session.SetInt32("notice:"+id,1);return RedirectToAction(nameof(Notifications));}
 [HttpPost("/Notifications/ReadAll")] public IActionResult ReadAll(){foreach(var item in NotificationsModel.All)HttpContext.Session.SetInt32("notice:"+item.Id,1);return RedirectToAction(nameof(Notifications));}
}
