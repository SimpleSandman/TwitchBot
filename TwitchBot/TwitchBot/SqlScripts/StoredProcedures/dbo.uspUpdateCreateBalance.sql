CREATE PROCEDURE [dbo].[uspUpdateCreateBalance]
	@tvpUsernames dbo.tblUsernames READONLY,
	@intDeposit INT,
	@intBroadcasterID INT,
	@bitShowOutput BIT = 0
AS
	DECLARE @tblResult TABLE (
		actionType NVARCHAR(10), 
		username VARCHAR(30) NOT NULL,
		wallet INT NOT NULL
	);

	MERGE INTO dbo.tblBank WITH (HOLDLOCK) AS target
	USING @tvpUsernames AS source
		ON target.username = source.username
		AND target.broadcaster = @intBroadcasterID
	WHEN MATCHED THEN
		UPDATE SET target.wallet = target.wallet + @intDeposit
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (username, wallet, broadcaster)
		VALUES (source.username, @intDeposit, @intBroadcasterID)
	OUTPUT $action actionType, source.username, inserted.wallet
	INTO @tblResult (actionType, username, wallet);

	IF @bitShowOutput = 1
	BEGIN
		SELECT * FROM @tblResult
	END
GO
