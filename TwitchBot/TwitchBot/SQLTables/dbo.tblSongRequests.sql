USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblSongRequests] Script Date: 9/8/2016 2:10:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblSongRequests] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [songRequests] VARCHAR (200) NOT NULL,
    [broadcaster]  INT           NOT NULL,
    [chatter]      VARCHAR (30)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblSongRequests_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);

