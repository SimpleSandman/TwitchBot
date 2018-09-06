using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.Models;

namespace TwitchBot.Repositories
{
    public class PartyUpRepository
    {
        private readonly string _twitchBotApiLink;

        public PartyUpRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<PartyUpRequest> HasAlreadyRequested(string username, int partyMemberId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<PartyUpRequest>(_twitchBotApiLink + $"partyuprequests/get/{partyMemberId}?username={username}");
        }

        public async Task<PartyUp> GetPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<PartyUp>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}&partymember={partyMember}");
        }

        public async Task AddRequestedPartyMember(string username, int partyMember)
        {
            PartyUpRequest requestedPartyMember = new PartyUpRequest
            {
                Username = username,
                PartyMember = partyMember
            };

            await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"partyuprequests/create", requestedPartyMember);
        }

        public async Task<List<string>> GetPartyList(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<string>>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}");
        }

        public async Task<List<PartyUpRequest>> GetRequestList(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<PartyUpRequest>>(_twitchBotApiLink + $"partyuprequests/get/{broadcasterId}?gameId={gameId}");
        }

        public async Task<PartyUpRequest> PopRequestedPartyMember(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<PartyUpRequest>(_twitchBotApiLink + $"partyuprequests/deletetopone/{broadcasterId}?gameid={gameId}");
        }
    }
}
