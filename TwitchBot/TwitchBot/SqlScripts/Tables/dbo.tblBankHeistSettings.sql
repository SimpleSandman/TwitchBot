USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblBankHeistSettings] Script Date: 9/16/2017 8:45:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblBankHeistSettings] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [broadcaster]        INT            NOT NULL,
    [cooldownPeriodMin]  INT            DEFAULT ((10)) NOT NULL,
    [entryPeriodSec]     INT            DEFAULT ((120)) NOT NULL,
    [entryMessage]       VARCHAR (150)  DEFAULT ('@user@ has started planning a bank heist! Looking for a bigger crew for a bigger score. Join in!') NOT NULL,
    [maxGamble]          INT            DEFAULT ((5000)) NOT NULL,
    [maxGambleText]      VARCHAR (100)  DEFAULT ('Buy-in to''ps out at @maxbet@ @pointsname@') NOT NULL,
    [entryInstructions]  VARCHAR (50)   DEFAULT ('Type @command@ [x] to enter') NOT NULL,
    [cooldownEntry]      VARCHAR (150)  DEFAULT ('The cops are on high alert after the last job, we have to lay low for a bit. Call me again after @timeleft@ minutes') NOT NULL,
    [cooldownOver]       VARCHAR (150)  DEFAULT ('Looks like the cops have given up the search … the banks are ripe for hitting!') NOT NULL,
    [nextLevelMessage2]  VARCHAR (200)  DEFAULT ('With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!') NOT NULL,
    [nextLevelMessage3]  VARCHAR (200)  DEFAULT ('Oh yeah! With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!') NOT NULL,
    [nextLevelMessage4]  VARCHAR (200)  DEFAULT ('Hell yeah! We can now hit the @bankname@. A few more, and we could hit the @nextbankname@!') NOT NULL,
    [nextLevelMessage5]  VARCHAR (200)  DEFAULT ('Epic crew! We are going to hit the @bankname@ guys! Gear up and get ready to head out.') NOT NULL,
    [gameStart]          VARCHAR (200)  DEFAULT ('Alright guys, check your guns. We are storming into the @bankname@ through all entrances. Let''s get the cash and get out before the cops get here.') NOT NULL,
    [singleUserSuccess]  VARCHAR (200)  DEFAULT ('@user@ executed the heist flawlessly, sneaking into the @bankname@ through the back entrance and looting @winamount@ @pointsname@ from the vault.') NOT NULL,
    [singleUserFail]     VARCHAR (200)  DEFAULT ('Local security caught @user@ trying to sneak into the @bankname@ through the back entrance and opened fire.') NOT NULL,
    [resultsMessage]     VARCHAR (50)   DEFAULT ('The heist payouts are:') NOT NULL,
    [success100]         VARCHAR (200)  DEFAULT ('The execution was flawless, in and out before the first cop arrived on scene.') NOT NULL,
    [success34]          VARCHAR (200)  DEFAULT ('The crew suffered a few losses engaging the local security team.') NOT NULL,
    [success1]           VARCHAR (200)  DEFAULT ('The crew suffered major losses as they engaged the SWAT backup.') NOT NULL,
    [success0]           VARCHAR (200)  DEFAULT ('SWAT teams nearby stormed the bank and killed the entire crew. Not a single soul survived…') NOT NULL,
    [levelName1]         VARCHAR (50)   DEFAULT ('Simple Municipal Bank') NOT NULL,
    [levelMaxUsers1]     INT            DEFAULT ((9)) NOT NULL,
    [levelName2]         VARCHAR (50)   DEFAULT ('Simple City Bank') NOT NULL,
    [levelMaxUsers2]     INT            DEFAULT ((19)) NOT NULL,
    [levelName3]         VARCHAR (50)   DEFAULT ('Simple State Bank') NOT NULL,
    [levelMaxUsers3]     INT            DEFAULT ((29)) NOT NULL,
    [levelName4]         VARCHAR (50)   DEFAULT ('Simple National Reserve') NOT NULL,
    [levelMaxUsers4]     INT            DEFAULT ((39)) NOT NULL,
    [levelName5]         VARCHAR (50)   DEFAULT ('Simple Federal Reserve') NOT NULL,
    [levelMaxUsers5]     INT            DEFAULT ((9001)) NOT NULL,
    [payoutSuccessRate1] DECIMAL (5, 2) DEFAULT ((54.00)) NOT NULL,
    [payoutMultiplier1]  DECIMAL (3, 2) DEFAULT ((1.50)) NOT NULL,
    [payoutSuccessRate2] DECIMAL (5, 2) DEFAULT ((48.80)) NOT NULL,
    [payoutMultiplier2]  DECIMAL (3, 2) DEFAULT ((1.70)) NOT NULL,
    [payoutSuccessRate3] DECIMAL (5, 2) DEFAULT ((42.50)) NOT NULL,
    [payoutMultiplier3]  DECIMAL (3, 2) DEFAULT ((2.00)) NOT NULL,
    [payoutSuccessRate4] DECIMAL (5, 2) DEFAULT ((38.70)) NOT NULL,
    [payoutMultiplier4]  DECIMAL (3, 2) DEFAULT ((2.25)) NOT NULL,
    [payoutSuccessRate5] DECIMAL (5, 2) DEFAULT ((32.40)) NOT NULL,
    [payoutMultiplier5]  DECIMAL (3, 2) DEFAULT ((2.75)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblBankHeistSettings_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);

