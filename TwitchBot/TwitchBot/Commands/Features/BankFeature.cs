using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Bank" feature
    /// </summary>
    public sealed class BankFeature : BaseFeature
    {
        private readonly BankService _bank;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public BankFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, BankService bank) : base(irc, botConfig)
        {
            _bank = bank;
            _rolePermission.Add("!deposit", new CommandPermission { General = ChatterType.Moderator});
            _rolePermission.Add("!charge", new CommandPermission { General = ChatterType.Moderator });
            _rolePermission.Add("!points", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add($"!{_botConfig.CurrencyType.ToLower()}", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add($"!{_botConfig.CurrencyType.ToLower()}top3", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!bonusall", new CommandPermission { General = ChatterType.Moderator });
            _rolePermission.Add("!give", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!gamble", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!deposit":
                        return (true, await Deposit(chatter));
                    case "!charge":
                        return (true, await Charge(chatter));
                    case "!bonusall":
                        return (true, await BonusAll(chatter));
                    case "!points":
                        return (true, await CheckFunds(chatter));
                    case "!give":
                        return (true, await GiveFunds(chatter));
                    case "!gamble":
                        return (true, await Gamble(chatter));
                    default:
                        if (requestedCommand == $"!{_botConfig.CurrencyType.ToLower()}")
                        {
                            return (true, await CheckFunds(chatter));
                        }

                        else if (requestedCommand == $"!{_botConfig.CurrencyType.ToLower()}top3")
                        {
                            return (true, await LeaderboardCurrency(chatter));
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Gives a set amount of stream currency to user
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> Deposit(TwitchChatter chatter)
        {
            try
            {
                List<string> userList = new List<string>();

                foreach (int index in chatter.Message.AllIndexesOf("@"))
                {
                    int lengthUsername = chatter.Message.IndexOf(" ", index) - index - 1;
                    if (lengthUsername < 0)
                        userList.Add(chatter.Message.Substring(index + 1).ToLower());
                    else
                        userList.Add(chatter.Message.Substring(index + 1, lengthUsername).ToLower());
                }

                // Check for valid command
                if (chatter.Message.StartsWith("!deposit @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount to a user @{chatter.DisplayName}");
                // Check if moderator is trying to give streamer currency to themselves
                else if (chatter.Username != _botConfig.Broadcaster.ToLower() && userList.Contains(chatter.Username))
                    _irc.SendPublicChatMessage($"Entire deposit voided. You cannot add {_botConfig.CurrencyType} to your own account @{chatter.DisplayName}");
                else
                {
                    // Check if moderator is trying to give streamer currency to other moderators
                    if (chatter.Username != _botConfig.Broadcaster.ToLower())
                    {
                        foreach (string user in userList)
                        {
                            if (_twitchChatterListInstance.GetUserChatterType(user) == ChatterType.Moderator)
                            {
                                _irc.SendPublicChatMessage($"Entire deposit voided. You cannot add {_botConfig.CurrencyType} to another moderator's account @{chatter.DisplayName}");
                                return DateTime.Now;
                            }
                        }
                    }

                    int indexAction = chatter.Message.IndexOf(" ");
                    int deposit = -1;
                    bool isValidDeposit = int.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out deposit);

                    // Check if deposit amount is valid
                    if (deposit < 0)
                        _irc.SendPublicChatMessage("Please insert a positive whole amount (no decimals) "
                            + $" or use the !charge command to remove {_botConfig.CurrencyType} from a user");
                    else if (!isValidDeposit)
                        _irc.SendPublicChatMessage($"The deposit wasn't accepted. Please try again with a positive whole amount (no decimals) @{chatter.DisplayName}");
                    else
                    {
                        if (userList.Count > 0)
                        {
                            List<BalanceResult> balResultList = await _bank.UpdateCreateBalance(userList, _broadcasterInstance.DatabaseId, deposit, true);

                            string responseMsg = $"Gave {deposit} {_botConfig.CurrencyType} to ";

                            if (balResultList.Count > 1)
                            {
                                foreach (BalanceResult userResult in balResultList)
                                    responseMsg += $"{userResult.Username}, ";

                                responseMsg = responseMsg.ReplaceLastOccurrence(", ", "");
                            }
                            else if (balResultList.Count == 1)
                            {
                                responseMsg += $"@{balResultList[0].Username} ";

                                if (balResultList[0].ActionType == "UPDATE")
                                    responseMsg += $"and now has {balResultList[0].Wallet} {_botConfig.CurrencyType}!";
                                else if (balResultList[0].ActionType == "INSERT")
                                    responseMsg += $"and can now gamble it all away! Kappa";
                            }
                            else
                                responseMsg = $"Unknown error has occurred in retrieving results. Please check your recipient's {_botConfig.CurrencyType}";

                            _irc.SendPublicChatMessage(responseMsg);
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{chatter.DisplayName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "Deposit(TwitchChatter)", false, "!deposit");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Takes money away from a user
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> Charge(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Message.StartsWith("!charge @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount @{chatter.DisplayName}");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    int fee = -1;
                    bool isValidFee = int.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out fee);
                    string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();
                    int wallet = await _bank.CheckBalance(recipient, _broadcasterInstance.DatabaseId);

                    // Check user's bank account exist or has currency
                    if (wallet == -1)
                        _irc.SendPublicChatMessage($"{recipient} is not currently banking with us @{chatter.DisplayName}");
                    else if (wallet == 0)
                        _irc.SendPublicChatMessage($"{recipient} is out of {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    // Check if fee can be accepted
                    else if (fee > 0)
                        _irc.SendPublicChatMessage("Please insert a negative whole amount (no decimal numbers) "
                            + $" or use the !deposit command to add {_botConfig.CurrencyType} to a user's account");
                    else if (!isValidFee)
                        _irc.SendPublicChatMessage($"The fee wasn't accepted. Please try again with negative whole amount (no decimals) @{chatter.DisplayName}");
                    else if (chatter.Username != _botConfig.Broadcaster.ToLower() && _twitchChatterListInstance.GetUserChatterType(recipient) == ChatterType.Moderator)
                        _irc.SendPublicChatMessage($"Entire deposit voided. You cannot remove {_botConfig.CurrencyType} from another moderator's account @{chatter.DisplayName}");
                    else /* Deduct funds from wallet */
                    {
                        wallet += fee;

                        // Zero out account balance if user is being charged more than they have
                        if (wallet < 0)
                            wallet = 0;

                        await _bank.UpdateFunds(recipient, _broadcasterInstance.DatabaseId, wallet);

                        // Prompt user's balance
                        if (wallet == 0)
                            _irc.SendPublicChatMessage($"Charged {fee.ToString().Replace("-", "")} {_botConfig.CurrencyType} to {recipient}"
                                + $"'s account! They are out of {_botConfig.CurrencyType} to spend");
                        else
                            _irc.SendPublicChatMessage($"Charged {fee.ToString().Replace("-", "")} {_botConfig.CurrencyType} to {recipient}"
                                + $"'s account! They only have {wallet} {_botConfig.CurrencyType} to spend");
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "Charge(TwitchChatter)", false, "!charge");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Check user's account balance
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> CheckFunds(TwitchChatter chatter)
        {
            try
            {
                int balance = await _bank.CheckBalance(chatter.Username, _broadcasterInstance.DatabaseId);

                if (balance == -1)
                    _irc.SendPublicChatMessage($"You are not currently banking with us at the moment. Please talk to a moderator about acquiring {_botConfig.CurrencyType}");
                else
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} currently has {balance} {_botConfig.CurrencyType}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "CheckFunds(TwitchChatter)", false, "![currency name]");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Gives every viewer currently watching a set amount of currency
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> BonusAll(TwitchChatter chatter)
        {
            try
            {
                // Check for valid command
                if (chatter.Message.StartsWith("!bonusall @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount to a user @{chatter.DisplayName}");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    int deposit = -1;
                    bool isValidDeposit = int.TryParse(chatter.Message.Substring(indexAction), out deposit);

                    // Check if deposit amount is valid
                    if (deposit < 0)
                        _irc.SendPublicChatMessage("Please insert a positive whole amount (no decimals) "
                            + $" or use the !charge command to remove {_botConfig.CurrencyType} from a user");
                    else if (!isValidDeposit)
                        _irc.SendPublicChatMessage($"The bulk deposit wasn't accepted. Please try again with positive whole amount (no decimals) @{chatter.DisplayName}");
                    else
                    {
                        // Wait until chatter lists are available
                        // ToDo: Create timeout in case if list never becomes available
                        while (!_twitchChatterListInstance.AreListsAvailable)
                        {

                        }

                        List<string> chatterList = _twitchChatterListInstance.ChattersByName;

                        // broadcaster gives stream currency to all but themselves and the bot
                        if (chatter.Username == _botConfig.Broadcaster.ToLower())
                        {
                            chatterList = chatterList.Where(t => t != chatter.Username.ToLower() && t != _botConfig.BotName.ToLower()).ToList();
                        }
                        else // moderators gives stream currency to all but other moderators (including broadcaster)
                        {
                            chatterList = chatterList
                                .Where(t => _twitchChatterListInstance.GetUserChatterType(t) != ChatterType.Moderator
                                    && t != _botConfig.BotName.ToLower()).ToList();
                        }

                        if (chatterList != null && chatterList.Count > 0)
                        {
                            await _bank.UpdateCreateBalance(chatterList, _broadcasterInstance.DatabaseId, deposit);

                            _irc.SendPublicChatMessage($"{deposit} {_botConfig.CurrencyType} for everyone! "
                                + $"Check your stream bank account with !{_botConfig.CurrencyType.ToLower()}");
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{chatter.DisplayName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "BonusAll(TwitchChatter)", false, "!bonusall");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Let a user give an amount of their funds to another chatter
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> GiveFunds(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Message.StartsWith("!give @"))
                {
                    _irc.SendPublicChatMessage($"Please enter a valid amount @{chatter.DisplayName} (ex: !give [amount/all] @[username])");
                    return DateTime.Now;
                }

                int giftAmount = 0;
                bool validGiftAmount = false;
                string giftMessage = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1, chatter.Message.GetNthCharIndex(' ', 2) - chatter.Message.IndexOf(" ") - 1);

                // Check if user wants to give all of their wallet to another user
                // Else check if their message is a valid amount to give
                validGiftAmount = giftMessage == "all" ? true : int.TryParse(giftMessage, out giftAmount);

                if (!validGiftAmount)
                {
                    _irc.SendPublicChatMessage($"Please insert a positive whole amount (no decimal numbers) to gamble @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                // Get and check recipient
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();

                if (string.IsNullOrEmpty(recipient) || chatter.Message.IndexOf("@") == -1)
                {
                    _irc.SendPublicChatMessage($"I don't know who I'm supposed to send this to. Please specify a recipient @{chatter.DisplayName}");
                    return DateTime.Now;
                }
                else if (recipient == chatter.Username)
                {
                    _irc.SendPublicChatMessage($"Stop trying to give {_botConfig.CurrencyType} to yourself @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                // Get and check wallet balance
                int balance = await _bank.CheckBalance(chatter.Username, _broadcasterInstance.DatabaseId);

                if (giftMessage == "all")
                {
                    giftAmount = balance;
                }

                if (balance == -1)
                    _irc.SendPublicChatMessage($"You are not currently banking with us @{chatter.DisplayName} . Please talk to a moderator about acquiring {_botConfig.CurrencyType}");
                else if (giftAmount < 1)
                    _irc.SendPublicChatMessage($"That is not a valid amount of {_botConfig.CurrencyType} to give. Please try again with a positive whole amount (no decimals) @{chatter.DisplayName}");
                else if (balance < giftAmount)
                    _irc.SendPublicChatMessage($"You do not have enough to give {giftAmount} {_botConfig.CurrencyType} @{chatter.DisplayName}");
                else
                {
                    // make sure the user exists in the database to prevent fake accounts from being created
                    int recipientBalance = await _bank.CheckBalance(recipient, _broadcasterInstance.DatabaseId);

                    if (recipientBalance == -1)
                        _irc.SendPublicChatMessage($"The user \"{recipient}\" is currently not banking with us. Please talk to a moderator about creating their account @{chatter.DisplayName}");
                    else
                    {
                        await _bank.UpdateFunds(chatter.Username, _broadcasterInstance.DatabaseId, balance - giftAmount); // take away from sender
                        await _bank.UpdateFunds(recipient, _broadcasterInstance.DatabaseId, giftAmount + recipientBalance); // give to recipient

                        _irc.SendPublicChatMessage($"@{chatter.DisplayName} gave {giftAmount} {_botConfig.CurrencyType} to @{recipient}");
                        return DateTime.Now.AddSeconds(20);
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "GiveFunds(TwitchChatter)", false, "!give", chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Disply the top 3 richest users (if available)
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> LeaderboardCurrency(TwitchChatter chatter)
        {
            try
            {
                List<Bank> richestUsers = await _bank.GetCurrencyLeaderboard(_botConfig.Broadcaster, _broadcasterInstance.DatabaseId, _botConfig.BotName);

                if (richestUsers.Count == 0)
                {
                    _irc.SendPublicChatMessage($"Everyone's broke! @{chatter.DisplayName} NotLikeThis");
                    return DateTime.Now;
                }

                string resultMsg = "";
                foreach (Bank user in richestUsers)
                {
                    resultMsg += $"\"{user.Username}\" with {user.Wallet} {_botConfig.CurrencyType}, ";
                }

                resultMsg = resultMsg.Remove(resultMsg.Length - 2); // remove extra ","

                // improve list grammar
                if (richestUsers.Count == 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", " and ");
                else if (richestUsers.Count > 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", ", and ");

                if (richestUsers.Count == 1)
                    _irc.SendPublicChatMessage($"The richest user is {resultMsg}");
                else
                    _irc.SendPublicChatMessage($"The richest users are: {resultMsg}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "LeaderboardCurrency(TwitchChatter)", false, "![currency name]top3");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Gamble away currency
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Gamble(TwitchChatter chatter)
        {
            try
            {
                int gambledMoney = 0; // Money put into the gambling system
                bool isValidMsg = false;
                string gambleMessage = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                // Check if user wants to gamble all of their wallet
                // Else check if their message is a valid amount to gamble
                isValidMsg = gambleMessage.Equals("all", StringComparison.CurrentCultureIgnoreCase) ? true : int.TryParse(gambleMessage, out gambledMoney);

                if (!isValidMsg)
                {
                    _irc.SendPublicChatMessage($"Please insert a positive whole amount (no decimal numbers) to gamble @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                int walletBalance = await _bank.CheckBalance(chatter.Username, _broadcasterInstance.DatabaseId);

                // Check if user wants to gamble all of their wallet
                if (gambleMessage.Equals("all", StringComparison.CurrentCultureIgnoreCase))
                {
                    gambledMoney = walletBalance;
                }

                if (gambledMoney < 1)
                    _irc.SendPublicChatMessage($"Please insert a positive whole amount (no decimal numbers) to gamble @{chatter.DisplayName}");
                else if (gambledMoney > walletBalance)
                    _irc.SendPublicChatMessage($"You do not have the sufficient funds to gamble {gambledMoney} {_botConfig.CurrencyType} @{chatter.DisplayName}");
                else
                {
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int diceRoll = rnd.Next(1, 101); // between 1 and 100
                    int newBalance = 0;

                    string result = $"@{chatter.DisplayName} gambled ";
                    string allResponse = "";

                    if (gambledMoney == walletBalance)
                    {
                        allResponse = "ALL ";
                    }

                    result += $"{allResponse} {gambledMoney} {_botConfig.CurrencyType} and the dice roll was {diceRoll}. They ";

                    // Check the 100-sided die roll result
                    if (diceRoll < 61) // lose gambled money
                    {
                        newBalance = walletBalance - gambledMoney;

                        result += $"lost {allResponse} {gambledMoney} {_botConfig.CurrencyType}";
                    }
                    else if (diceRoll >= 61 && diceRoll <= 98) // earn double
                    {
                        walletBalance -= gambledMoney; // put money into the gambling pot (remove money from wallet)
                        newBalance = walletBalance + (gambledMoney * 2); // recieve 2x earnings back into wallet

                        result += $"won {gambledMoney * 2} {_botConfig.CurrencyType}";
                    }
                    else if (diceRoll == 99 || diceRoll == 100) // earn triple
                    {
                        walletBalance -= gambledMoney; // put money into the gambling pot (remove money from wallet)
                        newBalance = walletBalance + (gambledMoney * 3); // recieve 3x earnings back into wallet

                        result += $"won {gambledMoney * 3} {_botConfig.CurrencyType}";
                    }

                    await _bank.UpdateFunds(chatter.Username, _broadcasterInstance.DatabaseId, newBalance);

                    // Show how much the user has left if they didn't gamble all of their currency or gambled all and lost
                    if (allResponse != "ALL " || (allResponse == "ALL " && diceRoll < 61))
                    {
                        string possession = "has";

                        if (newBalance > 1)
                            possession = "have";

                        result += $" and now {possession} {newBalance} {_botConfig.CurrencyType}";
                    }

                    _irc.SendPublicChatMessage(result);
                    return DateTime.Now.AddSeconds(20);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "Gamble(TwitchChatter)", false, "!gamble", chatter.Message);
            }

            return DateTime.Now;
        }
    }
}
