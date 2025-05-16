// Custom user model extending IdentityUser. Adds extra fields for registration date, last name, and OTP verification.

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookNamo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Add these properties for OTP functionality
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }

        public ICollection<BorrowRecord>? BorrowRecords { get; set; }
    }
}