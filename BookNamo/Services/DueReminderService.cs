// Background service that sends email reminders to users when borrowed books are due soon.

using BookNamo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookNamo.Services
{
    public class DueReminderService : BackgroundService
    {
        private readonly ILogger<DueReminderService> _logger;
        private readonly IServiceProvider _services;

        public DueReminderService(
            ILogger<DueReminderService> logger,
            IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Due Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check the current time - run once a day at a specific time (e.g., 8:00 AM)
                    var now = DateTime.Now;
                    var scheduledTime = new TimeSpan(8, 0, 0); // 8:00 AM

                    if (now.TimeOfDay > scheduledTime)
                    {
                        // Calculate time until next day's scheduled time
                        var tomorrow = now.Date.AddDays(1).Add(scheduledTime);
                        var delay = tomorrow - now;
                        await Task.Delay(delay, stoppingToken);
                    }
                    else
                    {
                        // Calculate time until today's scheduled time
                        var today = now.Date.Add(scheduledTime);
                        var delay = today - now;
                        await Task.Delay(delay, stoppingToken);
                    }

                    // After waiting, send reminders
                    await SendRemindersAsync();

                    // Wait 24 hours before checking again
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error occurred in due reminder background service.");

                    // Wait for a shorter period on error before trying again
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }

            _logger.LogInformation("Due Reminder Service is stopping.");
        }

        private async Task SendRemindersAsync()
        {
            _logger.LogInformation("Sending due date reminders...");

            try
            {
                using var scope = _services.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<BookNotificationService>();

                await notificationService.SendDueRemindersForAllBooksAsync();

                _logger.LogInformation("Due date reminders sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send due date reminders");
            }
        }
    }
}
