using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Configuration
{
    public sealed class TwitchBotConfigurationSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties;
        private static bool _readOnly;

        private static readonly ConfigurationProperty _botName =
            new ConfigurationProperty("botName", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _broadcaster =
            new ConfigurationProperty("broadcaster", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitchOAuth =
            new ConfigurationProperty("twitchOAuth", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitchClientId =
            new ConfigurationProperty("twitchClientId", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitchAccessToken =
            new ConfigurationProperty("twitchAccessToken", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitterConsumerKey =
            new ConfigurationProperty("twitterConsumerKey", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitterConsumerSecret =
            new ConfigurationProperty("twitterConsumerSecret", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitterAccessToken =
            new ConfigurationProperty("twitterAccessToken", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitterAccessSecret =
            new ConfigurationProperty("twitterAccessSecret", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _discordLink =
            new ConfigurationProperty("discordLink", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _currencyType =
            new ConfigurationProperty("currencyType", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _enableTweet =
            new ConfigurationProperty("enableTweets", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _enableDisplaySong =
            new ConfigurationProperty("enableDisplaySong", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _streamLatency =
            new ConfigurationProperty("streamLatency", typeof(int), 12, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubeClientId =
            new ConfigurationProperty("youTubeClientId", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubeClientSecret =
            new ConfigurationProperty("youTubeClientSecret", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubeCode =
            new ConfigurationProperty("youTubeCode", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubeAccessToken =
            new ConfigurationProperty("youTubeAccessToken", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubeRefreshToken =
            new ConfigurationProperty("youTubeRefreshToken", typeof(string), "", ConfigurationPropertyOptions.None);

        public TwitchBotConfigurationSection()
        {
            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_botName);
            _properties.Add(_broadcaster);
            _properties.Add(_twitchOAuth);
            _properties.Add(_twitchClientId);
            _properties.Add(_twitchAccessToken);
            _properties.Add(_twitterConsumerKey);
            _properties.Add(_twitterConsumerSecret);
            _properties.Add(_twitterAccessToken);
            _properties.Add(_twitterAccessSecret);
            _properties.Add(_discordLink);
            _properties.Add(_currencyType);
            _properties.Add(_enableTweet);
            _properties.Add(_enableDisplaySong);
            _properties.Add(_streamLatency);
            _properties.Add(_youTubeClientId);
            _properties.Add(_youTubeClientSecret);
            _properties.Add(_youTubeCode);
            _properties.Add(_youTubeAccessToken);
            _properties.Add(_youTubeRefreshToken);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        private new bool IsReadOnly
        {
            get
            {
                return _readOnly;
            }
        }

        private void ThrowIfReadOnly(string propertyName)
        {
            if (IsReadOnly)
                throw new ConfigurationErrorsException($"The property {propertyName} is read only.");
        }

        protected override object GetRuntimeObject()
        {
            _readOnly = false;
            return base.GetRuntimeObject();
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\",
        MinLength = 1, MaxLength = 60)]
        public string BotName
        {
            get { return (string)this["botName"]; }
            set
            {
                ThrowIfReadOnly("BotName");
                this["botName"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\",
            MinLength = 1, MaxLength = 60)]
        public string Broadcaster
        {
            get { return (string)this["broadcaster"]; }
            set
            {
                ThrowIfReadOnly("Broadcaster");
                this["broadcaster"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitchOAuth
        {
            get { return (string)this["twitchOAuth"]; }
            set
            {
                ThrowIfReadOnly("TwitchOAuth");
                this["twitchOAuth"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitchClientId
        {
            get { return (string)this["twitchClientId"]; }
            set
            {
                ThrowIfReadOnly("TwitchClientId");
                this["twitchClientId"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitchAccessToken
        {
            get { return (string)this["twitchAccessToken"]; }
            set
            {
                ThrowIfReadOnly("TwitchAccessToken");
                this["twitchAccessToken"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitterConsumerKey
        {
            get { return (string)this["twitterConsumerKey"]; }
            set
            {
                ThrowIfReadOnly("TwitterConsumerKey");
                this["twitterConsumerKey"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitterConsumerSecret
        {
            get { return (string)this["twitterConsumerSecret"]; }
            set
            {
                ThrowIfReadOnly("TwitterConsumerSecret");
                this["twitterConsumerSecret"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitterAccessToken
        {
            get { return (string)this["twitterAccessToken"]; }
            set
            {
                ThrowIfReadOnly("TwitterAccessToken");
                this["twitterAccessToken"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string TwitterAccessSecret
        {
            get { return (string)this["twitterAccessSecret"]; }
            set
            {
                ThrowIfReadOnly("TwitterAccessSecret");
                this["twitterAccessSecret"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string DiscordLink
        {
            get { return (string)this["discordLink"]; }
            set
            {
                ThrowIfReadOnly("DiscordLink");
                this["discordLink"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\")]
        public string CurrencyType
        {
            get { return (string)this["currencyType"]; }
            set
            {
                ThrowIfReadOnly("CurrencyType");
                this["currencyType"] = value;
            }
        }

        public bool EnableTweets
        {
            get { return (bool)this["enableTweets"]; }
            set
            {
                ThrowIfReadOnly("EnableTweets");
                this["enableTweets"] = value;
            }
        }

        public bool EnableDisplaySong
        {
            get { return (bool)this["enableDisplaySong"]; }
            set
            {
                ThrowIfReadOnly("EnableDisplaySong");
                this["enableDisplaySong"] = value;
            }
        }

        public int StreamLatency
        {
            get { return (int)this["streamLatency"]; }
            set
            {
                ThrowIfReadOnly("StreamLatency");
                this["streamLatency"] = value;
            }
        }

        public string YouTubeClientId
        {
            get { return (string)this["youTubeClientId"]; }
            set
            {
                ThrowIfReadOnly("YouTubeClientId");
                this["youTubeClientId"] = value;
            }
        }

        public string YouTubeClientSecret
        {
            get { return (string)this["youTubeClientSecret"]; }
            set
            {
                ThrowIfReadOnly("YouTubeClientSecret");
                this["youTubeClientSecret"] = value;
            }
        }

        public string YouTubeCode
        {
            get { return (string)this["youTubeCode"]; }
            set
            {
                ThrowIfReadOnly("YouTubeCode");
                this["youTubeCode"] = value;
            }
        }

        public string YouTubeAccessToken
        {
            get { return (string)this["youTubeAccessToken"]; }
            set
            {
                ThrowIfReadOnly("YouTubeAccessToken");
                this["youTubeAccessToken"] = value;
            }
        }

        public string YouTubeRefreshToken
        {
            get { return (string)this["youTubeRefreshToken"]; }
            set
            {
                ThrowIfReadOnly("YouTubeRefreshToken");
                this["youTubeRefreshToken"] = value;
            }
        }

    }
}
