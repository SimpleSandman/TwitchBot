using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Models;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class QuoteService
    {
        private QuoteRepository _quoteDb;

        public QuoteService(QuoteRepository quote)
        {
            _quoteDb = quote;
        }

        public List<Quote> GetQuotes(int broadcasterId)
        {
            return _quoteDb.GetQuotes(broadcasterId);
        }

        public void AddQuote(string quote, string username, int broadcasterId)
        {
            _quoteDb.AddQuote(quote, username, broadcasterId);
        }
    }
}
