namespace SiteParse;

public static class ParseTags
{
    public static async Task Start()
    {
        double dollarExchange = RUBExchange.GetDollarExchange();
        
        // URL сайта, который нужно спарсить
        string url = "https://plati.market/games/red-dead-redemption-2/854/";

        // Создаем конфигурацию для AngleSharp
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        // Загружаем страницу
        var document = await context.OpenAsync(url);

        // Находим элемент h2 с классом games-header
        var headerElement = document.QuerySelector("h2.games-header");

        // Проверяем, содержит ли текст "Steam Gift"
        if (headerElement != null && headerElement.TextContent.Contains("Steam Gift"))
        {
            Console.WriteLine("Заголовок содержит 'Steam Gift'. Продолжаем парсинг...");

            // Находим все элементы с классом orange_font
            var orangeFontElements = document.QuerySelectorAll("tr.orange_font");

            // Извлекаем контент из каждого элемента
            foreach (var element in orangeFontElements)
            {
                // Ищем внутри текущего элемента блок <td class="product-title">
                var productTitleElement = element.QuerySelector("td.product-title");
                var productPrice = element.QuerySelector("td.product-price");

                if (productTitleElement != null && productPrice != null)
                {
                    // Ищем внутри <td> элемент <a href>
                    var linkElement = productTitleElement.QuerySelector("a");

                    if (linkElement != null && linkElement.HasAttribute("href"))
                    {
                        // Получаем значение атрибута href
                        string href = linkElement.GetAttribute("href");
                        string title = linkElement.TextContent.Trim();

                        double price = double.Parse(productPrice.TextContent.Replace("$", "").Replace(".", ",").Trim());

                        Console.WriteLine($"Название: {title}");
                        Console.WriteLine($"Ссылка: https://plati.market/{href}");
                        Console.WriteLine($"Цена: {Math.Round(price * dollarExchange, 2)}");
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
        else
        {
            Console.WriteLine("Заголовок не содержит 'Steam Gift'. Парсинг прекращен.");
        }
    }
}