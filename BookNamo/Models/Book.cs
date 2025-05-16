using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookNamo.Models
{
    public class Book
    {
        // Add constructor to initialize non-nullable reference properties
        public Book()
        {
            Title = string.Empty;
            Author = string.Empty;
            ISBN = string.Empty;
            Genre = string.Empty;
            Description = string.Empty;
            Publisher = string.Empty;
            BorrowRecords = new List<BorrowRecord>();
        }

        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Display(Name = "Publication Year")]
        public int PublicationYear { get; set; }

        public string Genre { get; set; }

        [Display(Name = "Total Copies")]
        public int TotalCopies { get; set; }

        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        public string Description { get; set; }

        public string Publisher { get; set; }

        public ICollection<BorrowRecord> BorrowRecords { get; set; }
    }

}
