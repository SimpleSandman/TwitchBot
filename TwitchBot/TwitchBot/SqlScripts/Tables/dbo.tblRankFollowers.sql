USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblRankFollowers] Script Date: 1/5/2017 8:43:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblRankFollowers] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [username]    VARCHAR (30) NOT NULL,
    [exp]         INT          NOT NULL,
    [broadcaster] INT          NOT NULL
);


