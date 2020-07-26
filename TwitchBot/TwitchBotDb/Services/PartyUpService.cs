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

        public async Task<bool> HasUserAlreadyRequested(string username, int gameId, int broadcasterId)
        {
            List<PartyUpRequestResult> partyRequestList = await _partyUpDb.GetRequestList(gameId, broadcasterId);

            return partyRequestList.Any(m => m.Username == username);
        }

        public async Task<PartyUp> GetPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            return await _partyUpDb.GetPartyMember(partyMember, gameId, broadcasterId);
        }

        public async Task AddPartyMember(string username, int partyMemberId)
        {
            await _partyUpDb.AddRequestedPartyMember(username, partyMemberId);
        }

        public async Task<string> GetPartyList(int gameId, int broadcasterId)
        {
            List<string> partyList = await _partyUpDb.GetPartyList(gameId, broadcasterId);

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

        public async Task<string> GetRequestList(int gameId, int broadcasterId)
        {
            List<PartyUpRequestResult> partyRequestList = await _partyUpDb.GetRequestList(gameId, broadcasterId);

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

        public async Task<string> PopRequestedPartyMember(int gameId, int broadcasterId)
        {
            PartyUpRequestResult firstPartyMember = await _partyUpDb.PopRequestedPartyMember(gameId, broadcasterId);

            if (firstPartyMember == null)
                return "There are no party members that can be removed from the request list";

            return $"The requested party member, \"{firstPartyMember.PartyMemberName}\" from @{firstPartyMember.Username}, has been removed";
        }
    }
}
