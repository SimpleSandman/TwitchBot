# Simple Bot [![Build status](https://ci.appveyor.com/api/projects/status/k0cgg8xeqgh58uc7?svg=true)](https://ci.appveyor.com/project/SimpleSandman/twitchbot) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSimpleSandman%2FTwitchBot.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FSimpleSandman%2FTwitchBot?ref=badge_shield)
Custom chat bot for Twitch TV

This is an open-source console application with a .NET Core Web API that will benefit anyone who wants to have a foundation of making their own Twitch bot. This is primarly written in C#/SQL Server using an Azure SQL database from Microsoft. Currently, this bot is not end-user friendly because I'm concentrating on the logic of the bot first.

For developers, please read further down for details on manually setting a dev configuration so the information is always saved.

Check out the wiki for the full list of commands by [clicking here](https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands)!

The bot itself is an account on Twitch that I have made in order to have a custom bot name.

For a development environment (testing), create an `AppConfigSecrets.config` in the same folder as `App.config`. If you have any issues setting up this bot, please look further below for possible solutions.

## Twitch Chat OAuth Password Generator

The chat bot needs access to the IRC chat in order to communicate with the broadcaster and their viewers.

While you're signed into the chat bot account on Twitch, connect to the [Twitch Chat OAuth Password Generator](https://www.twitchapps.com/tmi/) site and grab the `oauth:XXXXXXXXXXXXXX` string for the `twitchOAuth` config variable.

## Twitch OAuth Implicit Access Token

In order for the chat bot to run correctly, we need a few permissions:
- `channel:manage:broadcast`
- `channel:read:subscriptions`
- `moderation:read`
- `user:read:email`
- `user:read:subscriptions`

For more information on the permissions above, please refer to the scope documentation [here](https://dev.twitch.tv/docs/authentication#scopes).

NOTE: You'll need to add your own Client-ID since this will be based on a proxy Twitch account for the chat bot. Also, we will be utilizing `http://localhost` for development purposes.

This is the "complete" request URL you'll paste into your browser and allow the chat bot access to your channel.

```
https://id.twitch.tv/oauth2/authorize?client_id=<client id goes here>&redirect_uri=http://localhost&response_type=token&scope=user:read:email+channel:read:subscriptions+channel:manage:broadcast+moderation:read+user:read:subscriptions
```

This is the "complete" response URL you'll see in your browser once you authenticate implicitly. Copy the access token from the response URL and paste it into the `twitchAccessToken` config variable.

```
http://localhost/#access_token=<copy access token from here>&scope=<scopes assigned from above>&token_type=bearer
```

For further documentation on "OAuth Implicit Code Flow", please refer to [this link here](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#oauth-implicit-code-flow).

## AppConfigSecrets.config

```xml
<TwitchBotConfiguration 
    botName="" 
    broadcaster="" 
    currencyType=""
    discordServerName=""
    discordToken=""
    enableDisplaySong="false"
    enableTweets="false" 
    libVLCAudioOutputDevice=""
    regularFollowerHours="30"
    spotifyClientId=""
    spotifyRedirectUri=""
    streamLatency="10" 
    twitchBotApiLink=""
    twitchOAuth="" 
    twitchClientId=""
    twitchAccessToken="" 
    twitterConsumerKey="" 
    twitterConsumerSecret=""
    twitterAccessToken="" 
    twitterAccessSecret="" 
    youTubeClientId="" 
    youTubeClientSecret=""
    youTubeBroadcasterPlaylistId=""
    youTubeBroadcasterPlaylistName="" />
```

Set file to `copy-if-newer` so it's included in the compilation. For production, this file is not needed and the bot will ask for configuration on first run

## Spotify Example Setup

Go to the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/) and create a new app with these settings below:

- Website:
  - `http://localhost:5000`
- Redirect URL:
  - `http://localhost:5000/callback`

Of course if you're using your own web server, replace `localhost` with your domain. Here we're using HTTP for local reasons. Always use HTTPS outside of a local environment.

## Possible Setup Issues:
- IIS HTTP Error 404.11 - Not Found `The request filtering module is configured to deny a request that contains a double escape sequence`
  - Add this line into the file: `C:\[PathToSolution]\.vs\config\applicationhost.config`
    ```xml
    <system.webServer>
        <security>
            <requestFiltering allowDoubleEscaping="true"/>
        </security>
    </system.webServer>
    ```
  - Workaround Source: https://stackoverflow.com/a/1453287/2113548
  - File Location Source: https://stackoverflow.com/q/12946476/2113548

## Host Self-Contained ASP.NET Core Web API on Windows with IIS
- Official Guide: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/
- This will be an ongoing doc as to how to publish the Web API so you don't have to run IIS Express

- My settings when publishing this app via "File System":
  - Configuration:
    - Release - x64
  - Target Framework:
    - netcoreapp3.1
  - Deployment Mode:
    - Self-Contained
  - Target Runtime:
    - win-x64
  - File Publish Options (checked):
    - Delete all existing files prior to publish
    
- Remember to give `IIS AppPool\[app_pool_name]` (at least) read & execute permissions to the deployed folder
  - This will allow the files to be overwritten as needed on deployment
  - Reference: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?application-pool-identity

# Scaffold Commands
*In case if you need to scaffold anything, here are some commands that may be useful*

## Models and DbContext

This is a single-line command using the "Package Manager Console" in Visual Studio that allows you to generate **ALL** of the models and the DbContext class.
```powershell
Scaffold-DbContext 'Data Source=;Initial Catalog=;User ID=;Password=;' Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Context
```

If you only need the model and context of a **SINGLE** table, here's the single-line command for that.
```powershell
Scaffold-DbContext 'Data Source=;Initial Catalog=;User ID=;Password=;' Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Context -T <TABLE_NAME_HERE>
```
