using System;
using System.IO;
using System.Windows;

using CefSharp;
using CefSharp.Wpf;

namespace TwitchBotWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Monitor parent process exit and close subprocesses if parent process exits first
            // This will at some point in the future becomes the default
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            CefSettings settings = new CefSettings()
            {
                // By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

            // Used to play videos without user intervention (as of Google Chrome 66)
            // Reference: https://developers.google.com/web/updates/2017/09/autoplay-policy-changes#developer-switches
            settings.CefCommandLineArgs.Add("--autoplay-policy", "no-user-gesture-required");

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Cef.Shutdown(); // ToDo: Try running this in the UI thread (Dispatcher)
            Environment.Exit(0);
        }
    }
}
