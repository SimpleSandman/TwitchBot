USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblQuote] Script Date: 9/8/2016 2:10:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Quote] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [userQuote]   VARCHAR (500) NOT NULL,
    [username]    VARCHAR (50)  NOT NULL,
    [timeCreated] DATETIME      DEFAULT (getdate()) NOT NULL,
    [broadcaster] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblQuote_tblBroadcasters] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[Broadcasters] ([Id])
);

