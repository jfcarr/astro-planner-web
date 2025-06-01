using System.Net.Http.Json;
using AstroPlanner.Util.Models;

namespace AstroPlanner.Util.Services;

public class DataProvider
{
    public static Star[]? stars;

    public static async Task InitStarsDb(string baseAddress)
    {
        try
        {
            HttpClient http = new();
            stars = await http.GetFromJsonAsync<Star[]>($"{baseAddress}astro-data/stars.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
