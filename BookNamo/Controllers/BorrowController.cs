using BookNamo.Models;
using BookNamo.Data;
using BookNamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookNamo.Controllers
{
    [Authorize]
    public class BorrowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BookNotificationService _notificationService;

        public BorrowController(
            ApplicationDbContext context,
            BookNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || book.AvailableCopies <= 0)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var borrowRecord = new BorrowRecord
            {
                BookId = bookId,
                UserId = userId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                ReturnDate = null,
                Status = "Borrowed"
            };

            book.AvailableCopies--;

            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            // Send borrow notification email
            await _notificationService.SendBorrowNotificationAsync(borrowRecord.Id);

            return RedirectToAction("MyBooks", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int recordId)
        {
            var record = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == recordId);

            if (record == null || record.Status == "Returned")
            {
                return NotFound();
            }

            record.ReturnDate = DateTime.Now;
            record.Status = "Returned";
            record.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            return RedirectToAction("MyBooks", "Home");
        }

        [HttpGet]
        public IActionResult BorrowPage()
        {
            var availableBooks = _context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderBy(b => b.Title)
                .ToList();

            return View(availableBooks);
        }

        [HttpGet]
        public IActionResult MyBorrowings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowRecords = _context.BorrowRecords
                .Include(br => br.Book)
                .Where(br => br.UserId == userId && br.Status == "Borrowed")
                .OrderBy(br => br.DueDate)
                .ToList();

            return View(borrowRecords);
        }

        [HttpGet]
        public IActionResult FeaturedBooks()
        {
            // Get featured books based on specific criteria
            var featuredBooks = _context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderByDescending(b =>
                    // Consider multiple factors for featuring books
                    (b.PublicationYear >= 2022 ? 10 : 0) + // Recent publications get priority
                    (b.Genre == "Bestseller" ? 5 : 0) +    // Bestsellers get priority
                    (b.TotalCopies > 3 ? 3 : 0)            // Books with more copies get some priority
                )
                .Take(6)
                .ToList();

            return View(featuredBooks);
        }
    }
}
