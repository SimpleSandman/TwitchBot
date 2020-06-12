using System;

namespace TwitchBot.Commands
{
    public static class CommandToolbox
    {
        /// <summary>
        /// Used for messages that require a boolean operation
        /// </summary>
        /// <param name="message">Valid operations: {on, off, true, false}</param>
        /// <returns></returns>
        public static bool SetBooleanFromMessage(string message)
        {
            if (message == "on" || message == "true" || message == "yes")
            {
                return true;
            }
            else if (message == "off" || message == "false" || message == "no")
            {
                return false;
            }
            else
            {
                throw new Exception("Couldn't find specified message");
            }
        }
    }
}
