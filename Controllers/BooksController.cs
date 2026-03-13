using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books
        // Alfabetik öncelikli arama sistemi
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var booksQuery = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Küçük/büyük harf duyarlılığını minimize ederek filtrele
                booksQuery = booksQuery.Where(s => s.Title.Contains(searchString)
                                       || s.Author.AuthorFullName.Contains(searchString)
                                       || s.Category.CategoryName.Contains(searchString));

                // SIRALAMA MANTIĞI: 
                // 1. Önce başlığı aranan kelime ile BAŞLAYANLAR (True olanlar en üste gelir)
                // 2. Sonra kendi içinde alfabetik sıralama
                booksQuery = booksQuery
                    .OrderByDescending(s => s.Title.StartsWith(searchString))
                    .ThenBy(s => s.Title);
            }
            else
            {
                // Arama yoksa varsayılan olarak alfabetik sırala
                booksQuery = booksQuery.OrderBy(s => s.Title);
            }

            return View(await booksQuery.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            // Kitabın aktif ödüncünü çek
            var activeLoan = await _context.Loans
                .Include(l => l.Student)
                .FirstOrDefaultAsync(l => l.BookId == id && !l.IsReturned);

            ViewBag.ActiveLoan = activeLoan;

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewBag.AuthorId = new SelectList(_context.Authors, "AuthorId", "AuthorFullName");
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Title,ISBN,PublishedYear,CategoryId,AuthorId,Summary,ImageUrl")] Book book, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    book.ImageUrl = "/img/" + fileName;
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AuthorId = new SelectList(_context.Authors, "AuthorId", "AuthorFullName", book.AuthorId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            ViewBag.AuthorId = new SelectList(_context.Authors, "AuthorId", "AuthorFullName", book.AuthorId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", book.CategoryId);

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,ISBN,PublishedYear,CategoryId,AuthorId,Summary,ImageUrl")] Book book, IFormFile? imageFile)
        {
            if (id != book.BookId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        book.ImageUrl = "/img/" + fileName;
                    }
                    else
                    {
                        // Resim yüklenmediyse mevcut yolu koru
                        _context.Entry(book).Property(x => x.ImageUrl).IsModified = false;
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AuthorId = new SelectList(_context.Authors, "AuthorId", "AuthorFullName", book.AuthorId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        // Otomatik tamamlama 
        [HttpGet]
        public async Task<JsonResult> GetSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<string>());

            var suggestions = await _context.Books
                .Where(b => b.Title.Contains(term)
                         || b.Author.AuthorFullName.Contains(term)
                         || b.Category.CategoryName.Contains(term))
                // filtreleme önceliklendirmesi
                .OrderByDescending(b => b.Title.StartsWith(term))
                .ThenBy(b => b.Title)
                .Select(b => b.Title)
                .Distinct()
                .Take(10)
                .ToListAsync();

            return Json(suggestions);
        }
    }
}