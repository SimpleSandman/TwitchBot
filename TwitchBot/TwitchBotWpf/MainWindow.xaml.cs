using System;
using System.Threading.Tasks;
using System.Windows;

using CefSharp;

using TwitchBotDb.Temp;

using TwitchBotWpf.Libraries;
using TwitchBotWpf.Handlers;

namespace TwitchBotWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _playingStatus = "<<Playing>>";
        private const string _pausedStatus = "<<Paused>>";

        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

        public MainWindow()
        {
            InitializeComponent();

            Browser.MenuHandler = new MenuHandler();

            // check if user has given permission to their YouTube account
            YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();
            bool hasYoutubeAuth = Dispatcher.Invoke(() => _youTubeClientInstance.GetAuthAsync(youtubePlaylistInfo.ClientId, youtubePlaylistInfo.ClientSecret)).Result;

            // ToDo: Control listener using a boolean (possibly from youtubePlaylistInfo property)
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

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            // ToDo: Find something for loading between videos
            Dispatcher.BeginInvoke((Action) (() => 
            {
                int index = -1;

                // Find index of video status
                if (Title.IndexOf(_playingStatus) > 1)
                    index = Title.IndexOf(_playingStatus);
                else if (Title.IndexOf(_pausedStatus) > 1)
                    index = Title.IndexOf(_pausedStatus);

                if (index > -1)
                {
                    if (e.Message == "Video is playing")
                        Title = Title.Replace(Title.Substring(index), _playingStatus);
                    else if (e.Message == "Video is not playing")
                        Title = Title.Replace(Title.Substring(index), _pausedStatus);
                }
                else
                {
                    if (e.Message == "Video is playing")
                        Title += $" {_playingStatus}";
                    else if (e.Message == "Video is not playing")
                        Title += $" {_pausedStatus}";
                }
            }));
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            Browser.GetMainFrame().ExecuteJavaScriptAsync(@"
                var youtubeMoviePlayer = document.getElementById('movie_player');

                var observer = new MutationObserver(function (event) {
                    twitchBotPlaybackStatus(event[0].target.className)   
                });

                observer.observe(youtubeMoviePlayer, {
                    attributes: true, 
                    attributeFilter: ['class'],
                    childList: false, 
                    characterData: false,
                    subtree: false
                });

                function twitchBotPlaybackStatus(mpClassAttr) {
                    if (mpClassAttr.includes('paused-mode') || mpClassAttr.includes('ended-mode')) {
                        console.log('Video is not playing');
                    } else if (mpClassAttr.includes('playing-mode')) {
                        console.log('Video is playing');
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

                    if (Browser.Address.Contains($"list={youtubePlaylistInfo.Id}") 
                        && Browser.Address.Contains("v="))
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
            while (true)
            {
                await Task.Delay(10000); // wait 10 seconds before checking if a video is being played

                await Dispatcher.InvokeAsync(async () =>
                {
                    // Check if a video is being played right now
                    if (!Title.Contains(_playingStatus) && !Title.Contains(_pausedStatus))
                    {
                        // Check what video was played last from the song request playlist
                        YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();

                        if (!Title.TrimEnd().Contains(youtubePlaylistInfo.Name))
                        {
                            CefSharpCache cefSharpCache = CefSharpCache.Load();

                            // Check if a video from the playlist was played at all
                            if (string.IsNullOrEmpty(cefSharpCache.LastPlaylistVideoId))
                            {
                                // play first song in the list
                                string firstVideoId = await _youTubeClientInstance.GetFirstPlaylistVideoId(youtubePlaylistInfo.Id);

                                Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={firstVideoId}&list={youtubePlaylistInfo.Id}");
                            }
                            else
                            {
                                // find the next song in the playlist
                                string nextVideoId = await _youTubeClientInstance.GetNextPlaylistVideoId(youtubePlaylistInfo.Id, cefSharpCache.LastPlaylistVideoId);

                                if (!string.IsNullOrEmpty(nextVideoId))
                                {
                                    Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={nextVideoId}&list={youtubePlaylistInfo.Id}");
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
