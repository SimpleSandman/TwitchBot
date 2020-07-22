CREATE PROCEDURE [dbo].[UpdateCreateBalance]
	@tvpUsernames dbo.Usernames READONLY,
	@intDeposit INT,
	@intBroadcasterID INT,
	@bitShowOutput BIT = 0
AS
	CREATE TABLE #tblResult (
		ActionType NVARCHAR(10), 
		Username VARCHAR(30) NOT NULL,
		Wallet INT NOT NULL
	);

	MERGE INTO dbo.Bank WITH (HOLDLOCK) AS target
	USING @tvpUsernames AS source
		ON target.Username = source.Username
		AND target.Broadcaster = @intBroadcasterID
	WHEN MATCHED THEN
		UPDATE SET target.Wallet = target.Wallet + @intDeposit
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (Username, Wallet, Broadcaster)
		VALUES (source.Username, @intDeposit, @intBroadcasterID)
	OUTPUT $action ActionType, source.Username, inserted.Wallet
	INTO #tblResult (ActionType, Username, Wallet);

	IF @bitShowOutput = 1
	BEGIN
		SELECT * FROM #tblResult
	END

	DROP TABLE #tblResult
GO
