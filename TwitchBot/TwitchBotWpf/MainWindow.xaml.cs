using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using CefSharp;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

using Newtonsoft.Json;

using TwitchBotDb.Temp;

using TwitchBotWpf.Extensions;

namespace TwitchBotWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YouTubeService _youtubeService;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Browser.Dispose();
        }

        private void ChromiumWebBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Browser.TitleChanged += Browser_TitleChanged;
            Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            Browser.ConsoleMessage += Browser_ConsoleMessage;

            if (Dispatcher.Invoke(GetAuth).Result)
                Task.Factory.StartNew(this.YoutubePlaylistListener);

            //Browser.ShowDevTools(); // debugging only
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Dispatcher.BeginInvoke((Action) (() => 
            {
                int index = Title.IndexOf("<<Playing>>") > 1 ? Title.IndexOf("<<Playing>>") : Title.IndexOf("<<Paused>>");

                if (index > -1)
                {
                    if (e.Message == "Video is playing")
                        Title = Title.Replace(Title.Substring(index), "<<Playing>>");
                    else if (e.Message == "Video is not playing")
                        Title = Title.Replace(Title.Substring(index), "<<Paused>>");
                }
                else
                {
                    if (e.Message == "Video is playing")
                        Title += " <<Playing>>";
                    else if (e.Message == "Video is not playing")
                        Title += " <<Paused>>";
                }
            }));
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            Browser.ExecuteScriptAsync(@"
                var youtubeMoviePlayer = document.getElementById('movie_player');

                var observer = new MutationObserver(function (event) {
                    twitchBotPlaybackStatus(event[0].target.className)   
                })

                observer.observe(youtubeMoviePlayer, {
                    attributes: true, 
                    attributeFilter: ['class'],
                    childList: false, 
                    characterData: false,
                    subtree: false
                })

                function twitchBotPlaybackStatus(mpClassAttr) {
                    if (mpClassAttr.includes('playing-mode')) {
                        console.log('Video is playing');
                    } else if (mpClassAttr.includes('paused-mode') || mpClassAttr.includes('ended-mode')) {
                        console.log('Video is not playing');
                    } else {
                        console.log('Cannot find video player');
                    }
                }
            ");
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();

            if (!string.IsNullOrEmpty(youtubePlaylistInfo.Id))
                Browser.Load($"https://www.youtube.com/playlist?list={youtubePlaylistInfo.Id}");
            else
                Browser.Load("https://www.youtube.com/");
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Title = Browser.Title.Replace("- YouTube", "");

            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (Browser.IsLoaded)
                {
                    CefSharpCache csCache = new CefSharpCache
                    {
                        Url = Browser.Address
                    };

                    // ToDo: Store file name and path into config file
                    string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchBot");
                    string filename = "CefSharpCache.json";

                    Directory.CreateDirectory(filepath);

                    using (StreamWriter file = File.CreateText($"{filepath}\\{filename}"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, csCache);
                    }
                }
            }));
        }

        private async Task YoutubePlaylistListener()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                // do stuff here...please?!

                Task.Delay(15000);
            });
        }

        /// <summary>
        /// Get access and refresh tokens from user's account
        /// </summary>
        private async Task<bool> GetAuth()
        {
            try
            {
                YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();

                string clientSecrets = @"{ 'installed': {'client_id': '" + youtubePlaylistInfo.ClientId + 
                    "', 'client_secret': '" + youtubePlaylistInfo.ClientSecret + "'} }";

                UserCredential credential;
                using (Stream stream = clientSecrets.ToStream())
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { YouTubeService.Scope.Youtube },
                        "user",
                        CancellationToken.None,
                        new FileDataStore("Twitch Bot")
                    );
                }

                _youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Twitch Bot"
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
