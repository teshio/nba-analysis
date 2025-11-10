
using Dapper;
using System.Data;

namespace Nba.Api.DataAccess
{
    public class DataRepository(IDbConnection connection) : IDataRepository
    {
        public async Task<IEnumerable<TeamSummaryDto>> GetTeamSummariesAsync() => 
            await connection.QueryAsync<TeamSummaryDto>("sp_GetTeamSummary");

        public async Task<IEnumerable<GameData>> GetAllGameDataAsync() =>
            await connection.QueryAsync<GameData>("sp_GetAllGames");
    }
}
