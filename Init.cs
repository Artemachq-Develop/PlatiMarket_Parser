global using AngleSharp;
global using AngleSharp.Dom;

namespace SiteParse;

class Init
{
    static async Task Main()
    {
        await ParseTags.Start();
    }
}