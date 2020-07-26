using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TwitchBotShared.Extensions
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Used to navigate to a URL based on the OS platform
        /// </summary>
        /// <param name="process"></param>
        /// <param name="url"></param>
        public static void StartUrlCrossPlatform(this Process process, string url)
        {
            try
            {
                process.Start();
            }
            catch
            {
                // Reference: https://stackoverflow.com/a/4580317/2113548
                // Hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");

                    process.StartInfo = new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true };
                    process.Start();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    process.StartInfo.FileName = "xdg-open";
                    process.StartInfo.Arguments = url;
                    process.Start();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    process.StartInfo.FileName = "open";
                    process.StartInfo.Arguments = url;
                    process.Start();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
