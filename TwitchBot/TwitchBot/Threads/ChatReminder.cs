using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

using TwitchBot.Extensions;
using TwitchBot.Models;

namespace TwitchBot.Threads
{
    public class ChatReminder
    {
        private Thread _chatReminderThread;
        private int _broadcasterId;
        private string _connStr;
        private List<Reminder> _reminders;

        public ChatReminder(int broadcasterId, string connStr)
        {
            _broadcasterId = broadcasterId;
            _connStr = connStr;
            _chatReminderThread = new Thread (new ThreadStart (this.Run)); 
        }

        public void Start()
        {
            _chatReminderThread.IsBackground = true;
            _chatReminderThread.Start(); 
        }

        public void Run()
        {
            LoadReminders();

            while (true)
            {
                foreach (Reminder reminder in _reminders)
                {
                    DateTime dateTimeOfEvent = DateTime.Today.Date.Add(reminder.TimeToPost);
                    dateTimeOfEvent = DateTime.SpecifyKind(dateTimeOfEvent, DateTimeKind.Utc);
                    dateTimeOfEvent = dateTimeOfEvent.ToLocalTime();

                    if (!reminder.IsReminderDay[(int)DateTime.Now.DayOfWeek] 
                        || dateTimeOfEvent < DateTime.Now
                        || Program.DelayedMessages.Any(m => m.Message.Contains(reminder.Message)))
                    {
                        continue; // do not display reminder
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
                                Message = $"{timeSpan.ToReadableString()} until \"{reminder.Message}\"",
                                SendDate = reminderTime
                            });
                        }
                    }

                    // announce event
                    Program.DelayedMessages.Add(new DelayedMessage
                    {
                        Message = $"It's time for \"{reminder.Message}\"",
                        SendDate = dateTimeOfEvent
                    });
                }

                Thread.Sleep(60000); // 60 seconds
            }
        }

        private void LoadReminders()
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
                                    TimeToPost = TimeSpan.Parse(reader["timeOfEvent"].ToString()),
                                    Message = reader["message"].ToString()
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
