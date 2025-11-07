# NBA Teams Summary — React (Vite + TS) + .NET 9 Minimal API + SQL View + Tailwind

React front end talks to a minimal ASP.NET Core API that reads from `dbo.vw_TeamSummary`.

## Quick start

### 1) Database
- Run the seed to create/populate `NBA`
- Execute `sql/Create_vw_TeamSummary.sql`
- Execute `sql/Create_sp_GetTeamSummary.sql`

### 2) API
```bash
cd api/src
dotnet restore
dotnet run
```
Default ports: `https://localhost:7088` / `http://localhost:5088`. Configure `appsettings.json`.

### 3) Frontend
```bash
cd frontend
npm i
$env:VITE_PROXY_TARGET="https://localhost:61677"; npm run dev
```
Open http://localhost:5173

Endpoints: `/api/teams/summary`, `/api/ai/analyse`, `/api/diag/db-counts`.


## Dev proxy (no env needed)
The Vite server proxies `/api/*` to your backend automatically.
- Default target: `https://localhost:7088`
- Override: `VITE_PROXY_TARGET=http://localhost:5088 npm run dev`

With the proxy enabled, you can remove `.env.local` (no `VITE_API_BASE` required).
