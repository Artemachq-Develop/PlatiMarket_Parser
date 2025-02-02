namespace SiteParse;

public class PriceFilter
{
    /// <summary>
    /// Filters out prices that are significantly lower than the majority.
    /// </summary>
    /// <param name="prices">The list of prices to filter.</param>
    /// <returns>A filtered list of prices without outliers.</returns>
    public static List<double> FilterPrices(List<double> prices)
    {
        if (prices == null || prices.Count == 0)
            return new List<double>();

        var sortedPrices = prices.OrderBy(p => p).ToList();

        double q1 = GetPercentile(sortedPrices, 40); // Первый квартиль (25%)
        double q3 = GetPercentile(sortedPrices, 60); // Третий квартиль (75%)

        double iqr = q3 - q1;

        //double median = GetPercentile(prices, 50);
        //double lowerBound = median - 1.5 * iqr;
        double lowerBound = Math.Max(0, q1 - 1.6 * iqr);

        Console.WriteLine($"Низкий порог: {lowerBound}");

        return sortedPrices.Where(p => p >= lowerBound).ToList();
    }

    /// <summary>
    /// Calculates the percentile value for a sorted list.
    /// </summary>
    /// <param name="sortedList">The sorted list of values.</param>
    /// <param name="percentile">The desired percentile (e.g., 25 for Q1).</param>
    /// <returns>The value at the specified percentile.</returns>
    static double GetPercentile(List<double> sortedList, double percentile)
    {
        int n = sortedList.Count;
        double index = (percentile / 100) * (n - 1);
        int lowerIndex = (int)Math.Floor(index);
        Console.WriteLine($"lowerIndex - {lowerIndex}");
        int upperIndex = (int)Math.Ceiling(index);
        Console.WriteLine($"upperIndex - {upperIndex}");

        if (lowerIndex == upperIndex)
        {
            return sortedList[lowerIndex];
        }

        double weight = index - lowerIndex;
        return sortedList[lowerIndex] * (1 - weight) + sortedList[upperIndex] * weight;
    }
}