namespace HungryMonsterCatering.Models;

/// <summary>
/// Represents all catering data for a single calendar year.
/// For every business partner (contractor) it stores how many meals
/// Hungry Monster served to that partner's employees during that year.
/// A value of 0 means the company did not work with that partner in this year.
/// </summary>
public class YearlyRecord
{
    public int Year { get; }

    /// <summary>
    /// Key   = contractor name
    /// Value = number of meals served to that contractor's employees in this year
    /// </summary>
    public Dictionary<string, int> MealsByContractor { get; } = new();

    public YearlyRecord(int year)
    {
        Year = year;
    }

    /// <summary>
    /// Returns how many distinct contractors used the catering service
    /// at least once during this year (meals &gt; 0).
    /// </summary>
    public int CountActiveContractors()
    {
        return MealsByContractor.Values.Count(meals => meals > 0);
    }

    /// <summary>
    /// Returns the total number of meals served across all contractors in this year.
    /// </summary>
    public int TotalMeals()
    {
        return MealsByContractor.Values.Sum();
    }
}
