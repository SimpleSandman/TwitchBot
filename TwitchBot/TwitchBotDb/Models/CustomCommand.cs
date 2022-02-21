using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public class CustomCommand
    {
        public CustomCommand()
        {
            Whitelists = new HashSet<Whitelist>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public bool IsSound { get; set; }
        public bool IsGlobalCooldown { get; set; }
        public int CooldownSec { get; set; }
        public int CurrencyCost { get; set; }
        public int? GameId { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
        public virtual TwitchGameCategory Game { get; set; }
        public virtual ICollection<Whitelist> Whitelists { get; set; }
    }
}
