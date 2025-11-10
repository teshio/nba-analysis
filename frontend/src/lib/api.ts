export type TeamSummary = {
    teamId: number;
    teamName: string | null;
    teamStadiumName: string | null;
    teamLogo: string | null;
    seasonMVPOnTeam: string | null;
    numberOfGamesPlayed: number | null;
    numberOfGamesWon: number | null;
    numberOfGamesLost: number | null;
    numberOfGamesPlayedAtHome: number | null;
    numberOfGamesPlayedAway: number | null;
    pointsInBiggestWin: string | null;
    pointsInBiggestLoss: string | null;
    lastGameStadiumName: string | null;
    lastGameDate: string | null
};

export type LeagueAnalysis = {
    topTeams: { name: string; winPct: number }[];
    anomalies: string[];
    insights: string[];
};

export type AnalyseAllResponse = {
    analysis: string | LeagueAnalysis; // allow either
};

export type AnalyseResponse = { analysis: string };

export type PredictResponse = { predictionSummary: string };

const API_BASE = (import.meta as any).env.VITE_API_BASE?.replace(/\/$/, '') || '';

async function get<T>(p: string) {
    const r = await fetch(`${API_BASE}${p}`);
    if (!r.ok) throw new Error(await r.text());
    return r.json()
}

async function postJson<T>(p: string, b: any) {
    const r = await fetch(
        `${API_BASE}${p}`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(b)
        });

    if (!r.ok) throw new Error(await r.text());

    return r.json()
}

export const api = {
    getSummary: () => get<TeamSummary[]>(
        '/api/teams/summary'
    ),
    analyseAll: () => get<{ analysis: string }>(
        '/api/analyse-all'
    ),
    analyse: (teamName: string) => postJson<AnalyseResponse>(
        '/api/ai/analyse',
        {
            teamName
        }),
    predict: (teamHome: string, teamAway: string) => postJson<PredictResponse>(
        '/api/predict-next?homeTeam=' + teamHome +  '&awayTeam=' + teamAway,
        {        })
};

