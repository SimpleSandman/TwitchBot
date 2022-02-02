using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;

namespace TwitchBotShared.Commands.Features
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
            _rolePermissions.Add("!quote", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!addquote", new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!quote":
                        return (true, await QuoteAsync());
                    case "!addquote":
                        return (true, await AddQuoteAsync(chatter));
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "QuoteFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Display random broadcaster quote
        /// </summary>
        private async Task<DateTime> QuoteAsync()
        {
            try
            {
                List<Quote> quotes = await _quote.GetQuotesAsync(_broadcasterInstance.DatabaseId);

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
                await _errHndlrInstance.LogErrorAsync(ex, "QuoteFeature", "Quote()", false, "!quote");
            }

            return DateTime.Now.AddSeconds(20);
        }

        /// <summary>
        /// Add a mod/broadcaster quote
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> AddQuoteAsync(TwitchChatter chatter)
        {
            try
            {
                string quote = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                await _quote.AddQuoteAsync(quote, chatter.DisplayName, _broadcasterInstance.DatabaseId);

                _irc.SendPublicChatMessage($"Quote has been created @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "QuoteFeature", "AddQuote(TwitchChatter)", false, "!addquote");
            }

            return DateTime.Now;
        }
    }
}
