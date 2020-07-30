using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;

namespace TwitchBotDb.Repositories
{
    public class PartyUpRepository
    {
        private readonly string _twitchBotApiLink;

        public PartyUpRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<PartyUp> GetPartyMemberAsync(string partyMember, int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<PartyUp>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}&partymember={partyMember}");
        }

        public async Task AddRequestedPartyMemberAsync(string username, int partyMemberId)
        {
            PartyUpRequest requestedPartyMember = new PartyUpRequest
            {
                Username = username,
                PartyMemberId = partyMemberId
            };

            await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"partyuprequests/create", requestedPartyMember);
        }

        public async Task<List<string>> GetPartyListAsync(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<List<string>>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}");
        }

        public async Task<List<PartyUpRequestResult>> GetRequestListAsync(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<List<PartyUpRequestResult>>(_twitchBotApiLink + $"partyuprequests/getlist/{broadcasterId}?gameId={gameId}");
        }

        public async Task<PartyUpRequestResult> PopRequestedPartyMemberAsync(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteAsync<PartyUpRequestResult>(_twitchBotApiLink + $"partyuprequests/deletefirst/{broadcasterId}?gameid={gameId}");
        }
    }
}
