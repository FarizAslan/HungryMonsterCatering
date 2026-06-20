using HungryMonsterCatering.Models;

namespace HungryMonsterCatering.Services;

/// <summary>
/// In-memory storage for all data entered by the user during the program's run:
/// the list of known business partners (contractors) and the yearly meal records.
/// </summary>
public class CateringDataStore
{
    /// <summary>List of all known business partner (contractor) names.</summary>
    public List<string> Contractors { get; } = new();

    /// <summary>One entry per year for which data has been entered.</summary>
    public List<YearlyRecord> Records { get; } = new();

    public bool HasContractors => Contractors.Count > 0;
    public bool HasRecords => Records.Count > 0;

    /// <summary>
    /// Adds a new contractor name if it is valid and not already present.
    /// Returns false if the name is empty or a duplicate (case-insensitive).
    /// </summary>
    public bool AddContractor(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;

        bool alreadyExists = Contractors.Any(c => c.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (alreadyExists)
            return false;

        Contractors.Add(name);
        return true;
    }

    /// <summary>
    /// Returns the existing YearlyRecord for the given year, or creates and
    /// registers a new one if it does not exist yet.
    /// </summary>
    public YearlyRecord GetOrCreateRecord(int year)
    {
        var existing = Records.FirstOrDefault(r => r.Year == year);
        if (existing is not null)
            return existing;

        var created = new YearlyRecord(year);
        Records.Add(created);
        return created;
    }

    /// <summary>
    /// Stores the number of meals served to a given contractor in a given year.
    /// </summary>
    public void SetMeals(int year, string contractor, int meals)
    {
        var record = GetOrCreateRecord(year);
        record.MealsByContractor[contractor] = meals;
    }

    /// <summary>Returns all stored years in ascending order.</summary>
    public IEnumerable<int> GetSortedYears()
    {
        return Records.Select(r => r.Year).OrderBy(y => y);
    }
}
