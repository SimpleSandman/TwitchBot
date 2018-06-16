using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

using TwitchBot.Extensions;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class ChatReminder
    {
        private Thread _chatReminderThread;
        private IrcClient _irc;
        private static string _connStr;
        private static bool _refreshReminders;
        private static int _broadcasterId;
        private static string _twitchClientId;
        private int? _gameId;
        private int _lastSecCountdownReminder;
        private static List<Reminder> _reminders;
        private GameDirectoryService _gameDirectory;

        public ChatReminder(IrcClient irc, int broadcasterId, string connStr, string twitchClientId, GameDirectoryService gameDirectory)
        {
            _irc = irc;
            _broadcasterId = broadcasterId;
            _connStr = connStr;
            _twitchClientId = twitchClientId;
            _gameDirectory = gameDirectory;
            _lastSecCountdownReminder = -10;
            _refreshReminders = false;
            _chatReminderThread = new Thread (new ThreadStart (this.Run));
        }

        public void Start()
        {
            _chatReminderThread.IsBackground = true;
            _chatReminderThread.Start(); 
        }

        private async void Run()
        {
            LoadReminderContext(); // initial load
            DateTime midnightNextDay = DateTime.Today.AddDays(1);

            while (true)
            {
                ChannelJSON channelJSON = await TwitchApi.GetBroadcasterChannelById(_twitchClientId);
                string gameTitle = channelJSON.Game;

                TwitchBotDb.Models.GameList game = await _gameDirectory.GetGameId(gameTitle);

                if (game == null || game.Id == 0)
                    _gameId = null;
                else
                    _gameId = game.Id;

                // remove pending reminders
                Program.DelayedMessages.RemoveAll(r => r.ReminderId > 0);

                foreach (Reminder reminder in _reminders.OrderBy(m => m.RemindEveryMin))
                {
                    if (IsEveryMinReminder(reminder)) continue;
                    else if (IsCountdownEvent(reminder)) continue;
                    else AddDayOfReminder(reminder);
                }

                if (_refreshReminders)
                    _irc.SendPublicChatMessage("Reminders refreshed!");

                // reset refresh
                midnightNextDay = DateTime.Today.AddDays(1);
                _refreshReminders = false;

                // wait until midnight to check reminders
                // unless a manual refresh was called
                while (DateTime.Now < midnightNextDay && !_refreshReminders)
                {
                    Thread.Sleep(1000); // 1 second
                }
            }
        }

        /// <summary>
        /// Manual refresh of reminders
        /// </summary>
        /// <returns></returns>
        public static void RefreshReminders()
        {
            LoadReminderContext();
            _refreshReminders = true;
        }

        /// <summary>
        /// Load reminders from database
        /// </summary>
        private static void LoadReminderContext()
        {
            _reminders = new List<Reminder>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                // do not show any expired reminders
                string query = "SELECT * FROM Reminders " 
                    + "WHERE Broadcaster = @broadcaster " 
                        + "AND (ExpirationDateUtc IS NULL OR ExpirationDateUtc > GETDATE())";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                _reminders.Add(new Reminder
                                {
                                    Id = int.Parse(reader["Id"].ToString()),
                                    GameId = reader["Game"].ToString().ToNullableInt(),
                                    IsReminderDay = new bool[7]
                                    {
                                        bool.Parse(reader["Sunday"].ToString()),
                                        bool.Parse(reader["Monday"].ToString()),
                                        bool.Parse(reader["Tuesday"].ToString()),
                                        bool.Parse(reader["Wednesday"].ToString()),
                                        bool.Parse(reader["Thursday"].ToString()),
                                        bool.Parse(reader["Friday"].ToString()),
                                        bool.Parse(reader["Saturday"].ToString())
                                    },
                                    ReminderSeconds = new int?[]
                                    {
                                        reader["ReminderSec1"].ToString().ToNullableInt(),
                                        reader["ReminderSec2"].ToString().ToNullableInt(),
                                        reader["ReminderSec3"].ToString().ToNullableInt(),
                                        reader["ReminderSec4"].ToString().ToNullableInt(),
                                        reader["ReminderSec5"].ToString().ToNullableInt()
                                    },
                                    TimeOfEvent = reader["TimeOfEventUtc"].ToString().ToNullableTimeSpan(),
                                    ExpirationDate = reader["ExpirationDateUtc"].ToString().ToNullableDateTime(),
                                    RemindEveryMin = reader["RemindEveryMin"].ToString().ToNullableInt(),
                                    Message = reader["Message"].ToString(),
                                    IsCountdownEvent = bool.Parse(reader["IsCountdownEvent"].ToString()),
                                    HasCountdownTicker = bool.Parse(reader["HasCountdownTicker"].ToString())
                                });
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if reminder is set for a specific game
        /// </summary>
        /// <param name="reminder"></param>
        /// <returns></returns>
        private bool IsGameReminderBasedOnSetGame(Reminder reminder)
        {
            if (reminder.GameId != _gameId)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add up to 5 reminders at user-defined seconds before the event happens
        /// </summary>
        /// <param name="reminder"></param>
        /// <param name="dateTimeOfEvent"></param>
        private void AddCustomReminderSeconds(Reminder reminder, DateTime dateTimeOfEvent)
        {
            foreach (int? reminderSecond in reminder.ReminderSeconds)
            {
                if (reminderSecond == null || reminderSecond <= 0) continue;

                if (reminder.HasCountdownTicker && reminderSecond <= Math.Abs(_lastSecCountdownReminder)) continue;

                DateTime reminderTime = dateTimeOfEvent.AddSeconds(-(double)reminderSecond);
                TimeSpan timeSpan = dateTimeOfEvent.Subtract(reminderTime);

                if (DateTime.Now < reminderTime)
                {
                    Program.DelayedMessages.Add(new DelayedMessage
                    {
                        ReminderId = reminder.Id,
                        Message = $"{timeSpan.ToReadableString()} until \"{reminder.Message}\"",
                        SendDate = reminderTime
                    });
                }
            }
        }

        /// <summary>
        /// Add a preset decremental countdown if ticker set (starting 10 seconds before the event begins)
        /// </summary>
        /// <param name="reminder"></param>
        /// <param name="dateTimeOfEvent"></param>
        private void AddPresetCountdownSeconds(Reminder reminder, DateTime dateTimeOfEvent)
        {
            if (reminder.HasCountdownTicker)
            {
                // last second reminder before countdown begins
                Program.DelayedMessages.Add(new DelayedMessage
                {
                    ReminderId = reminder.Id,
                    Message = $"{Math.Abs(_lastSecCountdownReminder)} seconds until \"{reminder.Message}\"",
                    SendDate = dateTimeOfEvent.AddSeconds(_lastSecCountdownReminder)
                });

                // set up countdown messages
                for (int i = 5; i > 0; i--)
                {
                    Program.DelayedMessages.Add(new DelayedMessage
                    {
                        ReminderId = reminder.Id,
                        Message = $"{i}",
                        SendDate = dateTimeOfEvent.AddSeconds(-i)
                    });
                }
            }
        }

        /// <summary>
        /// Add the announcement message at the event time specified
        /// </summary>
        /// <param name="reminder"></param>
        /// <param name="dateTimeOfEvent"></param>
        private void AddAnnouncementMessage(Reminder reminder, DateTime dateTimeOfEvent)
        {
            Program.DelayedMessages.Add(new DelayedMessage
            {
                ReminderId = reminder.Id,
                Message = $"It's time for \"{reminder.Message}\"",
                SendDate = dateTimeOfEvent
            });
        }

        /// <summary>
        /// Check if reminder is on a certain minute-based interval.
        /// If so, add it to delayed messages queue
        /// </summary>
        /// <param name="reminder"></param>
        /// <returns></returns>
        private bool IsEveryMinReminder(Reminder reminder)
        {
            /* Set any reminders that happen every X minutes */
            if (reminder.RemindEveryMin != null
                && reminder.IsReminderDay[(int)DateTime.Now.DayOfWeek]
                && !Program.DelayedMessages.Any(m => m.Message.Contains(reminder.Message)))
            {
                if (reminder.GameId != null && !IsGameReminderBasedOnSetGame(reminder))
                {
                    return false;
                }

                int sameReminderMinCount = _reminders.Count(r => r.RemindEveryMin == reminder.RemindEveryMin && (r.GameId == _gameId || r.GameId == null));
                double dividedSeconds = ((double)reminder.RemindEveryMin * 60) / sameReminderMinCount;

                int sameDelayedMinCount = Program.DelayedMessages.Count(m => m.ReminderEveryMin == reminder.RemindEveryMin);
                double setSeconds = dividedSeconds;
                for (int i = 0; i < sameDelayedMinCount; i++)
                {
                    setSeconds += dividedSeconds;
                }

                Program.DelayedMessages.Add(new DelayedMessage
                {
                    ReminderId = reminder.Id,
                    Message = reminder.Message,
                    SendDate = DateTime.Now.AddSeconds(setSeconds),
                    ReminderEveryMin = reminder.RemindEveryMin
                });

                return true;
            }

            return false;
        }

        /// <summary>
        /// Add reminder based on if it hasn't passed that day and time assigned
        /// </summary>
        /// <param name="reminder"></param>
        private void AddDayOfReminder(Reminder reminder)
        {
            if (reminder.TimeOfEvent == null) return;

            /* Set reminders that happen throughout the day */
            DateTime dateTimeOfEvent = DateTime.UtcNow.Date.Add((TimeSpan)reminder.TimeOfEvent).ToLocalTime();

            if (!reminder.IsReminderDay[(int)DateTime.Now.DayOfWeek]
                || dateTimeOfEvent < DateTime.Now
                || Program.DelayedMessages.Any(m => m.Message.Contains(reminder.Message))
                || (reminder.GameId != null && !IsGameReminderBasedOnSetGame(reminder)))
            {
                return; // do not display reminder
            }

            AddCustomReminderSeconds(reminder, dateTimeOfEvent);
            AddPresetCountdownSeconds(reminder, dateTimeOfEvent);
            AddAnnouncementMessage(reminder, dateTimeOfEvent);
        }

        /// <summary>
        /// Add reminder if set to a single time and set up the countdown reminders
        /// </summary>
        /// <param name="reminder"></param>
        private bool IsCountdownEvent(Reminder reminder)
        {
            if (reminder.ExpirationDate == null || !reminder.IsCountdownEvent) return false;

            /* Set countdown event time */
            DateTime dateTimeOfEvent = DateTime.SpecifyKind(reminder.ExpirationDate.Value, DateTimeKind.Utc).ToLocalTime();

            if (dateTimeOfEvent < DateTime.Now
                || Program.DelayedMessages.Any(m => m.Message.Contains(reminder.Message))
                || (reminder.GameId != null && !IsGameReminderBasedOnSetGame(reminder)))
            {
                return false; // do not display countdown
            }

            AddCustomReminderSeconds(reminder, dateTimeOfEvent);
            AddPresetCountdownSeconds(reminder, dateTimeOfEvent);
            AddAnnouncementMessage(reminder, dateTimeOfEvent);

            return true;
        }
    }
}
