using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class GiveawayService
    {
        private GiveawayRepository _giveaway;

        public GiveawayService(GiveawayRepository giveaway)
        {
            _giveaway = giveaway;
        }

        public void AddGiveaway(DateTime giveawayDate, string giveawayText, int broadcasterId, int[] elgList,
            int giveawayType, string giveawayParam, string minRandNum, string maxRandNum)
        {
            _giveaway.AddGiveaway(giveawayDate, giveawayText, broadcasterId, elgList, 
                giveawayType, giveawayParam, minRandNum, maxRandNum);
        }

        public int GetGiveawayId(int reqGiveawayId, int broadcasterId)
        {
            return _giveaway.GetGiveawayId(reqGiveawayId, broadcasterId);
        }

        public void UpdateGiveaway(int inputType, DateTime giveawayDate, string giveawayInput, int[] elgList,
            int giveawayType, int giveawayId, int broadcasterId, string giveawayTypeParam1, string giveawayTypeParam2)
        {
            _giveaway.UpdateGiveaway(inputType, giveawayDate, giveawayInput, elgList, giveawayType, 
                giveawayId, broadcasterId, giveawayTypeParam1, giveawayTypeParam2);
        }
    }
}
