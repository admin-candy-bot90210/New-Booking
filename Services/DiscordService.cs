using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DJBookingSystem.Services
{
    public class DiscordService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task SendBookingNotificationAsync(string webhookUrl, string djName, string venueName, DateTime bookingTime, int availableSlots)
        {
            if (string.IsNullOrEmpty(webhookUrl))
                return;

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "🎉 New Booking!",
                            description = $"A new DJ booking has been made at **{venueName}**",
                            color = 3447003, // Blue color
                            fields = new[]
                            {
                                new { name = "DJ Name", value = djName, inline = true },
                                new { name = "Venue", value = venueName, inline = true },
                                new { name = "Time", value = bookingTime.ToString("HH:mm"), inline = true },
                                new { name = "Date", value = bookingTime.ToString("dddd, MMMM dd, yyyy"), inline = false },
                                new { name = "Available Slots Left Today", value = availableSlots.ToString(), inline = true }
                            },
                            footer = new
                            {
                                text = "DJ Booking System"
                            },
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                string json = JsonConvert.SerializeObject(embed);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                // Silently fail if Discord webhook fails
            }
        }
    }
}
