CREATE PROCEDURE [dbo].[CreateNewBossesWithDefaultStats]
(
    @SettingsId INT
  , @NewGameId INT
  , @Name1 VARCHAR(50)
  , @Name2 VARCHAR(50)
  , @Name3 VARCHAR(50)
  , @Name4 VARCHAR(50)
  , @Name5 VARCHAR(50)
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    IF @SettingsId IS NULL
        OR @NewGameId IS NULL
        OR @Name1 IS NULL
        OR @Name2 IS NULL
        OR @Name2 IS NULL
        OR @Name2 IS NULL
        OR @Name2 IS NULL
    BEGIN
        RAISERROR('Found a NULL value when validating the input parameters', 16, 1);
        SET NOEXEC ON;
    END;

    INSERT INTO dbo.BossFightBossStats (SettingsId, GameId, Name1, MaxUsers1, Attack1, Defense1, Evasion1, Health1, TurnLimit1, Loot1, LastAttackBonus1, Name2
                                      , MaxUsers2, Attack2, Defense2, Evasion2, Health2, TurnLimit2, Loot2, LastAttackBonus2, Name3, MaxUsers3, Attack3
                                      , Defense3, Evasion3, Health3, TurnLimit3, Loot3, LastAttackBonus3, Name4, MaxUsers4, Attack4, Defense4, Evasion4
                                      , Health4, TurnLimit4, Loot4, LastAttackBonus4, Name5, MaxUsers5, Attack5, Defense5, Evasion5, Health5, TurnLimit5, Loot5
                                      , LastAttackBonus5)
    SELECT @SettingsId AS SettingsId
         , @NewGameId AS GameId
         , @Name1 AS Name1
         , MaxUsers1
         , Attack1
         , Defense1
         , Evasion1
         , Health1
         , TurnLimit1
         , Loot1
         , LastAttackBonus1
         , @Name2 AS Name2
         , MaxUsers2
         , Attack2
         , Defense2
         , Evasion2
         , Health2
         , TurnLimit2
         , Loot2
         , LastAttackBonus2
         , @Name3 AS Name3
         , MaxUsers3
         , Attack3
         , Defense3
         , Evasion3
         , Health3
         , TurnLimit3
         , Loot3
         , LastAttackBonus3
         , @Name4 AS Name4
         , MaxUsers4
         , Attack4
         , Defense4
         , Evasion4
         , Health4
         , TurnLimit4
         , Loot4
         , LastAttackBonus4
         , @Name5 AS Name5
         , MaxUsers5
         , Attack5
         , Defense5
         , Evasion5
         , Health5
         , TurnLimit5
         , Loot5
         , LastAttackBonus5
    FROM dbo.BossFightBossStats
    WHERE GameId IS NULL
        AND SettingsId = @SettingsId;
END;
GO


