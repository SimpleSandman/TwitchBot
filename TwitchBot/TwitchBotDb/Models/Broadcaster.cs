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
            BotTimeout = new HashSet<BotTimeout>();
            ErrorLog = new HashSet<ErrorLog>();
            PartyUp = new HashSet<PartyUp>();
            Quote = new HashSet<Quote>();
            Rank = new HashSet<Rank>();
            RankFollower = new HashSet<RankFollower>();
            Reminder = new HashSet<Reminder>();
            SongRequest = new HashSet<SongRequest>();
            SongRequestIgnore = new HashSet<SongRequestIgnore>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public int TwitchId { get; set; }
        public DateTime LastUpdated { get; set; }

        public ICollection<Bank> Bank { get; set; }
        public ICollection<BankHeistSetting> BankHeistSetting { get; set; }
        public ICollection<BossFightSetting> BossFightSetting { get; set; }
        public ICollection<BotTimeout> BotTimeout { get; set; }
        public ICollection<ErrorLog> ErrorLog { get; set; }
        public ICollection<PartyUp> PartyUp { get; set; }
        public ICollection<Quote> Quote { get; set; }
        public ICollection<Rank> Rank { get; set; }
        public ICollection<RankFollower> RankFollower { get; set; }
        public ICollection<Reminder> Reminder { get; set; }
        public ICollection<SongRequest> SongRequest { get; set; }
        public ICollection<SongRequestIgnore> SongRequestIgnore { get; set; }
    }
}
