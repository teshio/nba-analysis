using Nba.Api.DataAccess;

public interface IDataRepository
{
    Task<IEnumerable<TeamSummaryDto>> GetTeamSummariesAsync();

    Task<IEnumerable<GameData>> GetAllGameDataAsync();
}
