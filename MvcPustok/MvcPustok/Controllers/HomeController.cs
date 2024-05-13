using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.Models;
using MvcPustok.ViewModels;
using Newtonsoft.Json;

namespace MvcPustok.Controllers;
public class HomeController : Controller
{
    private AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        HomeViewModel hv = new HomeViewModel()
        {
            FeaturedBooks = _context.Books.Include(x => x.Author).Include(x => x.BookImages.Where(x => x.Status != null)).Where(x => x.IsFeatured).Take(10).ToList(),
            NewBooks = _context.Books.Include(x => x.Author).Include(x => x.BookImages.Where(bi => bi.Status != null)).Where(x => x.IsNew).Take(10).ToList(),
            DiscountedBooks = _context.Books.Include(x => x.Author).Include(x => x.BookImages.Where(bi => bi.Status != null)).Where(x => x.DiscountPercent > 0).OrderByDescending(x => x.DiscountPercent).Take(10).ToList(),
            Sliders = _context.Sliders.OrderBy(x => x.Order).ToList(),
            Features = _context.Features.Take(4).ToList()
        };
        return View(hv);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> AddBasket(int id)
    {
        List<BasketCookiesViewModel> basketCookiesViewModels = null;

        Book book = _context.Books.FirstOrDefault(x => x.Id == id);

        if (HttpContext.Request.Cookies["Courses"] != null)
        {
            basketCookiesViewModels = JsonConvert.DeserializeObject<List<BasketCookiesViewModel>>(HttpContext.Request.Cookies["Courses"]);
        }
        else
        {
            basketCookiesViewModels = new List<BasketCookiesViewModel>();
        }
        var existsCooki = basketCookiesViewModels.FirstOrDefault(x => x.BookId == id);
        if (existsCooki != null)
        {
            existsCooki.Count++;
        }
        else
        {
            BasketCookiesViewModel basket = new BasketCookiesViewModel()
            {
                BookId = book.Id,
                Count = 1,
            };
            basketCookiesViewModels.Add(basket);
        }
        HttpContext.Response.Cookies.Append("Courses", JsonConvert.SerializeObject(basketCookiesViewModels));

        return RedirectToAction("index");
    }
}

