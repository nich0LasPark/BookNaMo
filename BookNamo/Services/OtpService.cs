// Service for generating, saving, validating, and emailing OTP codes for user verification.
using BookNamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BookNamo.Services
{
    public class OtpService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<OtpService> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // Generate a numeric OTP of specified length (default 6)
        public string GenerateOtp(int length = 6)
        {
            // Generate a random numeric OTP
            StringBuilder otp = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumber = new byte[1];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomNumber);
                    otp.Append(randomNumber[0] % 10); // Get a number between 0-9
                }
            }

            return otp.ToString();
        }

        // Save OTP for a user
        public async Task<bool> SaveOtpForUserAsync(ApplicationUser user, string otp)
        {
            try
            {
                user.OtpCode = otp;
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving OTP for user: {ex.Message}");
                return false;
            }
        }

        // Validate OTP for a user
        public async Task<bool> ValidateOtpAsync(ApplicationUser user, string otp)
        {
            if (user == null || string.IsNullOrEmpty(otp))
                return false;

            // Check if OTP matches and is not expired
            if (user.OtpCode == otp && user.OtpExpiry > DateTime.UtcNow)
            {
                // Clear the OTP after successful validation
                user.OtpCode = null;
                user.OtpExpiry = null;
                await _userManager.UpdateAsync(user);

                return true;
            }

            return false;
        }

        // Send OTP via email
        public async Task SendOtpEmailAsync(ApplicationUser user, string otp)
        {
            var emailSubject = "BookNamo - Your Account Verification Code";
            // Add null check or default value for username
            var username = user.UserName ?? "User"; // Fix for CS8604
            var emailBody = GenerateOtpEmailBody(username, otp);

            // Add null check for email
            if (user.Email != null) // Fix for CS8604
            {
                await _emailSender.SendEmailAsync(user.Email, emailSubject, emailBody);
                _logger.LogInformation($"OTP email sent to {user.Email}");
            }
            else
            {
                _logger.LogWarning("Cannot send OTP - user email is null");
            }
        }

        private string GenerateOtpEmailBody(string username, string otp)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #000000; color: #F2F2F2; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #F2F2F2; border: 1px solid #B6B09F; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 0.8em; color: #777; }}
                    .otp-container {{ background-color: #EAE4D5; padding: 15px; text-align: center; margin: 20px 0; 
                                     border-radius: 5px; border: 1px dashed #B6B09F; }}
                    .otp-code {{ font-size: 32px; font-weight: bold; letter-spacing: 5px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>BookNamo Library</h2>
                    </div>
                    <div class='content'>
                        <p>Hello {username},</p>
                        <p>Thank you for registering with BookNamo Library! To verify your account, please use the following verification code:</p>
                        
                        <div class='otp-container'>
                            <div class='otp-code'>{otp}</div>
                        </div>
                        
                        <p>This code will expire in <strong>10 minutes</strong>.</p>
                        
                        <p>If you didn't create an account with BookNamo Library, you can safely ignore this email.</p>
                        
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
