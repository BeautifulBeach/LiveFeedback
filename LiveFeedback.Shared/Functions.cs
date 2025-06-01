using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LiveFeedback.Shared;

public class Functions
{
    public static string EnvOrDefault(string envName, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(envName)?.Trim() ?? defaultValue;
    }

    public static string RemoveRedundantQuotationMarks(string input)
    {
        if (input.Length > 2 && input.StartsWith($"\"") && input.EndsWith($"\""))
        {
            return input.Substring(1, input.Length - 2);
        }

        return input;
    }

    /// <summary>
    /// Attempts to detect whether a WWWRoot path has been set manually. If one exists, it is returned,
    /// otherwise depending on the assumption of the environment.
    /// </summary>
    public static string GetWwwRoot()
    {
        string possibleAbsolutePath = EnvOrDefault(Constants.WwwRootPathEnvName, "").Trim();
        if (Directory.Exists(possibleAbsolutePath))
        {
            return possibleAbsolutePath;
        }
        
        string mode = EnvOrDefault(Constants.ModeEnvName, "local");
        if (mode == "local")
        {
            string baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "wwwroot");
        }
        else
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
    }

    /// <summary>
    /// Returns the first available IPv4 address that is not a loopback address 
    /// and not in the link-local range (169.254.x.x). If none is found, the loopback address is returned.
    /// </summary>
    public static IPAddress GetLocalNetworkIpAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Only consider active interfaces and no loopback or tunnel interfaces.
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                continue;

            IPInterfaceProperties ipProps = ni.GetIPProperties();
            foreach (UnicastIPAddressInformation ua in ipProps.UnicastAddresses)
            {
                // Only consider IPv4 addresses and make sure that it is not a loopback address.
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ua.Address))
                {
                    continue;
                }

                // Optional extension: Here you could, for example, exclude link-local addresses (169.254.x.x).
                byte[] bytes = ua.Address.GetAddressBytes();
                if (bytes[0] == 169 && bytes[1] == 254)
                    continue;

                return ua.Address;
            }
        }

        // If no valid address was found, as a fallback:
        return IPAddress.Loopback;
    }

    /// <summary>
    /// Tries to resolve a hostname (preferably a fully qualified domain name) from the local network IP.
    /// If a proper hostname is found, it is returned; otherwise, the IP address string is returned.
    /// </summary>
    public static string GetAccessibleLocalHost()
    {
        // Get the local network IP address.
        IPAddress localIp = GetLocalNetworkIpAddress();

        try
        {
            // Attempt a reverse DNS lookup using the local IP.
            IPHostEntry? hostEntry = Dns.GetHostEntry(localIp);

            // Check if a hostname is available and does not simply return the IP address.
            if (!string.IsNullOrEmpty(hostEntry.HostName)
                && !hostEntry.HostName.Equals(localIp.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return hostEntry.HostName;
            }
        }
        catch (SocketException)
        {
            // The DNS lookup failed; we will fallback to using the IP.
        }

        // Fallback: return the string representation of the local IP.
        return localIp.ToString();
    }
}