namespace SiteParse;

public static class ParseTags
{
    public static async Task Start()
    {
        double dollarExchange = RUBExchange.GetDollarExchange();
        
        string url = "https://plati.market/games/red-dead-redemption-2/854/" + "?lang=ru-RU#p2577=1&r2577=10&s2577=price";

        double middlePrice = 0;
        
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        var document = await context.OpenAsync(url);

        var headerElements = document.QuerySelectorAll("h2.games-header");

        List<ProductInfo> products = new List<ProductInfo>();

        bool tagsWasFound = false;
        
        int counter = 0;
        
        foreach (var headerElement in headerElements)
        {
            if (headerElement != null && (headerElement.TextContent.Contains("Steam")))
            {
                tagsWasFound = true;

                Console.WriteLine($"Заголовок содержит {headerElement.TextContent}. Продолжаем парсинг...");

                var orangeFontElements = document.QuerySelectorAll("tr.orange_font, tr.colored_row");

                foreach (var element in orangeFontElements)
                {
                    var productTitleElement = element.QuerySelector("td.product-title");
                    var productPrice = element.QuerySelector("td.product-price");
                    var productSold = element.QuerySelector("td.product-sold");

                    if (productTitleElement != null && productPrice != null && productSold != null)
                    {
                        var linkElement = productTitleElement.QuerySelector("a");

                        if (linkElement != null && linkElement.HasAttribute("href"))
                        {
                            string href = linkElement.GetAttribute("href");
                            string title = linkElement.TextContent.Trim();

                            double price =
                                double.Parse(productPrice.TextContent.Replace("$", "").Replace(".", ",").Trim());

                            /*products.Add(new ProductInfo(title, Math.Round(price * dollarExchange, 2),
                                $"https://plati.market/{href}", headerElement.TextContent));*/
                            
                            string numberString = new string(productSold.TextContent.Where(char.IsDigit).ToArray());

                            if (int.TryParse(numberString, out int number))
                            {
                                if (number > 10)
                                {
                                    Console.WriteLine("__________________________");
                                    Console.WriteLine($"№{counter}");
                                    Console.WriteLine($"Название: {title}");
                                    Console.WriteLine($"Ссылка: https://plati.market/{href}");
                                    Console.WriteLine($"Цена: {Math.Round(price * dollarExchange, 2)}");
                                    Console.WriteLine($"Было продано: {number}");
                                    Console.WriteLine("__________________________");
                                    
                                    middlePrice += Math.Round(price * dollarExchange, 2);

                                    counter++;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Элемент <a href> не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Блок <td class='product-title'> не найден.");
                    }
                }
            }
        }

        Console.WriteLine($"Средняя цена: {middlePrice / counter}");
        
        if(!tagsWasFound) Console.WriteLine("Ни одного тэга не было найдено");


        double min = double.MaxValue;
        int minIndex = 0;

        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].Price < min)
            {
                min = products[i].Price;
                minIndex = i;
            }
        }

        Console.WriteLine($"Самый дешевый продукт:\n{products[minIndex]}");
    }
}