import React, { useEffect, useState, useMemo } from 'react';
import { api, LeagueAnalysis, TeamSummary } from '../lib/api';

export default function App() {
    const [rows, setRows] = useState<TeamSummary[] | null>(null);

    // analysis per team
    const [analysis, setAnalysis] = useState<Record<string, string>>({});
    const [loading, setLoading] = useState<Record<string, boolean>>({});
    const [error, setError] = useState<string | null>(null);

    // analysis all
    const [allLoading, setAllLoading] = useState(false);
    const [allError, setAllError] = useState<string | null>(null);
    const [league, setLeague] = useState<LeagueAnalysis | null>(null);


    useEffect(() => {
        let m = true;
        (async () => {
            try {
                const [rows] = await Promise.all([api.getSummary()]);
                if (!m) return;
                setRows(rows);
            } catch (e: any) { setError(String(e?.message || e)) }
        })();
        return () => { m = false }
    }, []);

    const handleAnalyse = async (name: string | null) => {
        if (!name) return;
        setLoading(s => ({ ...s, [name]: true }));
        try {
            const res = await api.analyse(name);
            setAnalysis(s => ({ ...s, [name]: res.analysis }))
        } catch (e: any) { setAnalysis(s => ({ ...s, [name]: String(e?.message || e) })) } finally { setLoading(s => ({ ...s, [name]: false })) }
    };

    const handleAnalyseAll = async () => {
        setAllLoading(true)

        try {
            const res = await api.analyseAll(); // { analysis: string | LeagueAnalysis }
            const a = (res as any).analysis;


            let obj: LeagueAnalysis | null = null;
            if (typeof a === 'string') {
                try {
                    obj = JSON.parse(a) as LeagueAnalysis;
                } catch {
                    /* fall through */
                }
            } else if (a && typeof a === 'object') {
                obj = a as LeagueAnalysis;
            }
            if (!obj) throw new Error('AI did not return valid JSON for league analysis.');

            setLeague(obj);

        } catch (e: any) {

            setAllError(String(e?.message || e))

        } finally {

            setAllLoading(false);
        }
    };

    const count = useMemo(() => rows?.length ?? 0, [rows]);

    return (<div className='p-8 max-w-[1200px] mx-auto'>
        <h1 className='text-2xl font-semibold mb-2'>NBA Teams Summary</h1>
        <div className='text-sm opacity-70 mb-4'><div>Rows loaded: {count}</div>
            {(import.meta as any).env.VITE_API_BASE && <div>API: {(import.meta as any).env.VITE_API_BASE}</div>}</div>
        {error && <div className='p-3 rounded bg-red-100 text-red-800 mb-4'>{error}</div>}
        {!rows ? (<div>Loading…</div>) : (
            <div className='overflow-x-auto rounded-2xl shadow'>
                <table className='min-w-full text-sm'>
                    <thead className='bg-gray-100'><tr>
                        <th className='p-3 text-left '>Team</th>
                        <th className='p-3'>Stadium</th>
                        <th className='p-3'>MVP</th>
                        <th className='p-3'>Played</th>
                        <th className='p-3'>Won</th>
                        <th className='p-3'>Lost</th>
                        <th className='p-3'>Played Home</th>
                        <th className='p-3'>PlayedAway</th>
                        <th className='p-3'>Biggest Win</th>
                        <th className='p-3'>Biggest Loss</th>
                        <th className='p-3'>Last Stadium</th>
                        <th className='p-3'>Last Date</th>
                        <th className='p-3'>AI</th></tr>
                    </thead>
                    <tbody>{rows.map((r, i) => {
                        const key = `${r.teamName ?? ''}|${r.teamStadiumName ?? ''}|${i}`;
                        return (
                            <>
                                <tr key={key} className='odd:bg-white even:bg-gray-50 align-top'>
                                    <td className='p-3'>
                                        <div className='flex items-center gap-2'>{r.teamLogo ? <img src={r.teamLogo} alt={r.teamLogo} className='h-9 w-10 rounded' /> : null}
                                        </div>
                                        <a target="_blank" href={r.teamUrl} class="text-blue-600 hover:text-blue-800 underline decoration-dotted whitespace-nowrap pr-2">
                                            {r.teamName} ↗
                                        </a>
                                    </td>
                                    <td className='p-3'>{r.teamStadiumName}</td>
                                    <td className='p-3'>{r.seasonMVPOnTeam}</td>
                                    <td className='p-3 text-center'>{r.numberOfGamesPlayed}</td>
                                    <td className='p-3 text-center'>{r.numberOfGamesWon}</td>
                                    <td className='p-3 text-center'>{r.numberOfGamesLost}</td>
                                    <td className='p-3 text-center'>{r.numberOfGamesPlayedAtHome}</td>
                                    <td className='p-3 text-center'>{r.numberOfGamesPlayedAway}</td>
                                    <td className='p-3'>{r.pointsInBiggestWin}</td>
                                    <td className='p-3'>{r.pointsInBiggestLoss}</td>
                                    <td className='p-3'>{r.lastGameStadiumName}</td>
                                    <td className='p-3'>{r.lastGameDate ? new Date(r.lastGameDate).toISOString().slice(0, 10) : ''}</td>
                                    <td className='p-3'>
                                        <button className='px-3 py-1 rounded-2xl border hover:shadow disabled:opacity-60'
                                            onClick={() => handleAnalyse(r.teamName)} disabled={!!loading[r.teamName ?? '']}>
                                            {loading[r.teamName ?? ''] ? 'Analysing…' : 'Analyse'}
                                        </button>
                                    </td>
                                </tr>
                                {analysis[r.teamName ?? ''] &&
                                    <tr>
                                        <td colspan='13'>
                                            <div className='m-3 text-xs'>{analysis[r.teamName ?? '']}</div>
                                        </td>
                                    </tr>}
                            </>)
                    })}</tbody>
                </table>

                <div className="flex items-center gap-3 mb-4 p-5">
                    <button
                        className="px-3 py-1 rounded-2xl border hover:shadow disabled:opacity-60"
                        onClick={handleAnalyseAll}
                        disabled={allLoading}>
                        {allLoading ? 'Analysing All…' : 'Analyse All (OpenAI)'}
                    </button>
                    {allError && <span className="text-sm text-red-600">{allError}</span>}
                </div>

                {league && (
                    <div className="rounded-xl border bg-gray-50 p-4 m-5">
                        <h2 className="font-bold mb-3 text">AI Analysis</h2>

                        <div className="grid gap-4 md:grid-cols-3">
                            <div>
                                <h3 className="font-medium mb-2 text">Top Teams</h3>
                                <ol className="list-decimal list-inside text-sm space-y-1">
                                    {league.topTeams.map(t => (
                                        <li key={t.name}>
                                            {t.name} - {(t.winPct * 100).toFixed(1)}%
                                        </li>
                                    ))}
                                </ol>
                            </div>

                            <div>
                                <h3 className="font-medium mb-2 text">Anomalies</h3>
                                <ul className="list-disc list-inside text-sm space-y-1">
                                    {league.anomalies.map((s, i) => <li key={i}>{s}</li>)}
                                </ul>
                            </div>

                            <div>
                                <h3 className="font-medium mb-2 text">Insights</h3>
                                <ul className="list-disc list-inside text-sm space-y-1">
                                    {league.insights.map((s, i) => <li key={i}>{s}</li>)}
                                </ul>
                            </div>
                        </div>

                        <details className="mt-4">
                            <summary className="cursor-pointer text-xs opacity-70">Show raw JSON</summary>
                            <pre className="mt-2 text-xs overflow-auto">
                                {JSON.stringify(league, null, 2)}
                            </pre>
                        </details>
                    </div>
                )}
            </div>)}
    </div>)
}
