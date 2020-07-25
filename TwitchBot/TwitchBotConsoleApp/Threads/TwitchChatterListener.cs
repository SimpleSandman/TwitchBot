using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;

using TwitchBotDb.Services;

using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;

namespace TwitchBotConsoleApp.Threads
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
                _twitchChatterListInstance.AreListsAvailable = false;
                await ResetChatterLists();
                _twitchChatterListInstance.AreListsAvailable = true;
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
        private async Task ResetChatterLists()
        {
            try
            {
                // Grab user's chatter info (viewers, mods, etc.)
                using (HttpResponseMessage message = await _twitchInfo.GetChatters())
                {
                    if (!message.IsSuccessStatusCode)
                    {
                        return;
                    }

                    string body = await message.Content.ReadAsStringAsync();
                    ChatterInfoJSON chatterInfo = JsonConvert.DeserializeObject<ChatterInfoJSON>(body);

                    _twitchChatterListInstance.ChattersByName.Clear();
                    _twitchChatterListInstance.ChattersByType.Clear();

                    if (chatterInfo.ChatterCount > 0)
                    {
                        Chatters chatters = chatterInfo.Chatters;

                        // Grab and divide chatters from tmi.twitch.tv
                        if (chatters.Viewers.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = GroupTmiTwitchChatters(chatters.Viewers),
                                    ChatterType = ChatterType.Viewer
                                }
                            );
                        }
                        if (chatters.VIPs.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = GroupTmiTwitchChatters(chatters.VIPs),
                                    ChatterType = ChatterType.VIP
                                }
                            );
                        }
                        if (chatters.Moderators.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = GroupTmiTwitchChatters(chatters.Moderators),
                                    ChatterType = ChatterType.Moderator
                                }
                            );
                        }
                        if (chatters.GlobalMods.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = GroupTmiTwitchChatters(chatters.GlobalMods),
                                    ChatterType = ChatterType.GlobalModerator
                                }
                            );
                        }
                        if (chatters.Admins.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = GroupTmiTwitchChatters(chatters.Admins),
                                    ChatterType = ChatterType.Admin
                                }
                            );
                        }
                        if (chatters.Staff.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = GroupTmiTwitchChatters(chatters.Staff),
                                    ChatterType = ChatterType.Staff
                                }
                            );
                        }

                        // Set followers, regular followers, and subscribers
                        if (_twitchChatterListInstance.TwitchFollowers.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = _twitchChatterListInstance.TwitchFollowers,
                                    ChatterType = ChatterType.Follower
                                }
                            );
                        }

                        if (_twitchChatterListInstance.TwitchRegularFollowers.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = _twitchChatterListInstance.TwitchRegularFollowers,
                                    ChatterType = ChatterType.RegularFollower
                                }
                            );
                        }

                        if (_twitchChatterListInstance.TwitchSubscribers.Count > 0)
                        {
                            _twitchChatterListInstance.ChattersByType.Add
                            (
                                new TwitchChatterType
                                {
                                    TwitchChatters = _twitchChatterListInstance.TwitchSubscribers,
                                    ChatterType = ChatterType.Subscriber
                                }
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inside TwitchChatterListener ResetChatterLists(): " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private List<TwitchChatter> GroupTmiTwitchChatters(List<string> usernamesByType)
        {
            List<TwitchChatter> listChattersByViewers = new List<TwitchChatter>();

            foreach (string username in usernamesByType)
            {
                TwitchChatter chatter = new TwitchChatter { Username = username, CreatedAt = null };

                _twitchChatterListInstance.ChattersByName.Add(username);

                listChattersByViewers.Add(chatter); // used for organizing ChattersByType list
            }

            return listChattersByViewers;
        }
    }
}
