using System;
using System.Configuration;
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
            CefSettings settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
    }
}
