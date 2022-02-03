namespace TwitchBotDb.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int? TwitchId { get; set; }
        public int Wallet { get; set; }
        public int Broadcaster { get; set; }

        public virtual Broadcaster BroadcasterNavigation { get; set; }
    }
}
