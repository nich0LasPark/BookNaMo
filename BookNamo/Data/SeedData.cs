// Seeds initial data (admin user, roles, sample books) into the database on application startup.

using BookNamo.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace BookNamo.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create Admin Role
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Admin User
            var adminEmail = "admin@booknamo.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "Admin", // Set username to "Admin"
                    Email = adminEmail,
                    LastName = "User",
                    RegistrationDate = DateTime.Now,
                    EmailConfirmed = true,
                    OtpCode = null,
                    OtpExpiry = null
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    // Log any errors
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"Admin creation failed: {errors}");
                }
            }
            else
            {
                // If the admin user already exists, make sure the username is "Admin"
                if (adminUser.UserName != "Admin")
                {
                    var setUsernameResult = await userManager.SetUserNameAsync(adminUser, "Admin");
                    if (setUsernameResult.Succeeded)
                    {
                        Console.WriteLine("Admin username updated to 'Admin'.");
                    }
                    else
                    {
                        var errors = string.Join(", ", setUsernameResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"Failed to update admin username: {errors}");
                    }
                }
            }

            // Seed books if none exist
            if (!context.Books.Any())
            {
                var books = new[]
                {
                    new Book
                    {
                        Title = "The Great Gatsby",
                        Author = "F. Scott Fitzgerald",
                        ISBN = "9780743273565",
                        PublicationYear = 1925,
                        Genre = "Classic",
                        TotalCopies = 5,
                        AvailableCopies = 5,
                        Description = "The story of the fabulously wealthy Jay Gatsby and his love for the beautiful Daisy Buchanan.",
                        Publisher = "Scribner"
                    },
                    new Book
                    {
                        Title = "The Midnight Library",
                        Author = "Matt Haig",
                        ISBN = "9780525559474",
                        PublicationYear = 2023,
                        Genre = "Bestseller",
                        TotalCopies = 8,
                        AvailableCopies = 5,
                        Description = "Between life and death there is a library filled with books that give you a chance to try different lives.",
                        Publisher = "Viking"
                    },
                    new Book
                    {
                        Title = "Project Hail Mary",
                        Author = "Andy Weir",
                        ISBN = "9780593395561",
                        PublicationYear = 2022,
                        Genre = "Science Fiction",
                        TotalCopies = 7,
                        AvailableCopies = 4,
                        Description = "A lone astronaut must save the earth from disaster in this incredible new science-based thriller.",
                        Publisher = "Ballantine Books"
                    },
                    new Book
                    {
                        Title = "Lessons in Chemistry",
                        Author = "Bonnie Garmus",
                        ISBN = "9780385547345",
                        PublicationYear = 2023,
                        Genre = "Bestseller",
                        TotalCopies = 10,
                        AvailableCopies = 6,
                        Description = "A scientist and single mother in 1960s California who becomes a star on a TV cooking show.",
                        Publisher = "Doubleday"
                    },
                    new Book
                    {
                        Title = "The Covenant of Water",
                        Author = "Abraham Verghese",
                        ISBN = "9780802162175",
                        PublicationYear = 2023,
                        Genre = "Bestseller",
                        TotalCopies = 6,
                        AvailableCopies = 3,
                        Description = "A sweeping novel set in Kerala, India spanning from 1900 to 1977.",
                        Publisher = "Grove Press"
                    },
                    new Book
                    {
                        Title = "Tomorrow, and Tomorrow, and Tomorrow",
                        Author = "Gabrielle Zevin",
                        ISBN = "9780593321201",
                        PublicationYear = 2022,
                        Genre = "Fiction",
                        TotalCopies = 7,
                        AvailableCopies = 4,
                        Description = "The story of friendship, creativity, and video game development spanning thirty years.",
                        Publisher = "Knopf"
                    }
                };

                context.Books.AddRange(books);
                await context.SaveChangesAsync();
            }

            // Ensure all users have a properly formatted username
            var allUsers = userManager.Users.ToList();
            foreach (var user in allUsers)
            {
                // Ensure all emails are confirmed
                if (!user.EmailConfirmed)
                {
                    var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, confirmToken);
                    Console.WriteLine($"Confirmed email for user: {user.UserName}");
                }
            }
        }
    }
}
