using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotCore.Repositories;

using TwitchBotDb.Models;

namespace TwitchBotCore.Services
{
    public class QuoteService
    {
        private QuoteRepository _quoteDb;

        public QuoteService(QuoteRepository quote)
        {
            _quoteDb = quote;
        }

        public async Task<List<Quote>> GetQuotes(int broadcasterId)
        {
            return await _quoteDb.GetQuotes(broadcasterId);
        }

        public async Task AddQuote(string quote, string username, int broadcasterId)
        {
            await _quoteDb.AddQuote(quote, username, broadcasterId);
        }
    }
}
