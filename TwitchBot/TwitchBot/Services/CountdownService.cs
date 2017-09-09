using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class CountdownService
    {
        private CountdownRepository _countdown;

        public CountdownService(CountdownRepository countdown)
        {
            _countdown = countdown;
        }

        public void AddCountdown(string countdownMessage, DateTime countdownDuration, int broadcasterId)
        {
            _countdown.AddCountdown(countdownMessage, countdownDuration, broadcasterId);
        }

        public int GetCountdownId(int reqCountdownId, int broadcasterId)
        {
            return _countdown.GetCountdownId(reqCountdownId, broadcasterId);
        }

        public string ListCountdowns(int broadcasterId)
        {
            return _countdown.ListCountdowns(broadcasterId);
        }

        public void UpdateCountdown(int inputType, DateTime countdownDuration, string countdownInput, int responseCountdownId, int broadcasterId)
        {
            _countdown.UpdateCountdown(inputType, countdownDuration, countdownInput, responseCountdownId, broadcasterId);
        }
    }
}
