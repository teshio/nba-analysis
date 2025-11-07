USE [NBA];
GO

IF OBJECT_ID('dbo.vw_TeamSummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TeamSummary;
GO

CREATE VIEW dbo.vw_TeamSummary
AS
WITH TeamGames AS (
    SELECT
        g.GameID, g.GameDateTime,
        tHome.TeamID AS HomeTeamID,
        tAway.TeamID AS AwayTeamID,
        tHome.Stadium AS HomeStadium,
        tHome.TeamID AS TeamID,
        tAway.TeamID AS OpponentTeamID,
        CAST(1 AS bit) AS IsHome,
        g.HomeScore AS TeamScore,
        g.AwayScore AS OppScore,
        CASE WHEN g.HomeScore > g.AwayScore THEN 1 ELSE 0 END AS Won,
        (g.HomeScore - g.AwayScore) AS Diff,
        g.MVPPlayerID
    FROM dbo.Games g
    JOIN dbo.Teams tHome ON tHome.TeamID = g.HomeTeamID
    JOIN dbo.Teams tAway ON tAway.TeamID = g.AwayTeamID
    UNION ALL
    SELECT
        g.GameID, g.GameDateTime,
        tHome.TeamID, tAway.TeamID, tHome.Stadium,
        tAway.TeamID, tHome.TeamID,
        CAST(0 AS bit),
        g.AwayScore, g.HomeScore,
        CASE WHEN g.AwayScore > g.HomeScore THEN 1 ELSE 0 END,
        (g.AwayScore - g.HomeScore),
        g.MVPPlayerID
    FROM dbo.Games g
    JOIN dbo.Teams tHome ON tHome.TeamID = g.HomeTeamID
    JOIN dbo.Teams tAway ON tAway.TeamID = g.AwayTeamID
),
Agg AS (
    SELECT
        tg.TeamID,
        COUNT(*) AS GamesPlayed,
        SUM(CASE WHEN tg.Won = 1 THEN 1 ELSE 0 END) AS GamesWon,
        SUM(CASE WHEN tg.Won = 0 THEN 1 ELSE 0 END) AS GamesLost,
        SUM(CASE WHEN tg.IsHome = 1 THEN 1 ELSE 0 END) AS GamesHome,
        SUM(CASE WHEN tg.IsHome = 0 THEN 1 ELSE 0 END) AS GamesAway
    FROM TeamGames tg
    GROUP BY tg.TeamID
),
MvpOnTeam AS (
    SELECT
        tp.TeamID,
        p.PlayerID,
        p.Name,
        COUNT(*) AS MvpCount,
        MAX(tg.GameDateTime) AS LastMvpDate
    FROM TeamGames tg
    JOIN dbo.Team_Player tp ON tp.PlayerID = tg.MVPPlayerID AND tp.TeamID = tg.TeamID
    JOIN dbo.Players p ON p.PlayerID = tp.PlayerID
    GROUP BY tp.TeamID, p.PlayerID, p.Name
),
MvpPick AS (
    SELECT m.TeamID, m.Name AS SeasonMVP
    FROM (
        SELECT TeamID, Name, MvpCount, LastMvpDate,
               ROW_NUMBER() OVER (PARTITION BY TeamID ORDER BY MvpCount DESC, LastMvpDate DESC, Name ASC) AS rn
        FROM MvpOnTeam
    ) m
    WHERE m.rn = 1
),
BiggestWin AS (
    SELECT tg.TeamID, CONCAT(tg.TeamScore, '-', tg.OppScore) AS BiggestWinScore
    FROM (
        SELECT TeamID, TeamScore, OppScore,
               ROW_NUMBER() OVER (PARTITION BY TeamID ORDER BY Diff DESC, GameDateTime DESC, GameID DESC) AS rn
        FROM TeamGames
        WHERE Won = 1
    ) tg
    WHERE tg.rn = 1
),
BiggestLoss AS (
    SELECT tg.TeamID, CONCAT(tg.TeamScore, '-', tg.OppScore) AS BiggestLossScore
    FROM (
        SELECT TeamID, TeamScore, OppScore,
               ROW_NUMBER() OVER (PARTITION BY TeamID ORDER BY Diff ASC, GameDateTime DESC, GameID DESC) AS rn
        FROM TeamGames
        WHERE Won = 0
    ) tg
    WHERE tg.rn = 1
),
LastGame AS (
    SELECT x.TeamID, lg.GameDateTime AS LastGameDate, lg.HomeStadium AS LastGameStadium
    FROM (
        SELECT TeamID, GameID,
               ROW_NUMBER() OVER (PARTITION BY TeamID ORDER BY GameDateTime DESC, GameID DESC) AS rn
        FROM TeamGames 
    ) x
    JOIN TeamGames lg ON lg.TeamID = x.TeamID AND lg.GameID = x.GameID
    WHERE x.rn = 1
)
SELECT
    t.Name AS TeamName,
    t.Stadium AS TeamStadiumName,
    t.Logo AS TeamLogo,
    t.URL as TeamUrl,
    COALESCE(mp.SeasonMVP, '—') AS SeasonMVPOnTeam,
    a.GamesPlayed AS NumberOfGamesPlayed,
    a.GamesWon AS NumberOfGamesWon,
    a.GamesLost AS NumberOfGamesLost,
    a.GamesHome AS NumberOfGamesPlayedAtHome,
    a.GamesAway AS NumberOfGamesPlayedAway,
    bw.BiggestWinScore AS PointsInBiggestWin,
    bl.BiggestLossScore AS PointsInBiggestLoss,
    lg.LastGameStadium AS LastGameStadiumName,
    lg.LastGameDate AS LastGameDate
FROM dbo.Teams t
LEFT JOIN Agg a ON a.TeamID = t.TeamID
LEFT JOIN MvpPick mp ON mp.TeamID = t.TeamID
LEFT JOIN BiggestWin bw ON bw.TeamID = t.TeamID
LEFT JOIN BiggestLoss bl ON bl.TeamID = t.TeamID
LEFT JOIN LastGame lg ON lg.TeamID = t.TeamID;
GO
