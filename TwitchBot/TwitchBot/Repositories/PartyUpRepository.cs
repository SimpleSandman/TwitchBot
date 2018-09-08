using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.DTO;
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

        public async Task<PartyUpRequest> HasUserAlreadyRequested(string username, int partyMemberId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<PartyUpRequest>(_twitchBotApiLink + $"partyuprequests/getuserrequest/{partyMemberId}?username={username}");
        }

        public async Task<PartyUp> GetPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<PartyUp>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}&partymember={partyMember}");
        }

        public async Task AddRequestedPartyMember(string username, int partyMemberId)
        {
            PartyUpRequest requestedPartyMember = new PartyUpRequest
            {
                Username = username,
                PartyMemberId = partyMemberId
            };

            await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"partyuprequests/create", requestedPartyMember);
        }

        public async Task<List<string>> GetPartyList(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<string>>(_twitchBotApiLink + $"partyups/get/{broadcasterId}?gameId={gameId}");
        }

        public async Task<List<PartyUpRequestResult>> GetRequestList(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<PartyUpRequestResult>>(_twitchBotApiLink + $"partyuprequests/getlist/{broadcasterId}?gameId={gameId}");
        }

        public async Task<PartyUpRequest> PopRequestedPartyMember(int gameId, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<PartyUpRequest>(_twitchBotApiLink + $"partyuprequests/deletefirst/{broadcasterId}?gameid={gameId}");
        }
    }
}
