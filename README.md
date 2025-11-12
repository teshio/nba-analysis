# NBA Teams Summary with AI Analysis — React (Vite + Tyepscript) + .NET 9 Minimal API + SQL Views/Procs + TailwindCSS

React front-end talks to a minimal ASP.NET Core API that reads from `dbo.vw_TeamSummary`. Provides several different analysis function against the data:
* Per team analysis (via OpenAI)
* Entire league analysis (via OpenAI)
* Score prediction (via ML.NET)

<img width="2500" height="2076" alt="preview" src="https://github.com/user-attachments/assets/2200b70d-ce2f-49d2-a01a-edf4c0c8b720" />

## Quick start

### 1) Database
- Run the seed to create/populate `NBA`
- Execute `sql/Create_vw_TeamSummary.sql`
- Execute `sql/Create_sp_GetTeamSummary.sql`
- Execute `sql/Create_sp_GetAllGames.sql`

### 2) Config
-- Open `/api/src/appsettings.json`
-- Check and update `ConnectionString.DefaultConnection` with a connection string to the NBA data that was previously seeded
-- Update `OpenAI.ApiKey` with an OpenAI api key

### 3) Run API
```bash
cd api/src
dotnet restore
dotnet run
```
Default ports: `https://localhost:7088` / `http://localhost:5088`. Configure `appsettings.json`.

### 4) Run Frontend
```bash
cd frontend
npm i
$env:VITE_PROXY_TARGET="https://localhost:61677"; npm run dev
```
Open http://localhost:5173

Endpoints: `/api/teams/summary`, `/api/ai/analyse`, `/api/diag/db-counts`.

## Environment

Hosted on Azure [here](https://agreeable-glacier-076d9fb03.3.azurestaticapps.net)


## Dev proxy (no env needed)
The Vite server proxies `/api/*` to your backend automatically.
- Default target: `https://localhost:7088`
- Override: `VITE_PROXY_TARGET=http://localhost:5088 npm run dev`

With the proxy enabled, you can remove `.env.local` (no `VITE_API_BASE` required).


## Future Enhancements

- API Authorisation
- Load limiting (checking IP address/rate limiting)
- Introduce multiple threads for the ML.NET section to improve performance
- Improve the prediction model by passing more stats regarding a team so a stronger prediction can be made
