using System;
using System.Threading.Tasks;
using System.Windows;

using CefSharp;

using TwitchBotDb.Models;
using TwitchBotDb.Temp;

using TwitchBotUtil.Libraries;

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
        private const string _endedStatus = "<<Ended>>";
        private const string _bufferingStatus = "<<Buffering>>";

        private const string _playingMessage = "Video is playing";
        private const string _pausedMessage = "Video is paused";
        private const string _endedMessage = "Video has ended";
        private const string _bufferingMessage = "Video is buffering";

        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

        public MainWindow()
        {
            InitializeComponent();

            Browser.MenuHandler = new MenuHandler();

            #if DEBUG
                System.Threading.Thread.Sleep(5000);
            #endif

            // check if user has given permission to their YouTube account
            YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();
            bool hasYoutubeAuth = Dispatcher.Invoke(() => _youTubeClientInstance.GetAuthAsync(youtubePlaylistInfo.ClientId, youtubePlaylistInfo.ClientSecret)).Result;

            CefSharpCache.Save(new CefSharpCache()); // reset cache file

            if (hasYoutubeAuth)
            {
                Task.Factory.StartNew(() => YoutubePlaylistListener(youtubePlaylistInfo));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Browser.Dispose();
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Dispatcher.BeginInvoke((Action) (async () => 
            {
                int index = -1;

                // Find index of video status
                if (Title.IndexOf(_playingStatus) > 1)
                    index = Title.IndexOf(_playingStatus);
                else if (Title.IndexOf(_pausedStatus) > 1)
                    index = Title.IndexOf(_pausedStatus);
                else if (Title.IndexOf(_bufferingStatus) > 1)
                    index = Title.IndexOf(_bufferingStatus);
                else if (Title.IndexOf(_endedStatus) > 1)
                    index = Title.IndexOf(_endedStatus);

                if (index > -1)
                {
                    if (e.Message == _playingMessage)
                        Title = Title.Replace(Title.Substring(index), _playingStatus);
                    else if (e.Message == _pausedMessage)
                        Title = Title.Replace(Title.Substring(index), _pausedStatus);
                    else if (e.Message == _bufferingMessage)
                        Title = Title.Replace(Title.Substring(index), _bufferingStatus);
                    else if (e.Message == _endedMessage)
                        Title = Title.Replace(Title.Substring(index), _endedStatus);
                }
                else
                {
                    if (e.Message == _playingMessage)
                        Title += $" {_playingStatus}";
                    else if (e.Message == _pausedMessage)
                        Title += $" {_pausedStatus}";
                    else if (e.Message == _bufferingMessage)
                        Title += $" {_bufferingStatus}";
                    else if (e.Message == _endedMessage)
                        Title += $" {_endedStatus}";
                }

                // When DJ mode is enabled, check when the last video of a playlist has ended 
                // that isn't from the song request playlist
                if (YoutubeClient.SongRequestSetting.DjMode
                    && e.Message == _endedMessage 
                    && !Browser.Address.Contains($"list={YoutubeClient.SongRequestSetting.RequestPlaylistId}"))
                {
                    await FindNextVideo(YoutubePlaylistInfo.Load());
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
                    if (mpClassAttr.includes('paused-mode')) {
                        console.log('" + _pausedMessage + @"');
                    } else if (mpClassAttr.includes('ended-mode')) {
                        console.log('" + _endedMessage + @"');
                    } else if (mpClassAttr.includes('buffering-mode')) {
                        console.log('" + _bufferingMessage + @"');
                    } else if (mpClassAttr.includes('playing-mode')) {
                        console.log('" + _playingMessage + @"');
                    } else {
                        console.log('Cannot find video player');
                    }
                }
            ");
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(YoutubeClient.SongRequestSetting.RequestPlaylistId))
                Browser.Load($"https://www.youtube.com/playlist?list={YoutubeClient.SongRequestSetting.RequestPlaylistId}");
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
                    CefSharpCache loadedCefSharpCache = CefSharpCache.Load();
                    loadedCefSharpCache.Url = Browser.Address;

                    if (Browser.Address.Contains($"list={YoutubeClient.SongRequestSetting.RequestPlaylistId}") 
                        && Browser.Address.Contains("v="))
                    {
                        loadedCefSharpCache.LastRequestPlaylistVideoId = _youTubeClientInstance.GetYouTubeVideoId(Browser.Address);
                    }
                    else if (Browser.Address.Contains($"list={YoutubeClient.SongRequestSetting.PersonalPlaylistId}")
                        && Browser.Address.Contains("v="))
                    {
                        loadedCefSharpCache.LastPersonalPlaylistVideoId = _youTubeClientInstance.GetYouTubeVideoId(Browser.Address);
                    }

                    CefSharpCache.Save(loadedCefSharpCache);
                }
            }));
        }

        private async Task YoutubePlaylistListener(YoutubePlaylistInfo youtubePlaylistInfo)
        {
            YoutubeClient.SongRequestSetting = ApiBotRequest.GetExecuteTaskAsync<SongRequestSetting>(
                youtubePlaylistInfo.TwitchBotApiLink + $"songrequestsettings/get/{youtubePlaylistInfo.BroadcasterId}").Result;

            while (true)
            {
                await Task.Delay(10000); // wait 10 seconds before checking if a video is being played

                await Dispatcher.InvokeAsync(async () =>
                {
                    // Check if a video is being played right now
                    if (!Title.Contains(_playingStatus) && !Title.Contains(_pausedStatus) && !Title.Contains(_bufferingStatus) && Browser.IsLoaded)
                    {
                        // Check what video was played last from the song request playlist
                        YoutubeClient.SongRequestSetting = await ApiBotRequest.GetExecuteTaskAsync<SongRequestSetting>(
                            youtubePlaylistInfo.TwitchBotApiLink + $"songrequestsettings/get/{youtubePlaylistInfo.BroadcasterId}");

                        // Check if DJ mode is on and if the current URL is not looking at the requested or personal playlists
                        if (YoutubeClient.SongRequestSetting.DjMode
                            && (!Browser.Address.Contains("/playlist?list=" + YoutubeClient.SongRequestSetting.RequestPlaylistId)
                                && (!string.IsNullOrEmpty(YoutubeClient.SongRequestSetting.PersonalPlaylistId)
                                    && !Browser.Address.Contains("/playlist?list=" + YoutubeClient.SongRequestSetting.PersonalPlaylistId)
                                    )
                                )
                            )
                        {
                            await FindNextVideo(youtubePlaylistInfo);
                        }
                    }
                });
            }
        }

        private async Task FindNextVideo(YoutubePlaylistInfo youtubePlaylistInfo)
        {
            CefSharpCache cefSharpCache = CefSharpCache.Load();

            /* Check if a video from the request playlist was played at all */
            if (string.IsNullOrEmpty(cefSharpCache.LastRequestPlaylistVideoId))
            {
                // play first song in the list
                string firstRequestedVideoId = await _youTubeClientInstance.GetFirstPlaylistVideoId(YoutubeClient.SongRequestSetting.RequestPlaylistId);

                Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={firstRequestedVideoId}&list={YoutubeClient.SongRequestSetting.RequestPlaylistId}");
            }
            else
            {
                // find the next song in the playlist
                string nextRequestedVideoId = await _youTubeClientInstance.GetNextPlaylistVideoId(YoutubeClient.SongRequestSetting.RequestPlaylistId, cefSharpCache.LastRequestPlaylistVideoId);

                if (!string.IsNullOrEmpty(nextRequestedVideoId))
                {
                    Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={nextRequestedVideoId}&list={YoutubeClient.SongRequestSetting.RequestPlaylistId}");
                }
                else if (!string.IsNullOrEmpty(YoutubeClient.SongRequestSetting.PersonalPlaylistId))
                {
                    /* Check if a video from the personal playlist was played at all */
                    if (string.IsNullOrEmpty(cefSharpCache.LastPersonalPlaylistVideoId))
                    {
                        // play first song in the personal list
                        string firstPersonalVideoId = await _youTubeClientInstance.GetFirstPlaylistVideoId(YoutubeClient.SongRequestSetting.PersonalPlaylistId);

                        Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={firstPersonalVideoId}&list={YoutubeClient.SongRequestSetting.PersonalPlaylistId}");
                    }
                    else
                    {
                        // find the next song in the playlist
                        string nextPersonalVideoId = await _youTubeClientInstance.GetNextPlaylistVideoId(YoutubeClient.SongRequestSetting.PersonalPlaylistId, cefSharpCache.LastPersonalPlaylistVideoId);

                        if (!string.IsNullOrEmpty(nextPersonalVideoId))
                        {
                            Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={nextPersonalVideoId}&list={YoutubeClient.SongRequestSetting.PersonalPlaylistId}");
                        }
                    }
                }
            }
        }
    }
}
