using BookNamo.Data;
using BookNamo.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

// Service for sending transactional emails to users (e.g., borrow notifications, due date reminders).

namespace BookNamo.Services
{
    public class BookNotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookNotificationService> _logger;

        public BookNotificationService(
            IEmailSender emailSender,
            ApplicationDbContext context,
            ILogger<BookNotificationService> logger)
        {
            _emailSender = emailSender;
            _context = context;
            _logger = logger;
        }

        public async Task SendBorrowNotificationAsync(int borrowRecordId)
        {
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .Include(br => br.User)
                    .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

                if (borrowRecord == null || string.IsNullOrEmpty(borrowRecord.User?.Email))
                {
                    _logger.LogWarning($"Could not send borrow notification: Invalid borrow record ID {borrowRecordId} or missing user email");
                    return;
                }

                var subject = $"Book Borrowed: {borrowRecord.Book.Title}";
                var message = GenerateBorrowEmailContent(borrowRecord);

                await _emailSender.SendEmailAsync(borrowRecord.User.Email, subject, message);
                _logger.LogInformation($"Borrow notification email sent to {borrowRecord.User.Email} for book {borrowRecord.Book.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending borrow notification: {ex.Message}");
            }
        }

        public async Task SendDueReminderAsync(int borrowRecordId)
        {
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .Include(br => br.User)
                    .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

                if (borrowRecord == null || string.IsNullOrEmpty(borrowRecord.User?.Email))
                {
                    _logger.LogWarning($"Could not send due reminder: Invalid borrow record ID {borrowRecordId} or missing user email");
                    return;
                }

                var subject = $"Reminder: Book Due Soon - {borrowRecord.Book.Title}";
                var message = GenerateDueReminderEmailContent(borrowRecord);

                await _emailSender.SendEmailAsync(borrowRecord.User.Email, subject, message);
                _logger.LogInformation($"Due date reminder email sent to {borrowRecord.User.Email} for book {borrowRecord.Book.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending due reminder: {ex.Message}");
            }
        }

        public async Task SendDueRemindersForAllBooksAsync()
        {
            try
            {
                // Find all borrowed books that are due in 3 days
                var threeDaysFromNow = DateTime.Now.Date.AddDays(3);
                var recordsDueSoon = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .Include(br => br.User)
                    .Where(br =>
                        br.Status == "Borrowed" &&
                        br.ReturnDate == null &&
                        br.DueDate.Date == threeDaysFromNow)
                    .ToListAsync();

                _logger.LogInformation($"Found {recordsDueSoon.Count} books due in 3 days");

                foreach (var record in recordsDueSoon)
                {
                    if (!string.IsNullOrEmpty(record.User?.Email))
                    {
                        var subject = $"Reminder: Book Due Soon - {record.Book.Title}";
                        var message = GenerateDueReminderEmailContent(record);

                        await _emailSender.SendEmailAsync(record.User.Email, subject, message);
                        _logger.LogInformation($"Due date reminder email sent to {record.User.Email} for book {record.Book.Title}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending due reminders: {ex.Message}");
            }
        }

        private string GenerateBorrowEmailContent(BorrowRecord borrowRecord)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #000; color: #fff; padding: 10px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; }}
                    .book-details {{ margin: 20px 0; }}
                    .book-details h3 {{ margin-top: 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 0.8em; color: #777; }}
                    .button {{ background-color: #000; color: #fff; padding: 10px 15px; text-decoration: none; display: inline-block; margin-top: 15px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>BookNamo Library</h2>
                    </div>
                    <div class='content'>
                        <p>Dear {borrowRecord.User.UserName},</p>
                        <p>Thank you for borrowing from BookNamo Library. Here are the details of your borrowed book:</p>
                        
                        <div class='book-details'>
                            <h3>{borrowRecord.Book.Title}</h3>
                            <p><strong>Author:</strong> {borrowRecord.Book.Author}</p>
                            <p><strong>ISBN:</strong> {borrowRecord.Book.ISBN}</p>
                            <p><strong>Borrowed on:</strong> {borrowRecord.BorrowDate.ToShortDateString()}</p>
                            <p><strong>Due date:</strong> {borrowRecord.DueDate.ToShortDateString()}</p>
                        </div>
                        
                        <p>Please return the book on or before the due date to avoid any late fees.</p>
                        <p>We hope you enjoy your reading experience!</p>
                        
                        <p>Regards,<br>BookNamo Library Team</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BookNamo Library. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            ";
        }

        private string GenerateDueReminderEmailContent(BorrowRecord borrowRecord)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #000; color: #fff; padding: 10px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; }}
                    .book-details {{ margin: 20px 0; }}
                    .book-details h3 {{ margin-top: 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 0.8em; color: #777; }}
                    .reminder {{ background-color: #ffe6e6; padding: 10px; border-left: 4px solid #ff8080; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>BookNamo Library - Due Date Reminder</h2>
                    </div>
                    <div class='content'>
                        <p>Dear {borrowRecord.User.UserName},</p>
                        
                        <div class='reminder'>
                            <p>This is a friendly reminder that your borrowed book is due to be returned in 3 days.</p>
                        </div>
                        
                        <div class='book-details'>
                            <h3>{borrowRecord.Book.Title}</h3>
                            <p><strong>Author:</strong> {borrowRecord.Book.Author}</p>
                            <p><strong>ISBN:</strong> {borrowRecord.Book.ISBN}</p>
                            <p><strong>Borrowed on:</strong> {borrowRecord.BorrowDate.ToShortDateString()}</p>
                            <p><strong>Due date:</strong> {borrowRecord.DueDate.ToShortDateString()}</p>
                        </div>
                        
                        <p>Please make arrangements to return the book on or before the due date to avoid any late fees.</p>
                        <p>Thank you for using BookNamo Library!</p>
                        
                        <p>Regards,<br>BookNamo Library Team</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} BookNamo Library. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            ";
        }
    }
}
