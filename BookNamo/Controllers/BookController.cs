// Controller for book-related actions (e.g., listing, viewing, managing books).

using BookNamo.Data;
using BookNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookNamo.Controllers
{
    // Remove the class-level attribute to allow non-admin access to specific actions
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Book
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm, string genre)
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

            ViewBag.Genres = await _context.Books.Select(b => b.Genre).Distinct().ToListAsync();

            return View(await books.ToListAsync());
        }

        // GET: Book/Details/5
        // No Authorize attribute - everyone can access this
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Book/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book)
        {
            // Add some debug to see what's happening
            System.Diagnostics.Debug.WriteLine($"Create action called with book title: {book.Title}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["ValidationError"] = errors;
                System.Diagnostics.Debug.WriteLine($"Validation errors: {errors}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure non-required fields have default values if empty
                    book.Description ??= "";
                    book.Genre ??= "General";
                    book.Publisher ??= "Unknown Publisher";

                    if (book.TotalCopies <= 0) book.TotalCopies = 1;
                    if (book.AvailableCopies <= 0) book.AvailableCopies = book.TotalCopies;
                    if (book.PublicationYear <= 0) book.PublicationYear = DateTime.Now.Year;

                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Book saved successfully with ID: {book.Id}");
                    TempData["SuccessMessage"] = $"Book '{book.Title}' was successfully created.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving book: {ex.Message}");
                    TempData["ValidationError"] = $"Error saving book: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"Exception when saving book: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(book);
        }

        // GET: Book/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure non-required fields have default values if empty
                    book.Description ??= "";
                    book.Genre ??= "General";
                    book.Publisher ??= "Unknown Publisher";

                    if (book.TotalCopies <= 0) book.TotalCopies = 1;
                    if (book.AvailableCopies <= 0) book.AvailableCopies = book.TotalCopies;
                    if (book.PublicationYear <= 0) book.PublicationYear = DateTime.Now.Year;

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Book '{book.Title}' was successfully updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Concurrency error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Concurrency exception: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating book: {ex.Message}");
                    TempData["ValidationError"] = $"Error updating book: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"Exception when updating book: {ex.Message}");
                }
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["ValidationError"] = errors;
                System.Diagnostics.Debug.WriteLine($"Validation errors: {errors}");
            }

            return View(book);
        }

        // GET: Book/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                // Check if there are any active borrows for this book
                bool hasActiveBorrows = await _context.BorrowRecords
                    .AnyAsync(br => br.BookId == id && br.Status == "Borrowed");

                if (hasActiveBorrows)
                {
                    TempData["ValidationError"] = "Cannot delete this book because it is currently borrowed by users.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Book '{book.Title}' was successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ValidationError"] = $"Error deleting book: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception when deleting book: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
