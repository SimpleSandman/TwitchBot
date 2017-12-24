USE [twitchbotdb]
GO

/****** Object: Table [dbo].[BossFightSettings] Script Date: 12/24/2017 1:20:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BossFightSettings] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [cooldownPeriodMin] INT           DEFAULT ((10)) NOT NULL,
    [entryPeriodSec]    INT           DEFAULT ((60)) NOT NULL,
    [entryMessage]      VARCHAR (150) DEFAULT ('@user@ is trying to get a group of adventurers ready to fight a boss... Will you join them? Type !raid to join!') NOT NULL,
    [cost]              INT           DEFAULT ((100)) NOT NULL,
    [cooldownEntry]     VARCHAR (150) DEFAULT ('The boss floor is currently being cleaned up and won''t be available for at least @timeleft@') NOT NULL,
    [cooldownOver]      VARCHAR (150) DEFAULT ('The boss floor has been cleaned up... Want to go again?! Type !raid to start!') NOT NULL,
    [nextLevelMessage2] VARCHAR (200) DEFAULT ('With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!') NOT NULL,
    [nextLevelMessage3] VARCHAR (200) DEFAULT ('Oh yeah! With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!') NOT NULL,
    [nextLevelMessage4] VARCHAR (200) DEFAULT ('Hell yeah! We can now attack @bossname@. A few more and we could attack @nextbossname@!') NOT NULL,
    [nextLevelMessage5] VARCHAR (200) DEFAULT ('Epic raid party! We are going to attack @bossname@ guys! Gear up and get ready to head out.') NOT NULL,
    [gameStart]         VARCHAR (200) DEFAULT ('The combatants have stepped into the boss room... Will they be able to defeat @bossname@?!') NOT NULL,
    [resultsMessage]    VARCHAR (50)  DEFAULT ('The survivors are:') NOT NULL,
    [singleUserSuccess] VARCHAR (200) DEFAULT ('@user@ executed the raid flawlessly, soloing @bossname@ with ease and looting @winamount@ @pointsname@ with a last attack bonus of @lastattackbonus@ @pointsname@.') NOT NULL,
    [singleUserFail]    VARCHAR (200) DEFAULT ('@user@ thought they could try to solo @bossname@, but was deleted immediately...RIP') NOT NULL,
    [success100]        VARCHAR (200) DEFAULT ('The raid was a complete success. No one fell into the hands of @bossname@') NOT NULL,
    [success34]         VARCHAR (200) DEFAULT ('The raid party suffered a few casualties as they fought valiantly.') NOT NULL,
    [success1]          VARCHAR (200) DEFAULT ('The raid party suffered major casualties as they fought valiantly.') NOT NULL,
    [success0]          VARCHAR (200) DEFAULT ('It was absolute hell. The field is covered with the blood of the fallen...no one survived') NOT NULL,
    [broadcaster]       INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblBossFightSettings_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[Broadcasters] ([Id])
);

