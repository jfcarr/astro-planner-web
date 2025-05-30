using Microsoft.AspNetCore.Components;

namespace AstroPlanner.Models;

public static class PlanOptionsState
{
    /// <summary>
    /// Page Title, visible in the browser tab and page header bar.
    /// </summary>
    public static string PageTitle { get; set; } = "Plan Options";
    public static string? ZipCode { get; set; }
    public static string? PlaceName { get; set; }
    public static string? Latitude { get; set; }
    public static string? Longitude { get; set; }
    public static string? TimeZone { get; set; }
    public static TimeSpan? TimeZoneOffset { get; set; }
    public static bool? IsDaylightSaving { get; set; }
    public static DateTime? ObservationDate { get; set; }
}
