using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DJBookingSystem.Utilities
{
    public static class IPHelper
    {
        /// <summary>
        /// Gets the local IP address of the machine
        /// Note: This returns the local network IP, not the public IP
        /// For public IP tracking, you would need to call an external service
        /// </summary>
        public static string GetLocalIPAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                    {
                        return endPoint.Address.ToString();
                    }
                }
            }
            catch
            {
                // Fallback method
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.ToString();
                        }
                    }
                }
                catch
                {
                    // Return localhost if all else fails
                }
            }

            return "127.0.0.1";
        }

        /// <summary>
        /// Gets the public IP address by calling an external service
        /// This is more reliable for ban enforcement across different networks
        /// </summary>
        public static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetStringAsync("https://api.ipify.org");
                    return response.Trim();
                }
            }
            catch
            {
                // Fallback to local IP if public IP cannot be obtained
                return GetLocalIPAddress();
            }
        }

        /// <summary>
        /// Validates if an IP address is in a valid format
        /// </summary>
        public static bool IsValidIP(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            return IPAddress.TryParse(ip, out _);
        }
    }
}
