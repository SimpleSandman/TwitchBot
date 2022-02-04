using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class Broadcaster
    {
        public Broadcaster()
        {
            BankHeistSettings = new HashSet<BankHeistSetting>();
            Banks = new HashSet<Bank>();
            BossFightSettings = new HashSet<BossFightSetting>();
            CustomCommands = new HashSet<CustomCommand>();
            ErrorLogs = new HashSet<ErrorLog>();
            InGameUsernames = new HashSet<InGameUsername>();
            PartyUps = new HashSet<PartyUp>();
            Quotes = new HashSet<Quote>();
            RankFollowers = new HashSet<RankFollower>();
            Ranks = new HashSet<Rank>();
            Reminders = new HashSet<Reminder>();
            SongRequestIgnores = new HashSet<SongRequestIgnore>();
            SongRequestSettings = new HashSet<SongRequestSetting>();
            SongRequests = new HashSet<SongRequest>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public int TwitchId { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual ICollection<BankHeistSetting> BankHeistSettings { get; set; }
        public virtual ICollection<Bank> Banks { get; set; }
        public virtual ICollection<BossFightSetting> BossFightSettings { get; set; }
        public virtual ICollection<CustomCommand> CustomCommands { get; set; }
        public virtual ICollection<ErrorLog> ErrorLogs { get; set; }
        public virtual ICollection<InGameUsername> InGameUsernames { get; set; }
        public virtual ICollection<PartyUp> PartyUps { get; set; }
        public virtual ICollection<Quote> Quotes { get; set; }
        public virtual ICollection<RankFollower> RankFollowers { get; set; }
        public virtual ICollection<Rank> Ranks { get; set; }
        public virtual ICollection<Reminder> Reminders { get; set; }
        public virtual ICollection<SongRequestIgnore> SongRequestIgnores { get; set; }
        public virtual ICollection<SongRequestSetting> SongRequestSettings { get; set; }
        public virtual ICollection<SongRequest> SongRequests { get; set; }
    }
}
