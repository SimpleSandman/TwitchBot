using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class CmdMod
    {
        /// <summary>
        /// Displays Discord link (if available)
        /// </summary>
        public void CmdDiscord()
        {
            try
            {
                if (String.IsNullOrEmpty(Program._strDiscordLink) || Program._strDiscordLink.Equals("Link unavailable at the moment"))
                    Program._irc.sendPublicChatMessage("Discord link unavailable at the moment");
                else
                    Program._irc.sendPublicChatMessage("Join me on my discord server! " + Program._strDiscordLink);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdMod", "CmdDiscord()", false, "!discord");
            }
        }
    }
}
