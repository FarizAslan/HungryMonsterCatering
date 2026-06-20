using HungryMonsterCatering.Dtos;
using HungryMonsterCatering.Services;

var builder = WebApplication.CreateBuilder(args);

// Make the URL predictable so the instructions / screenshots always match.
builder.WebHost.UseUrls("http://localhost:5000");

// The data store keeps everything in memory for the lifetime of the running
// application (no database is used — see Documentation.md for the reasoning).
builder.Services.AddSingleton<CateringDataStore>();

var app = builder.Build();

// Serves the HTML/CSS/JS frontend located in wwwroot/ (index.html, css/, js/).
app.UseDefaultFiles();
app.UseStaticFiles();

// ------------------------------------------------------------------
// API endpoints
// ------------------------------------------------------------------

// GET /api/contractors -> list of registered business partner names
app.MapGet("/api/contractors", (CateringDataStore store) =>
{
    return Results.Ok(store.Contractors);
});

// POST /api/contractors -> register a new business partner
app.MapPost("/api/contractors", (AddContractorRequest request, CateringDataStore store) =>
{
    bool added = store.AddContractor(request.Name ?? string.Empty);
    if (!added)
    {
        return Results.Conflict(new ApiError("Partner name is empty or already exists."));
    }
    return Results.Ok(store.Contractors);
});

// GET /api/data -> full table: every year x every contractor + active count per year
app.MapGet("/api/data", (CateringDataStore store) =>
{
    var rows = store.GetSortedYears()
        .Select(year =>
        {
            var record = store.Records.First(r => r.Year == year);
            var meals = store.Contractors.ToDictionary(
                c => c,
                c => record.MealsByContractor.TryGetValue(c, out int value) ? value : 0);
            return new YearlyRowDto(year, meals, record.CountActiveContractors());
        })
        .ToList();

    return Results.Ok(new DataTableResponse(store.Contractors, rows));
});

// POST /api/data -> save the meal counts for a given year
app.MapPost("/api/data", (YearlyDataRequest request, CateringDataStore store) =>
{
    if (!store.HasContractors)
    {
        return Results.BadRequest(new ApiError("Add at least one business partner before entering yearly data."));
    }

    if (request.Year < 2000 || request.Year > 2100)
    {
        return Results.BadRequest(new ApiError("Year must be between 2000 and 2100."));
    }

    foreach (string contractor in store.Contractors)
    {
        int meals = request.Meals.TryGetValue(contractor, out int value) ? value : 0;
        if (meals < 0)
        {
            return Results.BadRequest(new ApiError($"Meal count for \"{contractor}\" cannot be negative."));
        }
        store.SetMeals(request.Year, contractor, meals);
    }

    return Results.Ok();
});

// GET /api/analysis -> the year(s) with the highest number of active partners
app.MapGet("/api/analysis", (CateringDataStore store) =>
{
    if (!store.HasRecords)
    {
        return Results.BadRequest(new ApiError("No data has been entered yet."));
    }

    var result = AnalysisService.FindYearsWithMostActiveContractors(store.Records);
    return Results.Ok(new AnalysisResponse(result.Years, result.ActiveContractorCount));
});

app.Run();
