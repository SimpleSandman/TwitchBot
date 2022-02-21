using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;


namespace TwitchBotDb.Services
{
    public class QuoteService
    {
        private readonly QuoteRepository _quoteDb;

        public QuoteService(QuoteRepository quote)
        {
            _quoteDb = quote;
        }

        public async Task<List<Quote>> GetQuotesAsync(int broadcasterId)
        {
            return await _quoteDb.GetQuotesAsync(broadcasterId);
        }

        public async Task AddQuoteAsync(string quote, string username, int broadcasterId)
        {
            await _quoteDb.AddQuoteAsync(quote, username, broadcasterId);
        }
    }
}
