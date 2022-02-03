namespace TwitchBotDb.Models
{
    public partial class InGameUsername
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int BroadcasterId { get; set; }
        public int? GameId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
        public virtual TwitchGameCategory Game { get; set; }
    }
}
