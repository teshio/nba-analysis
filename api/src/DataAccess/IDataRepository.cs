public interface IDataRepository
{
    Task<IEnumerable<TeamSummaryDto>> GetTeamSummariesAsync();

    Task<TeamSummaryDto?> GetTeamSummaryByNameAsync(string teamName);
}
