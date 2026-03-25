using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LibraryDbContext _context;

        public HomeController(ILogger<HomeController> logger, LibraryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Genel istatistikler
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalAuthors = await _context.Authors.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();

            // Aktif ödünç sayýsý
            ViewBag.ActiveLoans = await _context.Loans
                .CountAsync(l => !l.IsReturned);

            // gecikenler
            var allActiveLoans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Student)
                .Where(l => !l.IsReturned)
                .ToListAsync();

            var overdueLoans = allActiveLoans
                .Where(l => (DateTime.Now - l.LoanDate).TotalDays > 15)
                .ToList();

            ViewBag.OverdueLoans = overdueLoans.Count;
            ViewBag.OverdueList = overdueLoans;

            // Son eklenen 4 kitap
            ViewBag.RecentBooks = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.BookId)
                .Take(4)
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}