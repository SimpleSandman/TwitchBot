using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.Models;

namespace TwitchBot.Repositories
{
    public class PartyUpRepository
    {
        private readonly string _connStr;
        private readonly string _twitchBotApiLink;

        public PartyUpRepository(string connStr, string twitchBotApiLink)
        {
            _connStr = connStr;
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<PartyUpRequests> GetExistingRequester(string username, int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<PartyUpRequests>(_twitchBotApiLink + $"partyuprequests/get/{broadcasterId}?gameId={gameId}&username={username}");
        }

        public async Task<PartyUp> GetPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<PartyUp>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}&partymember={partyMember}");
        }

        public async Task AddRequestedPartyMember(string username, string partyMember, int gameId, int broadcasterId)
        {
            PartyUpRequests requestedPartyMember = new PartyUpRequests
            {
                Username = username,
                PartyMember = partyMember,
                Broadcaster = broadcasterId,
                Game = gameId
            };

            await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"partyuprequests/create/{broadcasterId}", requestedPartyMember);
        }

        public async Task<List<string>> GetPartyList(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<string>>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}");
        }

        public async Task<List<PartyUpRequests>> GetRequestList(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<PartyUpRequests>>(_twitchBotApiLink + $"partyuprequests/get/{broadcasterId}?gameId={gameId}");
        }

        public async Task<PartyUpRequests> PopRequestedPartyMember(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<PartyUpRequests>(_twitchBotApiLink + $"partyuprequests/deletetopone/{broadcasterId}?gameid={gameId}");
        }
    }
}
