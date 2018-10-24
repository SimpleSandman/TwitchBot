using System;
using System.IO;
using System.Windows;

using CefSharp;
using CefSharp.Wpf;

using TwitchBotDb.Temp;

namespace TwitchBotWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            CefSettings settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

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
