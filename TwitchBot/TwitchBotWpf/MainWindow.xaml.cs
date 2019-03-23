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

        private const string _djEnabledStatus = "==Bot DJ Mode ON==";
        private const string _djDisabledStatus = "==Bot DJ Mode OFF==";

        private static readonly object _findNextVideoLock = new object();

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
            Dispatcher.BeginInvoke((Action) (() => 
            {
                // Find index of video status
                int startIndex = -1;
                int endingIndex = Title.IndexOf(">>") + 2;
                
                if (Title.IndexOf(_playingStatus) > 1)
                    startIndex = Title.IndexOf(_playingStatus);
                else if (Title.IndexOf(_pausedStatus) > 1)
                    startIndex = Title.IndexOf(_pausedStatus);
                else if (Title.IndexOf(_bufferingStatus) > 1)
                    startIndex = Title.IndexOf(_bufferingStatus);
                else if (Title.IndexOf(_endedStatus) > 1)
                    startIndex = Title.IndexOf(_endedStatus);

                if (startIndex > -1)
                {
                    if (e.Message == _playingMessage && !Title.Contains(_playingStatus))
                        Title = Title.Replace(Title.Substring(startIndex, endingIndex - startIndex), _playingStatus);
                    else if (e.Message == _pausedMessage && !Title.Contains(_pausedStatus))
                        Title = Title.Replace(Title.Substring(startIndex, endingIndex - startIndex), _pausedStatus);
                    else if (e.Message == _bufferingMessage && !Title.Contains(_bufferingStatus))
                        Title = Title.Replace(Title.Substring(startIndex, endingIndex - startIndex), _bufferingStatus);
                    else if (e.Message == _endedMessage && !Title.Contains(_endedStatus))
                        Title = Title.Replace(Title.Substring(startIndex, endingIndex - startIndex), _endedStatus);
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
                // and if we haven't recently searched for the next video
                if (YoutubeClient.SongRequestSetting.DjMode
                    && !YoutubeClient.HasLookedForNextVideo
                    && e.Message == _endedMessage 
                    && !Browser.Address.Contains($"list={YoutubeClient.SongRequestSetting.RequestPlaylistId}"))
                {
                    YoutubeClient.HasLookedForNextVideo = LookForNextVideo(YoutubePlaylistInfo.Load());
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
            Dispatcher.BeginInvoke((Action) (async () => 
            {
                YoutubePlaylistInfo youtubePlaylistInfo = YoutubePlaylistInfo.Load();

                YoutubeClient.SongRequestSetting = await ApiBotRequest.GetExecuteTaskAsync<SongRequestSetting>(
                    youtubePlaylistInfo.TwitchBotApiLink + $"songrequestsettings/get/{youtubePlaylistInfo.BroadcasterId}");

                // reset cache file from last runtime
                CefSharpCache.Save(new CefSharpCache { RequestPlaylistId = YoutubeClient.SongRequestSetting.RequestPlaylistId });

                if (!string.IsNullOrEmpty(YoutubeClient.SongRequestSetting.RequestPlaylistId))
                    Browser.Load($"https://www.youtube.com/playlist?list={YoutubeClient.SongRequestSetting.RequestPlaylistId}");
                else
                    Browser.Load("https://www.youtube.com/");
            }));
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Title = Browser.Title.Replace("- YouTube", "");

            DisplayDjMode();

            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (Browser.IsLoaded)
                {
                    // Save the last video that was played from either request or personal playlist by ID
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
            while (true)
            {
                await Task.Delay(10000); // wait 10 seconds before checking if a video is being played

                await Dispatcher.BeginInvoke((Action)(async () =>
                {
                    // Check what video was played last from the song request playlist
                    YoutubeClient.SongRequestSetting = await ApiBotRequest.GetExecuteTaskAsync<SongRequestSetting>(
                        youtubePlaylistInfo.TwitchBotApiLink + $"songrequestsettings/get/{youtubePlaylistInfo.BroadcasterId}");

                    DisplayDjMode();

                    /* 
                     * Check if:
                     * - A video is currently playing/paused/buffering
                     * - Has a valid and loaded URL
                     * - If DJ mode is on
                     * - If the current URL is not looking at the requested or personal playlists
                    */
                    if (!Title.Contains(_playingStatus) 
                        && !Title.Contains(_pausedStatus) 
                        && !Title.Contains(_bufferingStatus) 
                        && !string.IsNullOrEmpty(Browser.Address)
                        && Browser.IsLoaded 
                        && YoutubeClient.SongRequestSetting.DjMode 
                        && !YoutubeClient.HasLookedForNextVideo
                        && (!Browser.Address.Contains($"/playlist?list={YoutubeClient.SongRequestSetting.RequestPlaylistId}")
                            && (!string.IsNullOrEmpty(YoutubeClient.SongRequestSetting.PersonalPlaylistId)
                                    && !Browser.Address.Contains($"/playlist?list={YoutubeClient.SongRequestSetting.PersonalPlaylistId}")
                                )
                            )
                        )
                    {
                        YoutubeClient.HasLookedForNextVideo = LookForNextVideo(youtubePlaylistInfo);
                    }
                    else
                    {
                        YoutubeClient.HasLookedForNextVideo = false;
                    }
                }));
            }
        }

        private bool LookForNextVideo(YoutubePlaylistInfo youtubePlaylistInfo)
        {
            lock (_findNextVideoLock)
            {
                // Check if the YouTube progress bar is showing
                const string script = @"
                    (function() 
                    {
                        return !!(document.getElementsByTagName('yt-page-navigation-progress').offsetParent);
                    })();";

                var response = Browser.EvaluateScriptAsync(script).Result;
                bool valid = bool.TryParse(response.Result?.ToString(), out bool isProgressBarVisible);

                if (response.Message == null && response.Success && valid && isProgressBarVisible)
                {
                    return false; // don't look until the progress bar is gone
                }

                CefSharpCache loadedCefSharpCache = CefSharpCache.Load();

                // Check if request playlist ID has been reset
                // If so, reset the last requested video ID
                if (loadedCefSharpCache.RequestPlaylistId != YoutubeClient.SongRequestSetting.RequestPlaylistId)
                {
                    loadedCefSharpCache.RequestPlaylistId = YoutubeClient.SongRequestSetting.RequestPlaylistId;
                    loadedCefSharpCache.LastRequestPlaylistVideoId = null;
                    CefSharpCache.Save(loadedCefSharpCache);
                }

                /* Check if a video from the request playlist was played at all */
                if (string.IsNullOrEmpty(loadedCefSharpCache.LastRequestPlaylistVideoId))
                {
                    LoadFirstPlaylistVideo(YoutubeClient.SongRequestSetting.RequestPlaylistId);
                }
                else
                {
                    if (LoadNextPlaylistVideo(YoutubeClient.SongRequestSetting.RequestPlaylistId, loadedCefSharpCache.LastRequestPlaylistVideoId))
                    {
                        return true;
                    }
                    else if (!string.IsNullOrEmpty(YoutubeClient.SongRequestSetting.PersonalPlaylistId))
                    {
                        /* Check if a video from the personal playlist was played at all */
                        if (string.IsNullOrEmpty(loadedCefSharpCache.LastPersonalPlaylistVideoId))
                        {
                            LoadFirstPlaylistVideo(YoutubeClient.SongRequestSetting.PersonalPlaylistId);
                        }
                        else
                        {
                            LoadNextPlaylistVideo(YoutubeClient.SongRequestSetting.PersonalPlaylistId, loadedCefSharpCache.LastPersonalPlaylistVideoId);
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Play first song in the playlist
        /// </summary>
        /// <param name="playlistId"></param>
        private void LoadFirstPlaylistVideo(string playlistId)
        {
            string firstVideoId = _youTubeClientInstance.GetFirstPlaylistVideoId(playlistId);

            if (!string.IsNullOrEmpty(firstVideoId))
                Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={firstVideoId}&list={playlistId}");
        }

        /// <summary>
        /// Play the next song in the playlist based on last played video from that playlist
        /// </summary>
        /// <param name="playlistId"></param>
        /// <param name="lastPlayedVideoId"></param>
        /// <returns></returns>
        private bool LoadNextPlaylistVideo(string playlistId, string lastPlayedVideoId)
        {
            string nextRequestedVideoId = _youTubeClientInstance.GetNextPlaylistVideoId(playlistId, lastPlayedVideoId);

            if (!string.IsNullOrEmpty(nextRequestedVideoId))
            {
                Browser.GetMainFrame().LoadUrl($"https://www.youtube.com/watch?v={nextRequestedVideoId}&list={playlistId}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Display the bot's current DJ mode in the Main Window title bar
        /// </summary>
        private void DisplayDjMode()
        {
            int djIndex = Title.IndexOf(_djEnabledStatus) > 1 ? Title.IndexOf(_djEnabledStatus) : Title.IndexOf(_djDisabledStatus);

            if (YoutubeClient.SongRequestSetting != null)
            {
                if (djIndex > 1)
                {
                    if (YoutubeClient.SongRequestSetting.DjMode && Title.Contains(_djDisabledStatus))
                        Title = Title.Replace(_djDisabledStatus, _djEnabledStatus);
                    else if (!YoutubeClient.SongRequestSetting.DjMode && Title.Contains(_djEnabledStatus))
                        Title = Title.Replace(_djEnabledStatus, _djDisabledStatus);
                }
                else
                {
                    if (YoutubeClient.SongRequestSetting.DjMode)
                        Title += $" {_djEnabledStatus}";
                    else
                        Title += $" {_djDisabledStatus}";
                }
            }
        }
    }
}
