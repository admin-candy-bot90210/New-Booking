using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string BookingsNode = "bookings";
        private const string VenuesNode = "venues";
        private const string UsersNode = "users";
        private const string SettingsNode = "settings";

        public FirebaseService(string firebaseUrl)
        {
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        // Add a new booking
        public async Task<string> AddBookingAsync(Booking booking)
        {
            var result = await _firebaseClient
                .Child(BookingsNode)
                .PostAsync(booking);

            return result.Key;
        }

        // Get all bookings
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            var bookings = await _firebaseClient
                .Child(BookingsNode)
                .OnceAsync<Booking>();

            return bookings.Select(b => new Booking
            {
                Id = b.Key,
                DJName = b.Object.DJName,
                StreamingLink = b.Object.StreamingLink,
                Venue = b.Object.Venue,
                BookingDate = b.Object.BookingDate,
                CreatedAt = b.Object.CreatedAt
            }).ToList();
        }

        // Get booking by ID
        public async Task<Booking?> GetBookingByIdAsync(string id)
        {
            var booking = await _firebaseClient
                .Child(BookingsNode)
                .Child(id)
                .OnceSingleAsync<Booking>();

            if (booking != null)
            {
                booking.Id = id;
            }

            return booking;
        }

        // Update booking
        public async Task UpdateBookingAsync(string id, Booking booking)
        {
            await _firebaseClient
                .Child(BookingsNode)
                .Child(id)
                .PutAsync(booking);
        }

        // Delete booking
        public async Task DeleteBookingAsync(string id)
        {
            await _firebaseClient
                .Child(BookingsNode)
                .Child(id)
                .DeleteAsync();
        }

        // Get bookings by venue
        public async Task<List<Booking>> GetBookingsByVenueAsync(string venue)
        {
            var allBookings = await GetAllBookingsAsync();
            return allBookings.Where(b => b.Venue == venue).ToList();
        }

        // VENUE MANAGEMENT

        // Add a new venue
        public async Task<string> AddVenueAsync(Venue venue)
        {
            var result = await _firebaseClient
                .Child(VenuesNode)
                .PostAsync(venue);

            return result.Key;
        }

        // Get all venues
        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            var venues = await _firebaseClient
                .Child(VenuesNode)
                .OnceAsync<Venue>();

            return venues.Select(v => new Venue
            {
                Id = v.Key,
                RoomName = v.Object.RoomName,
                RoomDescription = v.Object.RoomDescription,
                OpeningHours = v.Object.OpeningHours,
                IsOpen = v.Object.IsOpen,
                CreatedAt = v.Object.CreatedAt
            }).ToList();
        }

        // Get open venues only
        public async Task<List<Venue>> GetOpenVenuesAsync()
        {
            var allVenues = await GetAllVenuesAsync();
            return allVenues.Where(v => v.IsOpen).ToList();
        }

        // Update venue
        public async Task UpdateVenueAsync(string id, Venue venue)
        {
            await _firebaseClient
                .Child(VenuesNode)
                .Child(id)
                .PutAsync(venue);
        }

        // Delete venue
        public async Task DeleteVenueAsync(string id)
        {
            await _firebaseClient
                .Child(VenuesNode)
                .Child(id)
                .DeleteAsync();
        }

        // USER MANAGEMENT

        // Get user by username
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await _firebaseClient
                .Child(UsersNode)
                .OnceAsync<User>();

            var userSnapshot = users.FirstOrDefault(u =>
                u.Object.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (userSnapshot == null)
                return null;

            var user = userSnapshot.Object;
            user.Id = userSnapshot.Key;
            return user;
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _firebaseClient
                .Child(UsersNode)
                .OnceAsync<User>();

            return users.Select(u => new User
            {
                Id = u.Key,
                Username = u.Object.Username,
                PasswordHash = u.Object.PasswordHash,
                FullName = u.Object.FullName,
                Email = u.Object.Email,
                Role = u.Object.Role,
                Permissions = u.Object.Permissions ?? new UserPermissions(),
                IsActive = u.Object.IsActive,
                CreatedAt = u.Object.CreatedAt,
                LastLogin = u.Object.LastLogin
            }).ToList();
        }

        // Add user
        public async Task<string> AddUserAsync(User user)
        {
            var result = await _firebaseClient
                .Child(UsersNode)
                .PostAsync(user);

            return result.Key;
        }

        // Update user
        public async Task UpdateUserAsync(string id, User user)
        {
            await _firebaseClient
                .Child(UsersNode)
                .Child(id)
                .PutAsync(user);
        }

        // Delete user
        public async Task DeleteUserAsync(string id)
        {
            await _firebaseClient
                .Child(UsersNode)
                .Child(id)
                .DeleteAsync();
        }

        // Initialize default admin account
        public async Task InitializeDefaultAdminAsync()
        {
            try
            {
                var users = await GetAllUsersAsync();

                // Check if any SysAdmin exists
                if (!users.Any(u => u.Role == UserRole.SysAdmin))
                {
                    var defaultAdmin = new User
                    {
                        Username = "admin",
                        PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", // "admin123"
                        FullName = "System Administrator",
                        Email = "admin@djbooking.com",
                        Role = UserRole.SysAdmin,
                        Permissions = new UserPermissions
                        {
                            CanViewBookings = true,
                            CanCreateBookings = true,
                            CanEditBookings = true,
                            CanDeleteBookings = true,
                            CanViewVenues = true,
                            CanRegisterVenues = true,
                            CanEditVenues = true,
                            CanDeleteVenues = true,
                            CanToggleVenueStatus = true,
                            CanManageUsers = true,
                            CanCustomizeApp = true,
                            CanAccessSettings = true
                        },
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    await AddUserAsync(defaultAdmin);
                }
            }
            catch
            {
                // Silently fail if Firebase is not accessible
            }
        }

        // APP SETTINGS MANAGEMENT

        // Get app settings
        public async Task<AppSettings> GetAppSettingsAsync()
        {
            try
            {
                var settings = await _firebaseClient
                    .Child(SettingsNode)
                    .Child("app_settings")
                    .OnceSingleAsync<AppSettings>();

                return settings ?? new AppSettings();
            }
            catch
            {
                // Return default settings if not found
                return new AppSettings();
            }
        }

        // Update app settings
        public async Task UpdateAppSettingsAsync(AppSettings settings)
        {
            settings.UpdatedAt = DateTime.Now;
            await _firebaseClient
                .Child(SettingsNode)
                .Child("app_settings")
                .PutAsync(settings);
        }
    }
}
