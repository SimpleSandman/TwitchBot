USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblPartyUpRequests] Script Date: 9/8/2016 2:10:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblPartyUpRequests] (
    [Id]            INT          IDENTITY (1, 1) NOT NULL,
    [username]      VARCHAR (30) NOT NULL,
    [partyMember]   VARCHAR (50) NOT NULL,
    [timeRequested] DATETIME     DEFAULT (getdate()) NOT NULL,
    [broadcaster]   INT          NOT NULL,
    [game]          INT          NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblPartyUpRequests_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id]),
    CONSTRAINT [FK_tblPartyUpRequests_tblGameList] FOREIGN KEY ([game]) REFERENCES [dbo].[tblGameList] ([Id])
);


