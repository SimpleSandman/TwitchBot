USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblPartyUp] Script Date: 9/8/2016 2:10:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblPartyUp] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [partyMember] VARCHAR (100) NOT NULL,
    [game]        INT           NOT NULL,
    [broadcaster] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblPartyUp_tblGameList] FOREIGN KEY ([game]) REFERENCES [dbo].[tblGameList] ([Id]),
    CONSTRAINT [FK_tblPartyUps_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);


