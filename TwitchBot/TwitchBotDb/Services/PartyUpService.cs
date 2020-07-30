using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;
using TwitchBotDb.Repositories;

namespace TwitchBotDb.Services
{
    public class PartyUpService
    {
        private PartyUpRepository _partyUpDb;

        public PartyUpService(PartyUpRepository partyUpDb)
        {
            _partyUpDb = partyUpDb;
        }

        public async Task<bool> HasUserAlreadyRequestedAsync(string username, int gameId, int broadcasterId)
        {
            List<PartyUpRequestResult> partyRequestList = await _partyUpDb.GetRequestListAsync(gameId, broadcasterId);

            return partyRequestList.Any(m => m.Username == username);
        }

        public async Task<PartyUp> GetPartyMemberAsync(string partyMember, int gameId, int broadcasterId)
        {
            return await _partyUpDb.GetPartyMemberAsync(partyMember, gameId, broadcasterId);
        }

        public async Task AddRequestedPartyMemberAsync(string username, int partyMemberId)
        {
            await _partyUpDb.AddRequestedPartyMemberAsync(username, partyMemberId);
        }

        public async Task<string> GetPartyListAsync(int gameId, int broadcasterId)
        {
            List<string> partyList = await _partyUpDb.GetPartyListAsync(gameId, broadcasterId);

            if (partyList == null || partyList.Count == 0)
            {
                return "No party members are set for this game";
            }

            string message = "The available party members are: ";

            foreach (string member in partyList)
            {
                message += member + " >< ";
            }

            message = message.Substring(0, message.Length - 4);

            return message;
        }

        public async Task<string> GetRequestListAsync(int gameId, int broadcasterId)
        {
            List<PartyUpRequestResult> partyRequestList = await _partyUpDb.GetRequestListAsync(gameId, broadcasterId);

            if (partyRequestList == null || partyRequestList.Count == 0)
            {
                return "The party request list is empty. Request a member with !partyup [name]";
            }

            string message = "Here are the requested party members: ";

            foreach (PartyUpRequestResult member in partyRequestList)
            {
                message += member.PartyMemberName + " <-- " + member.Username + " || ";
            }

            message = message.Substring(0, message.Length - 4);

            return message;
        }

        public async Task<string> PopRequestedPartyMemberAsync(int gameId, int broadcasterId)
        {
            PartyUpRequestResult firstPartyMember = await _partyUpDb.PopRequestedPartyMemberAsync(gameId, broadcasterId);

            if (firstPartyMember == null)
            {
                return "There are no party members that can be removed from the request list";
            }

            return $"The requested party member, \"{firstPartyMember.PartyMemberName}\" from @{firstPartyMember.Username}, has been removed";
        }
    }
}
