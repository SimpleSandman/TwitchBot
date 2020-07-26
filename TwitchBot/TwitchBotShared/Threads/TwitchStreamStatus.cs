using System;
using System.Threading;
using System.Threading.Tasks;

using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.ClientLibraries;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.Threads
{
    public class TwitchStreamStatus
    {
        private readonly IrcClient _irc;
        private readonly Thread _checkStreamStatus;
        private readonly TwitchInfoService _twitchInfo;
        private readonly string _broadcasterName;
        private readonly DelayedMessagesSingleton _delayedMessagesInstance = DelayedMessagesSingleton.Instance;

        public static bool IsLive { get; private set; } = false;
        public static string CurrentCategory { get; private set; }
        public static string CurrentTitle { get; private set; }

        public TwitchStreamStatus(IrcClient irc, TwitchInfoService twitchInfo, string broadcasterName)
        {
            _irc = irc;
            _twitchInfo = twitchInfo;
            _broadcasterName = broadcasterName;
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
                        // ToDo: Add setting if user wants preset reminder
                        _delayedMessagesInstance.DelayedMessages.Add(new DelayedMessage
                        {
                            Message = $"Did you remind Twitter you're \"!live\"? @{_broadcasterName}",
                            SendDate = DateTime.Now.AddMinutes(5)
                        });

                        _irc.SendPublicChatMessage($"Live on Twitch playing {CurrentCategory} \"{CurrentTitle}\"");
                    }

                    IsLive = true;
                }

                Thread.Sleep(15000); // check every 15 seconds
            }
        }
    }
}
