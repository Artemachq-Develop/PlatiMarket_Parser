global using AngleSharp;
global using AngleSharp.Dom;
global using AngleSharp.Html.Dom;

namespace SiteParse;

class Init
{
    static async Task Main()
    {
        await ParseTags.Start();

        /*var address = "https://plati.market/games/red-dead-redemption-2/854/";
        var products = await ScrapeProductInfo(address);

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

        Console.WriteLine($"Самый дешевый продукт:\n{products[minIndex].ToString()}");*/
    }

    static async Task<List<ProductInfo>> ScrapeProductInfo(string url)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(url);

        string cellSelector = "tr.orange_font";
        string cellName = cellSelector + " td:nth-child(3) a";
        string cellPrice = cellSelector + " td:nth-child(6)";

        var names = document.QuerySelectorAll(cellName);
        var prices = document.QuerySelectorAll(cellPrice);

        return ParseProducts(names, prices);
    }

    static List<ProductInfo> ParseProducts(IHtmlCollection<IElement> names, IHtmlCollection<IElement> prices)
    {
        var products = new List<ProductInfo>();

        for (int i = 0; i < names.Length; i++)
        {
            try
            {
                if (names[i] is IHtmlAnchorElement anchor)
                {
                    if (i < prices.Length && prices[i] != null)
                    {
                        string convertedPrice = prices[i].TextContent.Replace("$", "").Replace(".", ",").Trim();
                        double price = double.Parse(convertedPrice);
                        products.Add(new ProductInfo(anchor.TextContent.Trim(), price, anchor.Href));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке продукта {i + 1}: {ex.Message}");
            }
        }

        return products;
    }
}

[Serializable]
struct ProductInfo
{
    public string Category;
    public string Name;
    public double Price;
    public string Link;

    public ProductInfo(string name, double price, string link, string category = null)
    {
        Category = category;
        Name = name;
        Price = price;
        Link = link;
    }

    public override string ToString()
    {
        return $"Name - {Name},\nPrice - {Price} $,\nLink - {Link}\n";
    }
}