# Simple Bot
Custom chat bot for Twitch TV

This is an open-source Console application that will benefit anyone who wants to have a foundation of making their own Twitch bot. This is primarly written in C#/SQL Server using an Azure database from Microsoft. Currently, this is not end-user friendly because I am concentrating on the logic of the bot first.

This bot has been revamped recently with a new configuration wizard that will allow new users to insert their credentials for the bot. For developers, please read further down for details on manually setting a dev configuration so the information is always saved.

Check out the wiki for the full list of commands by [clicking here](https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands)!

The bot itself is an account on Twitch that I have made in order to have a custom bot name.

For development environment testing, create 2 files in the same folder as App.config. If you have any issues setting up this bot, please look further below for possible solutions.

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
  <add name="TwitchBotConnStrPROD" 
       connectionString="[ConnectionString]"
       providerName="" />
  <add name="TwitchBotConnStrTEST" 
       connectionString="[ConnectionString]"
       providerName="" />
</connectionStrings>
```

Set both files to copy-if-newer so that they get included in the compilation.  For production this files are not needed, and the bot will ask for configuration on first run

## Possible setup issues:
- Connection string error `Configuration System Failed to Initialize`
  - Delete old config files from these file locations and restart the debugger:
    - `C:\Users\[username]\AppData\Local\[appname]`
    - `C:\Users\[username]\AppData\Roaming\[appname]`
  - Source: http://stackoverflow.com/q/6436157/2113548
