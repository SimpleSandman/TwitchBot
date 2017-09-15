using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class BankHeist
    {
        private IrcClient _irc;
        private string _connStr;
        private int _broadcasterId;
        private Thread _thread;
        private BankService _bank;
        private BankHeistSettings _heistSettings = BankHeistSettings.Instance;

        private BlockingCollection<BankRobber> _robbers = new BlockingCollection<BankRobber>();

        public BankHeist() { }

        public BankHeist(string connStr, BankService bank)
        {
            _connStr = connStr;
            _thread = new Thread(new ThreadStart(this.Run));
            _bank = bank;
        }

        public void Start(IrcClient irc, int broadcasterId)
        {
            _irc = irc;
            _broadcasterId = broadcasterId;
            _heistSettings.CooldownTimePeriod = DateTime.Now.AddMinutes(_heistSettings.CooldownTimePeriodMinutes);

            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                if (_heistSettings.IsHeistOnCooldown())
                {
                    double cooldownTime = (_heistSettings.CooldownTimePeriod.Subtract(DateTime.Now)).TotalMilliseconds;
                    Thread.Sleep((int)cooldownTime);
                }
                else if (_robbers.Count > 0 && _heistSettings.IsEntryPeriodOver())
                {
                    _robbers.CompleteAdding();
                    Consume();
                    
                    // refresh the list and reset the cooldown time period
                    _robbers = new BlockingCollection<BankRobber>();
                    _heistSettings.CooldownTimePeriod = DateTime.Now.AddMinutes(_heistSettings.CooldownTimePeriodMinutes);
                }
            }
        }

        public void Produce(BankRobber robber)
        {
            _robbers.Add(robber);
        }

        public void Consume()
        {
            _irc.SendPublicChatMessage(_heistSettings.GameStart);

            Thread.Sleep(2000); // wait in anticipation

            Random rnd = new Random();
            BankHeistPayout payout = _heistSettings.Payouts[HeistLevel() - 1];

            int chance = rnd.Next(1, 101); // 1 - 100
            if (chance >= payout.SuccessRate) // failed
            {
                if (_robbers.Count == 1)
                    _irc.SendPublicChatMessage(_heistSettings.SingleUserFail);
                else
                    _irc.SendPublicChatMessage(_heistSettings.Success0);

                return;
            }
            
            int numWinners = (int)Math.Ceiling(_robbers.Count * payout.SuccessRate);
            var winners = _robbers.OrderBy(x => rnd.Next()).Take(numWinners);

            foreach (BankRobber winner in winners)
            {
                int funds = _bank.CheckBalance(winner.Username.ToLower(), _broadcasterId);
                decimal earnings = Math.Ceiling(funds * payout.WinMultiplier);

                _bank.UpdateFunds(winner.Username.ToLower(), _broadcasterId, (int)earnings);
            }

            // display success outcome
            if (winners.Count() == 1)
            {
                _irc.SendPublicChatMessage(_heistSettings.SingleUserSuccess);
            }
            else if (payout.SuccessRate == 100.0m)
            {
                _irc.SendPublicChatMessage(_heistSettings.Success100);
            }
            else if (payout.SuccessRate > 34.0m && payout.SuccessRate < 100.0m)
            {
                _irc.SendPublicChatMessage(_heistSettings.Success34);
            }
            else if (payout.SuccessRate > 0 && payout.SuccessRate < 34.0m)
            {
                _irc.SendPublicChatMessage(_heistSettings.Success1);
            }
        }

        public int NumRobbers()
        {
            return _robbers.Count;
        }

        public bool HasRobberEntered(string username)
        {
            return _robbers.Any(u => u.Username == username) ? true : false;
        }

        public bool IsEntryPeriodOver()
        {
            return _robbers.IsAddingCompleted ? true : false;
        }

        public int HeistLevel()
        {
            if (_robbers.Count <= _heistSettings.Levels[0].MaxUsers)
                return 1;
            else if (_robbers.Count <= _heistSettings.Levels[1].MaxUsers)
                return 2;
            else if (_robbers.Count <= _heistSettings.Levels[2].MaxUsers)
                return 3;
            else if (_robbers.Count <= _heistSettings.Levels[3].MaxUsers)
                return 4;
            else
                return 5;
        }

        public string NextLevelMessage()
        {
            if (_robbers.Count == _heistSettings.Levels[0].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[0];
            else if (_robbers.Count == _heistSettings.Levels[1].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[1];
            else if (_robbers.Count == _heistSettings.Levels[2].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[2];
            else if (_robbers.Count == _heistSettings.Levels[3].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[3];

            return "";
        }
    }
}
