using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class TwitchChatterListener
    {
        private Thread _twitchChatterListener;
        private TwitchInfoService _twitchInfo;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;

        // Empty constructor makes instance of Thread
        public TwitchChatterListener(TwitchInfoService twitchInfo)
        {
            _twitchChatterListener = new Thread(new ThreadStart(this.Run));
            _twitchInfo = twitchInfo;
        }

        // Starts the thread
        public void Start()
        {
            _twitchChatterListener.IsBackground = true;
            _twitchChatterListener.Start();
        }

        /// <summary>
        /// Check if follower is watching. If so, give following viewer experience every iteration
        /// </summary>
        private void Run()
        {
            while (true)
            {
                CheckChatters().Wait();
                Thread.Sleep(15000); // 15 seconds
            }
        }

        private async Task CheckChatters()
        {
            try
            {
                _twitchChatterListInstance.ListsAvailable = false;
                await ResetChatterListByType();
                ResetChatterListByName();
                _twitchChatterListInstance.ListsAvailable = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inside TwitchChatterListener Run(): " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        /// <summary>
        /// Set a full list of chatters broken up by each type
        /// </summary>
        private async Task ResetChatterListByType()
        {
            _twitchChatterListInstance.ChattersByType.Clear();

            // Grab user's chatter info (viewers, mods, etc.)
            ChatterInfoJSON chatterInfo = await _twitchInfo.GetChatters();

            if (chatterInfo.ChatterCount > 0)
            {
                Chatters chatters = chatterInfo.Chatters; // get list of chatters

                if (chatters.Viewers.Count() > 0)
                {
                    _twitchChatterListInstance.ChattersByType.Add(
                        new TwitchChatterType
                        {
                            Usernames = chatters.Viewers,
                            ChatterType = ChatterType.Viewer
                        }
                    );
                }
                if (chatters.Moderators.Count() > 0)
                {
                    _twitchChatterListInstance.ChattersByType.Add(
                        new TwitchChatterType
                        {
                            Usernames = chatters.Moderators,
                            ChatterType = ChatterType.Moderator
                        }
                    );
                }
                if (chatters.GlobalMods.Count() > 0)
                {
                    _twitchChatterListInstance.ChattersByType.Add(
                        new TwitchChatterType
                        {
                            Usernames = chatters.GlobalMods,
                            ChatterType = ChatterType.GlobalModerator
                        }
                    );
                }
                if (chatters.Admins.Count() > 0)
                {
                    _twitchChatterListInstance.ChattersByType.Add(
                        new TwitchChatterType
                        {
                            Usernames = chatters.Admins,
                            ChatterType = ChatterType.Admin
                        }
                    );
                }
                if (chatters.Staff.Count() > 0)
                {
                    _twitchChatterListInstance.ChattersByType.Add(
                        new TwitchChatterType
                        {
                            Usernames = chatters.Staff,
                            ChatterType = ChatterType.Staff
                        }
                    );
                }
            }
        }

        /// <summary>
        /// Set a full list of chatters by name
        /// </summary>
        private void ResetChatterListByName()
        {
            _twitchChatterListInstance.ChattersByName.Clear();

            if (_twitchChatterListInstance.ChattersByType.Count > 0)
            {
                foreach (TwitchChatterType chatterType in _twitchChatterListInstance.ChattersByType)
                {
                    foreach (string chatter in chatterType.Usernames)
                    {
                        _twitchChatterListInstance.ChattersByName.Add(chatter);
                    }
                }
            }
        }
    }
}
