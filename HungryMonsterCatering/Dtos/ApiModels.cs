namespace HungryMonsterCatering.Dtos;

/// <summary>Request body for POST /api/contractors</summary>
public record AddContractorRequest(string Name);

/// <summary>Request body for POST /api/data — saves meal counts for one year.</summary>
public record YearlyDataRequest(int Year, Dictionary<string, int> Meals);

/// <summary>One row of the data table returned by GET /api/data</summary>
public record YearlyRowDto(int Year, Dictionary<string, int> Meals, int ActiveContractors);

/// <summary>Full payload returned by GET /api/data</summary>
public record DataTableResponse(List<string> Contractors, List<YearlyRowDto> Rows);

/// <summary>Payload returned by GET /api/analysis</summary>
public record AnalysisResponse(List<int> Years, int ActiveContractorCount);

/// <summary>Generic simple error message returned by the API.</summary>
public record ApiError(string Message);
