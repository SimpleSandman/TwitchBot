using System.Threading;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;
using TwitchBotConsoleApp.Models.JSON;
using TwitchBotConsoleApp.Services;

namespace TwitchBotConsoleApp.Threads
{
    public class TwitchStreamStatus
    {
        private readonly IrcClient _irc;
        private readonly Thread _checkStreamStatus;
        private readonly TwitchInfoService _twitchInfo;

        public static bool IsLive { get; private set; } = false;
        public static string CurrentCategory { get; private set; }
        public static string CurrentTitle { get; private set; }

        public TwitchStreamStatus(IrcClient irc, TwitchInfoService twitchInfo)
        {
            _irc = irc;
            _twitchInfo = twitchInfo;
            _checkStreamStatus = new Thread(new ThreadStart(this.Run));
        }

        public void Start()
        {
            _checkStreamStatus.IsBackground = true;
            _checkStreamStatus.Start();
        }

        public async Task LoadChannelInfo()
        {
            ChannelJSON channelJSON = await _twitchInfo.GetBroadcasterChannelById();

            if (channelJSON != null)
            {
                CurrentCategory = channelJSON.Game;
                CurrentTitle = channelJSON.Status;
            }
        }

        private async void Run()
        {
            while (true)
            {
                RootStreamJSON streamJSON = await _twitchInfo.GetBroadcasterStream();

                if (streamJSON.Stream == null)
                {
                    if (IsLive)
                    {
                        // ToDo: Clear greeted user list
                    }

                    IsLive = false;
                }
                else
                {
                    CurrentCategory = streamJSON.Stream.Game;
                    CurrentTitle = streamJSON.Stream.Channel.Status;

                    // tell the chat the stream is now live
                    if (!IsLive)
                    {
                        _irc.SendPublicChatMessage($"Live on Twitch playing {CurrentCategory} \"{CurrentTitle}\"");
                    }

                    IsLive = true;
                }

                Thread.Sleep(15000); // check every 15 seconds
            }
        }
    }
}
