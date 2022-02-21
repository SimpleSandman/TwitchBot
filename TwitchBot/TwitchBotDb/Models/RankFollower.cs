namespace TwitchBotDb.Models
{
    public class RankFollower
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int? TwitchId { get; set; }
        public int Experience { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
