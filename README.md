# Simple Bot
Custom chat bot for Twitch TV

This is an open-source project that will benefit anyone who wants to have a foundation of making their own Twitch bot. This is primarly written in C#/SQL Server using an Azure database from Microsoft. Currently, this is not end-user friendly because I am concentrating on the logic of the bot first.

This is a console application that requires an Azure database login. Please change the credentials from what is currently implemented to your database's credentials. This can be done through the `app.config` file. The username and password are manually entered every time the application runs to prevent compromise of the user's credentials. The `oauth` and `clientID` are being stored into the database as well.

After entering the password, the program will look for a local Spotify client. If there is one available, it will attempt to grab the song that is currently playing and post it onto the chat. This chat bot can control some of Spotify's music player like `!spotifyplay`, `!spotifypause`, `!spotifyskip`, and `!spotifyprev`.

Check out the wiki for the full list of commands by [clicking here](https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands)!

The bot itself is an account on Twitch that I have made in order to have a custom bot name.

For development environment testing create 2 files in the same folder as App.config


## AppConfigSecrets.config

```xml
<TwitchBotConfiguration botName="[BotName]" broadcaster="[BroadcasterName]"
    twitchOAuth="[OAuth]" twitchClientId="[ClientId]"
    twitchAccessToken="[AccessToken]" twitterConsumerKey="[ConsumerKey]" 
    twitterConsumerSecret="[ConsumerSecret]"
    twitterAccessToken="[AccessToken]" twitterAccessSecret="[AccessSecret]" discordLink="[DiscordLink]"
    currencyType="[CurrencyName] (optional)" enableTweets="false" enableDisplaySong="false"
    streamLatency="12" />
```

## ConnectionStringsSecrets.config

```xml
<connectionStrings>
  <add name="TwitchBotConnectionString" connectionString="[ConnectionString]"
    providerName="" />
</connectionStrings>
```

Set both files to copy-if-newer so that they get included in the compilation.  For production this files are not needed, and the bot will ask for configuration on first run