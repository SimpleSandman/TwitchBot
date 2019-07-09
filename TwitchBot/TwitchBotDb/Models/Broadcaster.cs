using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class Broadcaster
    {
        public Broadcaster()
        {
            Bank = new HashSet<Bank>();
            BankHeistSetting = new HashSet<BankHeistSetting>();
            BossFightSetting = new HashSet<BossFightSetting>();
            BotModerator = new HashSet<BotModerator>();
            BotTimeout = new HashSet<BotTimeout>();
            CustomCommand = new HashSet<CustomCommand>();
            ErrorLog = new HashSet<ErrorLog>();
            InGameUsername = new HashSet<InGameUsername>();
            PartyUp = new HashSet<PartyUp>();
            Quote = new HashSet<Quote>();
            Rank = new HashSet<Rank>();
            RankFollower = new HashSet<RankFollower>();
            Reminder = new HashSet<Reminder>();
            SongRequest = new HashSet<SongRequest>();
            SongRequestIgnore = new HashSet<SongRequestIgnore>();
            SongRequestSetting = new HashSet<SongRequestSetting>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public int TwitchId { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual ICollection<Bank> Bank { get; set; }
        public virtual ICollection<BankHeistSetting> BankHeistSetting { get; set; }
        public virtual ICollection<BossFightSetting> BossFightSetting { get; set; }
        public virtual ICollection<BotModerator> BotModerator { get; set; }
        public virtual ICollection<BotTimeout> BotTimeout { get; set; }
        public virtual ICollection<CustomCommand> CustomCommand { get; set; }
        public virtual ICollection<ErrorLog> ErrorLog { get; set; }
        public virtual ICollection<InGameUsername> InGameUsername { get; set; }
        public virtual ICollection<PartyUp> PartyUp { get; set; }
        public virtual ICollection<Quote> Quote { get; set; }
        public virtual ICollection<Rank> Rank { get; set; }
        public virtual ICollection<RankFollower> RankFollower { get; set; }
        public virtual ICollection<Reminder> Reminder { get; set; }
        public virtual ICollection<SongRequest> SongRequest { get; set; }
        public virtual ICollection<SongRequestIgnore> SongRequestIgnore { get; set; }
        public virtual ICollection<SongRequestSetting> SongRequestSetting { get; set; }
    }
}
