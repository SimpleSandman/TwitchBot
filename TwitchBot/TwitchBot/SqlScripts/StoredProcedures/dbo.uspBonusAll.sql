CREATE PROCEDURE [dbo].[uspBonusAll]
	@tvpBonusAll dbo.tblBonusAll READONLY
AS
	MERGE INTO dbo.tblBank WITH (HOLDLOCK) AS target
	USING @tvpBonusAll AS source
		ON target.username = source.username
		AND target.broadcaster = source.broadcaster
	WHEN MATCHED THEN 
		UPDATE SET target.wallet = source.wallet
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (username, wallet, broadcaster, timeAdded)
		VALUES (source.username, source.DateTimewallet, source.broadcaster, source.timeAdded);
RETURN 0
