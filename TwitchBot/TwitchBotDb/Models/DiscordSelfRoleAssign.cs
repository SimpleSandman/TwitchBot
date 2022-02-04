namespace TwitchBotDb.Models
{
    public partial class DiscordSelfRoleAssign
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string ServerName { get; set; }
        public int FollowAgeMinimumHour { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
