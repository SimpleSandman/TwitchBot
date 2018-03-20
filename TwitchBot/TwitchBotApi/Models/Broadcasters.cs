using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class Broadcasters
    {
        public Broadcasters()
        {
            Bank = new HashSet<Bank>();
            BankHeistSettings = new HashSet<BankHeistSettings>();
            BossFightSettings = new HashSet<BossFightSettings>();
            ErrorLog = new HashSet<ErrorLog>();
            Moderators = new HashSet<Moderators>();
            PartyUp = new HashSet<PartyUp>();
            PartyUpRequests = new HashSet<PartyUpRequests>();
            Quote = new HashSet<Quote>();
            Rank = new HashSet<Rank>();
            RankFollowers = new HashSet<RankFollowers>();
            Reminders = new HashSet<Reminders>();
            SongRequestBlacklist = new HashSet<SongRequestBlacklist>();
            SongRequests = new HashSet<SongRequests>();
            UserBotTimeout = new HashSet<UserBotTimeout>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime TimeAdded { get; set; }
        public int TwitchId { get; set; }

        public ICollection<Bank> Bank { get; set; }
        public ICollection<BankHeistSettings> BankHeistSettings { get; set; }
        public ICollection<BossFightSettings> BossFightSettings { get; set; }
        public ICollection<ErrorLog> ErrorLog { get; set; }
        public ICollection<Moderators> Moderators { get; set; }
        public ICollection<PartyUp> PartyUp { get; set; }
        public ICollection<PartyUpRequests> PartyUpRequests { get; set; }
        public ICollection<Quote> Quote { get; set; }
        public ICollection<Rank> Rank { get; set; }
        public ICollection<RankFollowers> RankFollowers { get; set; }
        public ICollection<Reminders> Reminders { get; set; }
        public ICollection<SongRequestBlacklist> SongRequestBlacklist { get; set; }
        public ICollection<SongRequests> SongRequests { get; set; }
        public ICollection<UserBotTimeout> UserBotTimeout { get; set; }
    }
}
