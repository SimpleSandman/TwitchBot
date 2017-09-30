using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

using TwitchBot.Extensions;
using TwitchBot.Libraries;
using TwitchBot.Models;

namespace TwitchBot.Threads
{
    public class ChatReminder
    {
        private Thread _chatReminderThread;
        private IrcClient _irc;
        private static int _broadcasterId;
        private static string _connStr;
        private static bool _refreshReminders;
        private static List<Reminder> _reminders;

        public ChatReminder(IrcClient irc, int broadcasterId, string connStr)
        {
            _irc = irc;
            _broadcasterId = broadcasterId;
            _connStr = connStr;
            _refreshReminders = false;
            _chatReminderThread = new Thread (new ThreadStart (this.Run));
        }

        public void Start()
        {
            _chatReminderThread.IsBackground = true;
            _chatReminderThread.Start(); 
        }

        public void Run()
        {
            LoadReminderContext(); // initial load
            DateTime midnightNextDay = DateTime.Today.AddDays(1);

            while (true)
            {
                foreach (Reminder reminder in _reminders.OrderBy(m => m.RemindEveryMin))
                {
                    if (IsEveryMinReminder(reminder)) continue;

                    AddDayOfReminder(reminder);
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
            Program.DelayedMessages.RemoveAll(r => r.ReminderId > 0);
        }

        private static void LoadReminderContext()
        {
            _reminders = new List<Reminder>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblReminders WHERE broadcaster = @broadcaster", conn))
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
                                    IsReminderDay = new bool[7]
                                    {
                                        bool.Parse(reader["sunday"].ToString()),
                                        bool.Parse(reader["monday"].ToString()),
                                        bool.Parse(reader["tuesday"].ToString()),
                                        bool.Parse(reader["wednesday"].ToString()),
                                        bool.Parse(reader["thursday"].ToString()),
                                        bool.Parse(reader["friday"].ToString()),
                                        bool.Parse(reader["saturday"].ToString())
                                    },
                                    ReminderSeconds = new int?[]
                                    {
                                        reader["reminderSec1"].ToString().ToNullableInt(),
                                        reader["reminderSec2"].ToString().ToNullableInt(),
                                        reader["reminderSec3"].ToString().ToNullableInt(),
                                        reader["reminderSec4"].ToString().ToNullableInt(),
                                        reader["reminderSec5"].ToString().ToNullableInt()
                                    },
                                    TimeOfEvent = TimeSpan.Parse(reader["timeOfEvent"].ToString()),
                                    RemindEveryMin = reader["remindEveryMin"].ToString().ToNullableInt(),
                                    Message = reader["message"].ToString()
                                });
                            }
                        }
                    }
                }
            }
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
                int sameReminderMinCount = _reminders.Count(r => r.RemindEveryMin == reminder.RemindEveryMin);
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
            /* Set reminders that happen throughout the day */
            DateTime dateTimeOfEvent = DateTime.Today.Date.Add(reminder.TimeOfEvent);
            dateTimeOfEvent = DateTime.SpecifyKind(dateTimeOfEvent, DateTimeKind.Utc);
            dateTimeOfEvent = dateTimeOfEvent.ToLocalTime();

            if (!reminder.IsReminderDay[(int)DateTime.Now.DayOfWeek]
                || dateTimeOfEvent < DateTime.Now
                || Program.DelayedMessages.Any(m => m.Message.Contains(reminder.Message)))
            {
                return; // do not display reminder
            }

            // add up to 5 reminders before the event happens
            foreach (int? reminderSecond in reminder.ReminderSeconds)
            {
                if (reminderSecond == null || reminderSecond <= 0)
                {
                    continue;
                }

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

            // announce event
            Program.DelayedMessages.Add(new DelayedMessage
            {
                ReminderId = reminder.Id,
                Message = $"It's time for \"{reminder.Message}\"",
                SendDate = dateTimeOfEvent
            });
        }
    }
}
