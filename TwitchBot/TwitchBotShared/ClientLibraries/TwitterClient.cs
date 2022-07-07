using System;

using CoreTweet;

namespace TwitchBotShared.ClientLibraries
{
    public class TwitterClient
    {
        private static volatile TwitterClient _instance;
        private static object _syncRoot = new Object();

        private TwitterClient() { }

        public bool HasCredentials { get; set; } = false;
        public string ScreenName { get; set; }
        public Tokens Tokens { get; set; }

        public static TwitterClient Instance
        {
            get
            {
                // first check
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        // second check
                        if (_instance == null)
                            _instance = new TwitterClient();
                    }
                }

                return _instance;
            }
        }

        public string SendTweet(string message)
        {
            try
            {
                if (message.Length <= 280)
                {
                    Tokens.Statuses.Update(new { status = message });
                    return "Tweet successfully published!";
                }
                else
                {
                    int overCharLimit = message.Length - 280;
                    return $"The message you attempted to tweet had {overCharLimit}" +
                        " characters more than the 280 character limit. Please shorten your message and try again";
                }
            }
            catch
            {
                return "Unknown error has occurred for your proposed tweet.";
            }
        }
    }
}
