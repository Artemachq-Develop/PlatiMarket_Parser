using System.Text.RegularExpressions;

namespace SiteParse;

public static class ParseTags
{
    /// <summary>
    /// Starts the parsing process for the specified URL, extracts product information,
    /// and calculates the average price of products that meet the criteria.
    /// </summary>
    public static async Task Start()
    {
        string[] urls =
        {
            "https://plati.market/games/a-way-out/979/?lang=ru-RU#p2577=1&r2577=10&s2577=price",
            "https://plati.market/games/helldivers-2/1232/?lang=ru-RU#p6343=1&r6343=10&s6343=price" // Пример второй ссылки
        };

        var fileSaver = new ResultWriter();

        foreach (var url in urls)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url);

            var headerElements = document.QuerySelectorAll("h2.games-header");
            
            bool tagsWasFound = false;
            double middlePrice = 0;
            int counter = 0;
            
            var products = new List<ProductInfo>();

            foreach (var headerElement in headerElements)
            {
                if (IsSteamHeader(headerElement))
                {
                    tagsWasFound = true;
                    
                    #if DEBUG
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Заголовок содержит {headerElement.TextContent}. Продолжаем парсинг...");
                        Console.ResetColor();
                    #endif

                    var orangeFontElements = document.QuerySelectorAll("tr.orange_font, tr.colored_row");
                    foreach (var element in orangeFontElements)
                    {
                        if (TryParseProduct(element, out var productInfo))
                        {
                            if (productInfo is { WasSold: > 10 })
                            {
                                products.Add(productInfo);
                                
                                PrintProductInfo(productInfo, counter);
                                
                                middlePrice += productInfo.Price;
                                counter++;
                            }
                        }
                    }
                }
            }

            if (counter > 0)
            {
                double averagePrice = middlePrice / counter;

                string siteName = GetGameNameFromUrl(url);

                // Save the data using FileSaver
                fileSaver.SaveWebsiteData(products, averagePrice, siteName);
            }

            if (!tagsWasFound)
            {
                Console.WriteLine("Ни одного тэга не было найдено");
            }
        }
    }
    
    /// <summary>
    /// Extracts the game name from the URL using a regular expression.
    /// </summary>
    /// <param name="url">The URL to parse.</param>
    /// <returns>The extracted game name or "Unknown" if not found.</returns>
    public static string GetGameNameFromUrl(string url)
    {
        // Регулярное выражение для поиска названия игры между "/games/" и следующим "/"
        var match = Regex.Match(url, @"/games/([^/]+)/");
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value; // Возвращаем первую группу захвата
        }

        return "Unknown"; // Если не найдено
    }

    /// <summary>
    /// Checks if the given header element contains the word "Steam".
    /// </summary>
    /// <param name="headerElement">The header element to check.</param>
    /// <returns>True if the header contains "Steam"; otherwise, false.</returns>
    private static bool IsSteamHeader(IElement? headerElement)
    {
        return headerElement != null && headerElement.TextContent.Contains("Steam");
    }

    /// <summary>
    /// Attempts to parse product information from the given HTML element.
    /// </summary>
    /// <param name="element">The HTML element containing product details.</param>
    /// <param name="productInfo">The parsed product information if successful.</param>
    /// <returns>True if the product was successfully parsed; otherwise, false.</returns>
    private static bool TryParseProduct(IElement element, out ProductInfo? productInfo)
    {
        productInfo = null;
        
        var productTitleElement = element.QuerySelector("td.product-title");
        var productPrice = element.QuerySelector("td.product-price");
        var productSold = element.QuerySelector("td.product-sold");
        
        if (productTitleElement != null && productPrice != null && productSold != null)
        {
            var linkElement = productTitleElement.QuerySelector("a");
            
            if (linkElement != null && linkElement.HasAttribute("href"))
            {
                var link = linkElement.GetAttribute("href");
                var name = linkElement.TextContent.Trim();
                var price = double.Parse(productPrice.TextContent.Replace("$", "").Replace(".", ",").Trim());
                var convertedPrice = Math.Round(price * RUBExchange.Dollar, 2);
                var numberString = new string(productSold.TextContent.Where(char.IsDigit).ToArray());
                
                if (int.TryParse(numberString, out int soldCount))
                {
                    if (link != null)
                    {
                        productInfo = new ProductInfo(name, convertedPrice, link, soldCount);
                        return true;
                    }
                    else
                        return false;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Prints detailed information about a product to the console.
    /// </summary>
    /// <param name="productInfo">The product information to display.</param>
    /// <param name="counter">The index of the product in the list.</param>
    private static void PrintProductInfo(ProductInfo productInfo, int counter)
    {
        Console.WriteLine("__________________________");
        Console.WriteLine($"№{counter}");
        Console.WriteLine($"Название: {productInfo.Name}");
        Console.WriteLine($"Ссылка: {productInfo.Link}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Цена: {productInfo.Price} рублей");
        Console.ResetColor();
        Console.WriteLine($"Было продано: {productInfo.WasSold}");
        Console.WriteLine("__________________________");
    }
}