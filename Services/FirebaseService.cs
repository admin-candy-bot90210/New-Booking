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
        private const string ChatNode = "chat_messages";
        private const string VersionNode = "app_version";
        private const string RadioStationsNode = "radio_stations";
        private const string CurrentAppVersion = "2.0.0";

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

                // Create primary SysAdmin if it doesn't exist
                if (!users.Any(u => u.Username == "SysAdmin"))
                {
                    var primarySysAdmin = new User
                    {
                        Username = "SysAdmin",
                        PasswordHash = "d8dffb2b4a7ede9c6e409bb120adc43bd0fd98e6f390424c13fa9768602573fb",
                        FullName = "System Administrator",
                        Email = "sysadmin@djbooking.com",
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
                            CanAccessSettings = true,
                            CanViewRadioBoss = true,
                            CanControlRadioBoss = true
                        },
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    await AddUserAsync(primarySysAdmin);
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

        // CHAT MANAGEMENT

        // Add a chat message
        public async Task<string> AddChatMessageAsync(ChatMessage message)
        {
            var result = await _firebaseClient
                .Child(ChatNode)
                .PostAsync(message);

            return result.Key;
        }

        // Get all chat messages
        public async Task<List<ChatMessage>> GetAllChatMessagesAsync()
        {
            var messages = await _firebaseClient
                .Child(ChatNode)
                .OnceAsync<ChatMessage>();

            return messages.Select(m => new ChatMessage
            {
                Id = m.Key,
                SenderUsername = m.Object.SenderUsername,
                SenderRole = m.Object.SenderRole,
                Message = m.Object.Message,
                Timestamp = m.Object.Timestamp,
                IsErrorMessage = m.Object.IsErrorMessage,
                ErrorCode = m.Object.ErrorCode,
                Type = m.Object.Type
            }).OrderBy(m => m.Timestamp).ToList();
        }

        // Log error to chat (automatically sent to SysAdmin)
        public async Task LogErrorToChatAsync(string errorMessage, string errorCode, string username)
        {
            var errorChatMessage = new ChatMessage
            {
                SenderUsername = username,
                SenderRole = "System",
                Message = $"ERROR [{errorCode}]: {errorMessage}",
                Timestamp = DateTime.Now,
                IsErrorMessage = true,
                ErrorCode = errorCode,
                Type = MessageType.Error
            };

            await AddChatMessageAsync(errorChatMessage);
        }

        // VERSION MANAGEMENT

        // Get latest app version from Firebase
        public async Task<AppVersion?> GetLatestVersionAsync()
        {
            try
            {
                var version = await _firebaseClient
                    .Child(VersionNode)
                    .OnceSingleAsync<AppVersion>();

                return version;
            }
            catch
            {
                return null;
            }
        }

        // Check if update is available
        public async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                var latestVersion = await GetLatestVersionAsync();
                if (latestVersion == null) return false;

                return CompareVersions(CurrentAppVersion, latestVersion.Version) < 0;
            }
            catch
            {
                return false;
            }
        }

        // Compare version strings (e.g., "2.0.0" vs "2.1.0")
        private int CompareVersions(string current, string latest)
        {
            var currentParts = current.Split('.').Select(int.Parse).ToArray();
            var latestParts = latest.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Max(currentParts.Length, latestParts.Length); i++)
            {
                int currentPart = i < currentParts.Length ? currentParts[i] : 0;
                int latestPart = i < latestParts.Length ? latestParts[i] : 0;

                if (currentPart < latestPart) return -1;
                if (currentPart > latestPart) return 1;
            }

            return 0;
        }

        // Update app version info (Admin only)
        public async Task SetLatestVersionAsync(AppVersion version)
        {
            await _firebaseClient
                .Child(VersionNode)
                .PutAsync(version);
        }

        public string GetCurrentVersion()
        {
            return CurrentAppVersion;
        }

        // RADIO STATION MANAGEMENT

        // Add a radio station
        public async Task<string> AddRadioStationAsync(RadioStation station)
        {
            var result = await _firebaseClient
                .Child(RadioStationsNode)
                .PostAsync(station);

            return result.Key;
        }

        // Get all radio stations
        public async Task<List<RadioStation>> GetAllRadioStationsAsync()
        {
            var stations = await _firebaseClient
                .Child(RadioStationsNode)
                .OnceAsync<RadioStation>();

            return stations.Select(s => new RadioStation
            {
                Id = s.Key,
                Name = s.Object.Name,
                StreamUrl = s.Object.StreamUrl,
                Genre = s.Object.Genre,
                AddedDate = s.Object.AddedDate,
                AddedBy = s.Object.AddedBy,
                IsFavorite = s.Object.IsFavorite
            }).OrderBy(s => s.Name).ToList();
        }

        // Get radio stations by user
        public async Task<List<RadioStation>> GetRadioStationsByUserAsync(string username)
        {
            var allStations = await GetAllRadioStationsAsync();
            return allStations.Where(s => s.AddedBy == username).ToList();
        }

        // Update radio station
        public async Task UpdateRadioStationAsync(string id, RadioStation station)
        {
            await _firebaseClient
                .Child(RadioStationsNode)
                .Child(id)
                .PutAsync(station);
        }

        // Delete radio station
        public async Task DeleteRadioStationAsync(string id)
        {
            await _firebaseClient
                .Child(RadioStationsNode)
                .Child(id)
                .DeleteAsync();
        }
    }
}
