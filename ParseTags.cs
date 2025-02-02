using System.Text.RegularExpressions;

namespace SiteParse;

public static class ParseTags
{
    private const string LinksFilePath = "links.txt";

    /// <summary>
    /// Starts the parsing process for the specified URL, extracts product information,
    /// and calculates the average price of products that meet the criteria.
    /// </summary>
    public static async Task Start()
    {
        List<string> urls = GetUrlsFromFile();

        if (urls.Count == 0)
        {
            Console.WriteLine("Файл links.txt пуст или не содержит корректных ссылок.");
            return;
        }

        var resultWriter = new ResultWriter();

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
                    /*Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Заголовок содержит {headerElement.TextContent}. Продолжаем парсинг...");
                    Console.ResetColor();*/
#endif

                    var orangeFontElements = document.QuerySelectorAll("tr.orange_font, tr.colored_row");
                    foreach (var element in orangeFontElements)
                    {
                        if (TryParseProduct(element, out var productInfo))
                        {
                            if (productInfo is { WasSold: > 10 })
                            {
                                products.Add(productInfo);

#if DEBUG
                                //PrintProductInfo(productInfo, counter);
#endif

                                middlePrice += productInfo.Price;
                                counter++;
                            }
                        }
                    }
                }
            }

            var prices = products
                .Where(p => p.Price > 0)
                .Select(p => p.Price)
                .ToList();

            var filteredPrices = PriceFilter.FilterPrices(prices);

            /*Console.WriteLine("Исходные цены:");
            foreach (var price in prices)
            {
                Console.Write($"{price} | ");
            }

            Console.WriteLine();
            
            Console.WriteLine("Отфильтрованные цены:");
            foreach (var price in filteredPrices)
            {
                Console.Write($"{price} | ");
            }

            Console.WriteLine();*/

            products = products
                .Where(p => filteredPrices.Contains(p.Price))
                .ToList();

            if (counter > 0)
            {
                double averagePrice = middlePrice / counter;

                string siteName = GetGameNameFromUrl(url);

                resultWriter.SaveWebsiteData(products, averagePrice, siteName);
            }
            
            var cheapestProduct = products.FirstOrDefault(p => p.Price == products.Min(x => x.Price));
            
            if (cheapestProduct != null)
            {
                Console.WriteLine($"Самый дешевый товар: {cheapestProduct.Name}, Цена: {cheapestProduct.Price}");
            }
            else
            {
                Console.WriteLine("Товары не найдены.");
            }

            if (!tagsWasFound)
            {
                Console.WriteLine("Ни одного тэга не было найдено");
            }
        }
    }

    /// <summary>
    /// Reads URLs from the links.txt file or creates the file if it doesn't exist.
    /// </summary>
    /// <returns>A list of valid URLs.</returns>
    private static List<string> GetUrlsFromFile()
    {
        if (!File.Exists(LinksFilePath))
        {
            Console.WriteLine("Файл links.txt не найден. Создаю новый файл...");
            File.Create(LinksFilePath).Close();
            Console.WriteLine("Файл links.txt создан. Пожалуйста, добавьте ссылки в файл (каждая ссылка на новой строке) и перезапустите программу.");
            Environment.Exit(0);
        }

        var lines = File.ReadAllLines(LinksFilePath);

        var urls = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToList();

        return urls;
    }
    
    /// <summary>
    /// Extracts the game name from the URL using a regular expression.
    /// </summary>
    /// <param name="url">The URL to parse.</param>
    /// <returns>The extracted game name or "Unknown" if not found.</returns>
    public static string GetGameNameFromUrl(string url)
    {
        var match = Regex.Match(url, @"/games/([^/]+)/");
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        return "Unknown";
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