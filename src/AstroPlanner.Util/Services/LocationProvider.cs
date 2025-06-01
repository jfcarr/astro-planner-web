using System.Net.Http.Json;
using AstroPlanner.Util.Models;
using GeoTimeZone;

namespace AstroPlanner.Util.Services;

public static class LocationProvider
{
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

    public static async Task<bool> GetCoordinates(string? zipCode)
    {
        try
        {
            (string placeName, string latitude, string longitude) = await LocationProvider.GetLocationInfo(zipCode ?? "");

            string tz = TimeZoneLookup.GetTimeZone(Convert.ToDouble(latitude), Convert.ToDouble(longitude)).Result;

            PlanOptionsState.PlaceName = placeName;
            PlanOptionsState.Latitude = latitude;
            PlanOptionsState.Longitude = longitude;
            PlanOptionsState.TimeZone = tz;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return false;
        }
    }

}
