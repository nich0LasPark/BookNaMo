using System;
using System.ComponentModel.DataAnnotations;

namespace BookNamo.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; } // Make nullable
        public string UserId { get; set; } = string.Empty; // Initialize with empty string
        public ApplicationUser? User { get; set; } // Make nullable

        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = string.Empty; // Initialize with empty string
    }

}
