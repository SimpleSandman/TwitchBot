using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class PartyUpService
    {
        private PartyUpRepository _partyUpDb;

        public PartyUpService(PartyUpRepository partyUpDb)
        {
            _partyUpDb = partyUpDb;
        }

        public bool HasPartyMemberBeenRequested(string username, int gameId, int broadcasterId)
        {
            return _partyUpDb.HasPartyMemberBeenRequested(username, gameId, broadcasterId);
        }

        public bool FindRequestedPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            return _partyUpDb.FindRequestedPartyMember(partyMember, gameId, broadcasterId);
        }

        public void AddPartyMember(string username, string partyMember, int gameId, int broadcasterId)
        {
            _partyUpDb.AddPartyMember(username, partyMember, gameId, broadcasterId);
        }

        public string GetPartyList(int gameId, int broadcasterId)
        {
            return _partyUpDb.GetPartyList(gameId, broadcasterId);
        }

        public string GetRequestList(int gameId, int broadcasterId)
        {
            return _partyUpDb.GetRequestList(gameId, broadcasterId);
        }
    }
}
