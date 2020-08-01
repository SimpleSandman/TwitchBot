using System.Configuration;

namespace TwitchBotShared.Config
{
    public sealed class TwitchBotConfigurationSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties;
        private static bool _readOnly;

        private static readonly ConfigurationProperty _botName =
            new ConfigurationProperty("botName", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _broadcaster =
            new ConfigurationProperty("broadcaster", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _twitchBotApiLink =
            new ConfigurationProperty("twitchBotApiLink", typeof(string), "", ConfigurationPropertyOptions.None);

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

        private static readonly ConfigurationProperty _youTubeBroadcasterPlaylistId =
            new ConfigurationProperty("youTubeBroadcasterPlaylistId", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubeBroadcasterPlaylistName =
            new ConfigurationProperty("youTubeBroadcasterPlaylistName", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubePersonalPlaylistId =
            new ConfigurationProperty("youTubePersonalPlaylistId", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _youTubePersonalPlaylistName =
            new ConfigurationProperty("youTubePersonalPlaylistName", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _regularFollowerHours =
            new ConfigurationProperty("regularFollowerHours", typeof(int), 30, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _spotifyClientId =
            new ConfigurationProperty("spotifyClientId", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _spotifyRedirectUri =
            new ConfigurationProperty("spotifyRedirectUri", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _spotifyServerUri =
            new ConfigurationProperty("spotifyServerUri", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _libVLCAudioOutputDevice =
            new ConfigurationProperty("libVLCAudioOutputDevice", typeof(string), "", ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _enablePersonalPlaylistShuffle =
            new ConfigurationProperty("enablePersonalPlaylistShuffle", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _isManualSongRequestAvail =
            new ConfigurationProperty("isManualSongRequestAvail", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _isYouTubeSongRequestAvail =
            new ConfigurationProperty("isYouTubeSongRequestAvail", typeof(bool), false, ConfigurationPropertyOptions.None);

        public TwitchBotConfigurationSection()
        {
            _properties = new ConfigurationPropertyCollection
            {
                _botName,
                _broadcaster,
                _twitchBotApiLink,
                _twitchOAuth,
                _twitchClientId,
                _twitchAccessToken,
                _twitterConsumerKey,
                _twitterConsumerSecret,
                _twitterAccessToken,
                _twitterAccessSecret,
                _currencyType,
                _enableTweet,
                _enableDisplaySong,
                _streamLatency,
                _youTubeClientId,
                _youTubeClientSecret,
                _youTubeBroadcasterPlaylistId,
                _youTubeBroadcasterPlaylistName,
                _youTubePersonalPlaylistId,
                _youTubePersonalPlaylistName,
                _regularFollowerHours,
                _spotifyClientId,
                _spotifyRedirectUri,
                _spotifyServerUri,
                _libVLCAudioOutputDevice,
                _enablePersonalPlaylistShuffle,
                _isManualSongRequestAvail,
                _isYouTubeSongRequestAvail
            };
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

        public string TwitchBotApiLink
        {
            get { return (string)this["twitchBotApiLink"]; }
            set
            {
                ThrowIfReadOnly("TwitchBotApiLink");
                this["twitchBotApiLink"] = value;
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

        public string YouTubeBroadcasterPlaylistId
        {
            get { return (string)this["youTubeBroadcasterPlaylistId"]; }
            set
            {
                ThrowIfReadOnly("YouTubeBroadcasterPlaylistId");
                this["youTubeBroadcasterPlaylistId"] = value;
            }
        }

        public string YouTubeBroadcasterPlaylistName
        {
            get { return (string)this["youTubeBroadcasterPlaylistName"]; }
            set
            {
                ThrowIfReadOnly("YouTubeBroadcasterPlaylistName");
                this["youTubeBroadcasterPlaylistName"] = value;
            }
        }

        public int RegularFollowerHours
        {
            get { return (int)this["regularFollowerHours"]; }
            set
            {
                ThrowIfReadOnly("RegularFollowerHours");
                this["regularFollowerHours"] = value;
            }
        }

        public string SpotifyClientId
        {
            get { return (string)this["spotifyClientId"]; }
            set
            {
                ThrowIfReadOnly("SpotifyClientId");
                this["spotifyClientId"] = value;
            }
        }

        public string SpotifyRedirectUri
        {
            get { return (string)this["spotifyRedirectUri"]; }
            set
            {
                ThrowIfReadOnly("SpotifyRedirectUri");
                this["spotifyRedirectUri"] = value;
            }
        }

        public string SpotifyServerUri
        {
            get { return (string)this["spotifyServerUri"]; }
            set
            {
                ThrowIfReadOnly("SpotifyServerUri");
                this["spotifyServerUri"] = value;
            }
        }

        public string YouTubePersonalPlaylistId
        {
            get { return (string)this["youTubePersonalPlaylistId"]; }
            set
            {
                ThrowIfReadOnly("YouTubePersonalPlaylistId");
                this["youTubePersonalPlaylistId"] = value;
            }
        }

        public string YouTubePersonalPlaylistName
        {
            get { return (string)this["youTubePersonalPlaylistName"]; }
            set
            {
                ThrowIfReadOnly("YouTubePersonalPlaylistName");
                this["youTubePersonalPlaylistName"] = value;
            }
        }

        public string LibVLCAudioOutputDevice
        {
            get { return (string)this["libVLCAudioOutputDevice"]; }
            set
            {
                ThrowIfReadOnly("LibVLCAudioOutputDevice");
                this["libVLCAudioOutputDevice"] = value;
            }
        }

        public bool EnablePersonalPlaylistShuffle
        {
            get { return (bool)this["enablePersonalPlaylistShuffle"]; }
            set
            {
                ThrowIfReadOnly("EnablePersonalPlaylistShuffle");
                this["enablePersonalPlaylistShuffle"] = value;
            }
        }

        public bool IsManualSongRequestAvail
        {
            get { return (bool)this["isManualSongRequestAvail"]; }
            set
            {
                ThrowIfReadOnly("IsManualSongRequestAvail");
                this["isManualSongRequestAvail"] = value;
            }
        }

        public bool IsYouTubeSongRequestAvail
        {
            get { return (bool)this["isYouTubeSongRequestAvail"]; }
            set
            {
                ThrowIfReadOnly("IsYouTubeSongRequestAvail");
                this["isYouTubeSongRequestAvail"] = value;
            }
        }
    }
}
