using System.Net;
using Newtonsoft.Json.Linq;

namespace SiteParse;

public static class RUBExchange
{
    private static readonly Lazy<double> _exchangeResult = new Lazy<double>(() => Exchange());

    public static double Dollar { get => _exchangeResult.Value; }

    private static double Exchange()
    {
        return JObject.Parse(new WebClient().DownloadString("https://www.cbr-xml-daily.ru/daily_json.js"))["Valute"]["USD"]["Value"].ToObject<double>();
    }
}