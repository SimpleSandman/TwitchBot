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
using Google.Apis.YouTube.v3.Data;

using TwitchBotDb.Temp;

using TwitchBotWpf.Libraries;

namespace TwitchBotWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

        public MainWindow()
        {
            InitializeComponent();

            // check if user has given permission to their YouTube account
            YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();
            bool hasYoutubeAuth = Dispatcher.Invoke(() => _youTubeClientInstance.GetAuthAsync(youtubePlaylistInfo.ClientId, youtubePlaylistInfo.ClientSecret)).Result;

            if (hasYoutubeAuth)
            {
                Task.Factory.StartNew(this.YoutubePlaylistListener);
            }
            
            CefSharpCache.Save(new CefSharpCache());
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

            
            //Browser.ShowDevTools(); // debugging only
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            // ToDo: Find something for loading between videos
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
                    YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();
                    CefSharpCache cefSharpCache = new CefSharpCache { Url = Browser.Address };

                    if (Browser.Address.Contains($"list={youtubePlaylistInfo.Id}") && Browser.Address.Contains("v="))
                    {
                        cefSharpCache.LastPlaylistVideoId = _youTubeClientInstance.GetYouTubeVideoId(Browser.Address);
                    }
                    else
                    {
                        cefSharpCache.LastPlaylistVideoId = CefSharpCache.Load().LastPlaylistVideoId;
                    }

                    CefSharpCache.Save(cefSharpCache);
                }
            }));
        }

        private async Task YoutubePlaylistListener()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                while (true)
                {
                    await Task.Delay(15000); // wait 15 seconds before checking if a video is being played

                    // Check if a video is being played right now
                    // Check if a video from the playlist was played at all
                    // Check if a video was played at all
                    if (Title.IndexOf("<<Playing>>") < 1 && Title.IndexOf("<<Paused>>") < 1)
                    {
                        // Check what video was played last from the song request playlist
                        CefSharpCache cefSharpCache = CefSharpCache.Load();
                        YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();

                        if (string.IsNullOrEmpty(cefSharpCache.LastPlaylistVideoId))
                        {
                            // play first song in the list
                            string firstVideoId = await _youTubeClientInstance.GetFirstPlaylistVideoId(youtubePlaylistInfo.Id, 1);

                            Browser.Load($"https://www.youtube.com/watch?v={firstVideoId}&list={youtubePlaylistInfo.Id}");
                        }
                    }
                }
            });
        }
    }
}
