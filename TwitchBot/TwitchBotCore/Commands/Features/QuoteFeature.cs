using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Config;
using TwitchBotConsoleApp.Libraries;
using TwitchBotConsoleApp.Models;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotUtil.Enums;

namespace TwitchBotConsoleApp.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Quote" feature
    /// </summary>
    public sealed class QuoteFeature : BaseFeature
    {
        private readonly QuoteService _quote;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public QuoteFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, QuoteService quote) : base(irc, botConfig)
        {
            _quote = quote;
            _rolePermission.Add("!quote", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!addquote", new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!quote":
                        return (true, await Quote());
                    case "!addquote":
                        return (true, await AddQuote(chatter));
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "QuoteFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Display random broadcaster quote
        /// </summary>
        public async Task<DateTime> Quote()
        {
            try
            {
                List<Quote> quotes = await _quote.GetQuotes(_broadcasterInstance.DatabaseId);

                // Check if there any quotes inside the system
                if (quotes == null || quotes.Count == 0)
                    _irc.SendPublicChatMessage("There are no quotes to be displayed at the moment");
                else
                {
                    // Randomly pick a quote from the list to display
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int index = rnd.Next(quotes.Count);

                    Quote resultingQuote = new Quote();
                    resultingQuote = quotes.ElementAt(index); // grab random quote from list of quotes
                    string quoteResult = $"\"{resultingQuote.UserQuote}\" - {_botConfig.Broadcaster} "
                        + $"({resultingQuote.TimeCreated.ToString("MMMM", CultureInfo.InvariantCulture)} {resultingQuote.TimeCreated.Year}) "
                        + $"< Quoted by @{resultingQuote.Username} >";

                    _irc.SendPublicChatMessage(quoteResult);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "QuoteFeature", "Quote()", false, "!quote");
            }

            return DateTime.Now.AddSeconds(20);
        }

        /// <summary>
        /// Add a mod/broadcaster quote
        /// </summary>
        /// <param name="chatter"></param>
        public async Task<DateTime> AddQuote(TwitchChatter chatter)
        {
            try
            {
                string quote = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                await _quote.AddQuote(quote, chatter.DisplayName, _broadcasterInstance.DatabaseId);

                _irc.SendPublicChatMessage($"Quote has been created @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "QuoteFeature", "AddQuote(TwitchChatter)", false, "!addquote");
            }

            return DateTime.Now;
        }
    }
}
