using Newtonsoft.Json;

namespace SiteParse;

public class ResultWriter
{
    private const string BaseDirectory = "DataParse";

    /// <summary>
    /// Saves the parsed product information and average price for a specific website.
    /// </summary>
    /// <param name="products">The list of products to save.</param>
    /// <param name="averagePrice">The calculated average price.</param>
    /// <param name="siteName">The name of the website (used for folder and file naming).</param>
    public void SaveWebsiteData(List<ProductInfo> products, double averagePrice, string siteName)
    {
        // Create the base directory if it doesn't exist
        Directory.CreateDirectory(BaseDirectory);

        // Create a subdirectory for the specific website
        string siteDirectory = Path.Combine(BaseDirectory, siteName);
        Directory.CreateDirectory(siteDirectory);

        // Save product data to {siteName}.txt
        string productsFilePath = Path.Combine(siteDirectory, $"{siteName}.txt");
        SaveProductsToFile(products, productsFilePath);

        // Save product data to {siteName}.json
        string productsJsonPath = Path.Combine(siteDirectory, $"{siteName}.json");
        SaveProductsToJson(products, productsJsonPath);

        // Save average price to average_price.txt
        string averagePriceFilePath = Path.Combine(siteDirectory, "average_price.txt");
        SaveAveragePriceToFile(averagePrice, averagePriceFilePath);

        Console.WriteLine($"Данные для сайта '{siteName}' успешно сохранены в папку '{siteDirectory}'.");
    }

    /// <summary>
    /// Saves the parsed product information to a text file.
    /// </summary>
    /// <param name="products">The list of products to save.</param>
    /// <param name="filePath">The path to the output file.</param>
    private void SaveProductsToFile(List<ProductInfo> products, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        foreach (var product in products)
        {
            writer.WriteLine(product.ToString());
            writer.WriteLine("__________________________");
        }
    }

    /// <summary>
    /// Saves the parsed product information to a JSON file using Newtonsoft.Json.
    /// </summary>
    /// <param name="products">The list of products to save.</param>
    /// <param name="filePath">The path to the output JSON file.</param>
    private void SaveProductsToJson(List<ProductInfo> products, string filePath)
    {
        string json = JsonConvert.SerializeObject(products, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Saves the average price to a text file.
    /// </summary>
    /// <param name="averagePrice">The calculated average price.</param>
    /// <param name="filePath">The path to the output file.</param>
    private void SaveAveragePriceToFile(double averagePrice, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine($"Средняя цена: {averagePrice}");
    }
}