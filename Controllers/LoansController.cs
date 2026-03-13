using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class LoansController : Controller
    {
        private readonly LibraryDbContext _context;

        public LoansController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var libraryDbContext = _context.Loans.Include(l => l.Book).Include(l => l.Student);
            return View(await libraryDbContext.ToListAsync());
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Student)
                .FirstOrDefaultAsync(m => m.LoanId == id);
            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            // Sadece henüz iade edilmemiş kitapları veya tüm kitapları listeleyebilirsin
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "Title");
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentFullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoanId,BookId,StudentId,LoanDate,ReturnDate,IsReturned")] Loan loan)
        {
            if (ModelState.IsValid)
            {
                // Aynı öğrenci numarası kontrolü
                // Bu kural Student controller'da yapılacak, burada Loan kuralları var.

                //Öğrencinin aktif ödüncü 3'ten fazla olamaz
                var activeLoansCount = await _context.Loans
                    .CountAsync(l => l.StudentId == loan.StudentId && !l.IsReturned);

                if (activeLoansCount >= 3)
                {
                    ModelState.AddModelError("", "Bu öğrencinin zaten 3 aktif ödüncü bulunmaktadır. Yeni kitap veremezsiniz.");
                    ViewData["BookId"] = new SelectList(_context.Books, "BookId", "Title", loan.BookId);
                    ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentFullName", loan.StudentId);
                    return View(loan);
                }

                // Aynı kitap zaten başka birinde olamaz
                var isBookAlreadyLoaned = await _context.Loans
                    .AnyAsync(l => l.BookId == loan.BookId && !l.IsReturned);

                if (isBookAlreadyLoaned)
                {
                    ModelState.AddModelError("", "Bu kitap şu anda başka bir öğrencide ödünçte. İade edilmeden verilemez.");
                    ViewData["BookId"] = new SelectList(_context.Books, "BookId", "Title", loan.BookId);
                    ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentFullName", loan.StudentId);
                    return View(loan);
                }

                _context.Add(loan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "Title", loan.BookId);
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentFullName", loan.StudentId);
            return View(loan);
        }

        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "Title", loan.BookId);
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentFullName", loan.StudentId);
            return View(loan);
        }

        // POST: Loans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LoanId,BookId,StudentId,LoanDate,ReturnDate,IsReturned")] Loan loan)
        {
            if (id != loan.LoanId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanExists(loan.LoanId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "Title", loan.BookId);
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentFullName", loan.StudentId);
            return View(loan);
        }

        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Student)
                .FirstOrDefaultAsync(m => m.LoanId == id);
            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan != null)
            {
                _context.Loans.Remove(loan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // POST: Loans/QuickReturn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickReturn(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return NotFound();

            loan.IsReturned = true;
            loan.ReturnDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.LoanId == id);
        }
    }
}
