using System.Net.Http.Json;
using AstroPlanner.Models;

namespace AstroPlanner.Services;

public static class DataProvider
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

    public static async Task<(string placeName, string latitude, string longitude)> GetLocationInfo(string zipCode)
    {
        try
        {
            HttpClient http = new();
            LocationInfo? result = await http.GetFromJsonAsync<LocationInfo>($"https://api.zippopotam.us/us/{zipCode}");

            return (result is not null && result.Places is not null)
                ? (result.Places[0].PlaceName ?? "", result.Places[0].Latitude ?? "", result.Places[0].Longitude ?? "")
                : ("", "", "");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return ("", "", "");
        }
    }
}
