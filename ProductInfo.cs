namespace SiteParse;

[Serializable]
public class ProductInfo
{
    public string? Category;
    public string Name;
    public double Price;
    public int WasSold;
    public string Link;

    public ProductInfo(string name, double price, string link, int wasSold, string? category = null)
    {
        Category = category;
        Name = name;
        Price = price;
        Link = "https://plati.market" + link;
        WasSold = wasSold;
    }

    public override string ToString()
    {
        return $"Name - {Name},\nPrice - {Price} руб.,\nLink - {Link}\nWas sold - {WasSold}\nCategory - {Category}";
    }
    
    public static implicit operator string(ProductInfo product)
    {
        return product.ToString();
    }
}