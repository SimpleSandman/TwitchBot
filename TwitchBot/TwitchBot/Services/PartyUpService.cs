using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Extensions;
using TwitchBot.Repositories;

using TwitchBotDb.Models;

namespace TwitchBot.Services
{
    public class PartyUpService
    {
        private PartyUpRepository _partyUpDb;

        public PartyUpService(PartyUpRepository partyUpDb)
        {
            _partyUpDb = partyUpDb;
        }

        public async Task<bool> HasAlreadyRequested(string username, int partyMember)
        {
            return await _partyUpDb.HasAlreadyRequested(username, partyMember) != null ? true : false;
        }

        public async Task<PartyUp> GetPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            return await _partyUpDb.GetPartyMember(partyMember, gameId, broadcasterId);
        }

        public async Task AddPartyMember(string username, int partyMember)
        {
            await _partyUpDb.AddRequestedPartyMember(username, partyMember);
        }

        public async Task<string> GetPartyList(int gameId, int broadcasterId)
        {
            List<string> partyList = await _partyUpDb.GetPartyList(gameId, broadcasterId);

            if (partyList == null || partyList.Count == 0)
                return "No party members are set for this game";

            string message = "The available party members are: ";

            foreach (string member in partyList)
            {
                message += member + " >< ";
            }

            return message.ReplaceLastOccurrence(" >< ", "");
        }

        public async Task<string> GetRequestList(int gameId, int broadcasterId)
        {
            List<PartyUpRequest> partyRequestList = await _partyUpDb.GetRequestList(gameId, broadcasterId);

            if (partyRequestList == null || partyRequestList.Count == 0)
                return "The party request list is empty. Request a member with !partyup [name]";

            string message = "Here are the requested party members: ";

            foreach (PartyUpRequest member in partyRequestList)
            {
                message += member.PartyMember + " <-- " + member.Username + " || ";
            }

            return message.ReplaceLastOccurrence(" || ", "");
        }

        public async Task<string> PopRequestedPartyMember(int gameId, int broadcasterId)
        {
            PartyUpRequest firstPartyMember = await _partyUpDb.PopRequestedPartyMember(gameId, broadcasterId);

            if (firstPartyMember == null)
                return "There are no party members that can be removed from the request list";

            return $"The requested party member, \"{firstPartyMember.PartyMember}\" from @{firstPartyMember.Username}, has been removed";
        }
    }
}
