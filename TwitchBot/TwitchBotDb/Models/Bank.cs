using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class Bank
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Wallet { get; set; }
        public int Broadcaster { get; set; }
        public DateTime TimeAdded { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
