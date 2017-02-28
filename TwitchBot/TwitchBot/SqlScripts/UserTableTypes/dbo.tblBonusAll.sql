CREATE TYPE [dbo].[tblBonusAll] AS TABLE (
	[Id]          INT          NULL,
	[username]    VARCHAR (30) NOT NULL,
	[wallet]      INT          NOT NULL,
	[broadcaster] INT		   NOT NULL,
	[timeAdded]   DATETIME     DEFAULT (getdate()) NOT NULL
);