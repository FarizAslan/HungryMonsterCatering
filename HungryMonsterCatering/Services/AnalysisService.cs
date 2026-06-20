using HungryMonsterCatering.Models;

namespace HungryMonsterCatering.Services;

/// <summary>
/// Result of the "most active year" analysis.
/// There can be more than one year tied for the highest number of active contractors,
/// so Years is a list rather than a single value.
/// </summary>
public record AnalysisResult(List<int> Years, int ActiveContractorCount);

/// <summary>
/// Contains the business logic that answers the client's main question:
/// "In which year did more companies use the catering service at least once?"
/// </summary>
public static class AnalysisService
{
    /// <summary>
    /// Scans every yearly record, counts how many contractors had at least
    /// one meal (value &gt; 0) in that year, and returns the year(s) with the
    /// highest such count. If several years are tied for first place, all of
    /// them are returned.
    /// </summary>
    public static AnalysisResult FindYearsWithMostActiveContractors(List<YearlyRecord> records)
    {
        if (records.Count == 0)
            return new AnalysisResult(new List<int>(), 0);

        int maxActive = records.Max(r => r.CountActiveContractors());

        var bestYears = records
            .Where(r => r.CountActiveContractors() == maxActive)
            .Select(r => r.Year)
            .OrderBy(y => y)
            .ToList();

        return new AnalysisResult(bestYears, maxActive);
    }
}
