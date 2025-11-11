using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DJBookingSystem.Services
{
    public class RadioBossService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly int _stationId;

        public RadioBossService(string baseUrl = "https://c40.radioboss.fm", string apiKey = "8VTO5355BZ5X", int stationId = 98)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _stationId = stationId;
        }

        // Get station information
        public async Task<JObject?> GetStationInfoAsync()
        {
            try
            {
                string url = $"{_baseUrl}/api/info/{_stationId}?key={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                return JObject.Parse(response);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get station info: {ex.Message}");
            }
        }

        // Get current playing track
        public async Task<string> GetCurrentTrackAsync()
        {
            try
            {
                var info = await GetStationInfoAsync();
                if (info != null && info["title"] != null)
                {
                    return info["title"]?.ToString() ?? "Unknown";
                }
                return "No track playing";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Get playlist contents
        public async Task<JObject?> GetPlaylistAsync()
        {
            try
            {
                string url = $"{_baseUrl}/api/getplaylist/{_stationId}?key={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                return JObject.Parse(response);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get playlist: {ex.Message}");
            }
        }

        // Get upcoming scheduled events
        public async Task<JObject?> GetUpcomingEventsAsync()
        {
            try
            {
                string url = $"{_baseUrl}/api/getupcomingevents/{_stationId}?key={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                return JObject.Parse(response);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get upcoming events: {ex.Message}");
            }
        }

        // Set artwork for live stream (POST request)
        public async Task<bool> SetArtworkAsync(string artworkUrl)
        {
            try
            {
                string url = $"{_baseUrl}/api/setartwork/{_stationId}?key={_apiKey}";
                var content = new StringContent($"{{\"artwork\":\"{artworkUrl}\"}}", System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set artwork: {ex.Message}");
            }
        }

        // Get listener statistics
        public async Task<string> GetListenerCountAsync()
        {
            try
            {
                var info = await GetStationInfoAsync();
                if (info != null && info["listeners"] != null)
                {
                    return info["listeners"]?.ToString() ?? "0";
                }
                return "0";
            }
            catch
            {
                return "N/A";
            }
        }

        // Get station status
        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                var info = await GetStationInfoAsync();
                if (info != null && info["online"] != null)
                {
                    return info["online"]?.Value<bool>() ?? false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Format track info for display
        public async Task<string> GetFormattedTrackInfoAsync()
        {
            try
            {
                var info = await GetStationInfoAsync();
                if (info == null) return "No data available";

                string title = info["title"]?.ToString() ?? "Unknown";
                string artist = info["artist"]?.ToString() ?? "";
                string duration = info["duration"]?.ToString() ?? "";
                string elapsed = info["elapsed"]?.ToString() ?? "";

                string result = $"Now Playing: {title}";
                if (!string.IsNullOrEmpty(artist))
                    result += $"\nArtist: {artist}";
                if (!string.IsNullOrEmpty(duration))
                    result += $"\nDuration: {duration}";
                if (!string.IsNullOrEmpty(elapsed))
                    result += $"\nElapsed: {elapsed}";

                return result;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
