// Controller for home page and general site navigation actions.

using BookNamo.Models;
using BookNamo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BookNamo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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


        public IActionResult Browse(string searchTerm, string genre)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                books = books.Where(b => b.Title.Contains(searchTerm) ||
                                        b.Author.Contains(searchTerm) ||
                                        b.ISBN.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            ViewBag.Genres = _context.Books.Select(b => b.Genre).Distinct().ToList();

            return View(books.ToList());
        }

        [Authorize]
        public IActionResult MyBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowRecords = _context.BorrowRecords
                .Include(br => br.Book)
                .Where(br => br.UserId == userId)
                .OrderByDescending(br => br.BorrowDate)
                .ToList();

            return View(borrowRecords);
        }

        // Display about page with team member information
        public IActionResult About()
        {
            return View();
        }
    }
}
