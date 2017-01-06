USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblRank] Script Date: 1/5/2017 8:39:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblRank] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [name]        VARCHAR (30) NOT NULL,
    [expCap]      INT          NOT NULL,
    [broadcaster] INT          NOT NULL
);


