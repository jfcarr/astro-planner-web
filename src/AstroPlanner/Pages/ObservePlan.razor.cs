using System.Globalization;
using AstroPlanner.Models;
using AstroPlanner.Services;
using GeoTimeZone;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using PALib;

namespace AstroPlanner.Pages;

public partial class ObservePlan
{
    private bool showObserverOptions;
    private bool showSections = false;
    private bool showSunInfo = true;
    private bool showMoonInfo = true;
    private bool showEclipseInfo = true;

    private string? zipCode;
    private DateTime? observationDate;
    private DateTime? observationTime = null;

    protected override async Task OnInitializedAsync()
    {
        zipCode = await LocalStorage.GetItemAsync("ZipCode");
        if (DateTime.TryParse(await LocalStorage.GetItemAsync("ObservationDate"), out DateTime dateValue))
            observationDate = dateValue;
        if (DateTime.TryParse(await LocalStorage.GetItemAsync("ObservationTime"), out DateTime timeValue))
            observationTime = timeValue;


        if (!String.IsNullOrEmpty(zipCode) && String.IsNullOrEmpty(PlanOptionsState.PlaceName))
            await GetCoordinates();

        if (!String.IsNullOrEmpty(PlanOptionsState.PlaceName) && PlanOptionsState.ObservationDate is not null)
        {
            UpdateEclipseInfo();
            UpdateMoonInfo();
            UpdateSunInfo();
        }
    }

    private async Task GetCoordinates()
    {
        try
        {
            (string placeName, string latitude, string longitude) = await DataProvider.GetLocationInfo(zipCode ?? "");

            string tz = TimeZoneLookup.GetTimeZone(Convert.ToDouble(latitude), Convert.ToDouble(longitude)).Result;

            PlanOptionsState.PlaceName = placeName;
            PlanOptionsState.Latitude = latitude;
            PlanOptionsState.Longitude = longitude;
            PlanOptionsState.TimeZone = tz;

            await UpdateState();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task UpdateState()
    {
        PlanOptionsState.ZipCode = zipCode;
        PlanOptionsState.ObservationDate = observationDate;
        PlanOptionsState.ObservationTime = observationTime;

        if (!string.IsNullOrEmpty(PlanOptionsState.TimeZone) && PlanOptionsState.ObservationDate is not null)
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(PlanOptionsState.TimeZone);
            PlanOptionsState.TimeZoneOffset = tzi.GetUtcOffset((DateTime)PlanOptionsState.ObservationDate);
            PlanOptionsState.IsDaylightSaving = tzi.IsDaylightSavingTime((DateTime)PlanOptionsState.ObservationDate);
        }

        await LocalStorage.SetItemAsync("ZipCode", zipCode ?? "");
        await LocalStorage.SetItemAsync("ObservationDate", observationDate.ToString() ?? "");
        await LocalStorage.SetItemAsync("ObservationTime", observationTime.ToString() ?? "");

        if (!String.IsNullOrEmpty(PlanOptionsState.PlaceName) && PlanOptionsState.ObservationDate is not null)
        {
            UpdateEclipseInfo();
            UpdateMoonInfo();
            UpdateSunInfo();
        }
    }

    private void UpdateEclipseInfo()
    {
        PAEclipses eclipseInfo = new();
        DateOnly checkDate = PlanOptionsState.ObservationDate.ToDateOnly().AddDays(-2);

        (string status, double eventDateDay, int eventDateMonth, int eventDateYear) occurrence = eclipseInfo.LunarEclipseOccurrence(
            checkDate.Day, checkDate.Month, checkDate.Year,
            false,
            (PlanOptionsState.TimeZoneOffset is not null) ? Math.Abs(PlanOptionsState.TimeZoneOffset.Value.Hours) : 0
        );

        EclipseInfo.LunarEclipseStatus = occurrence.status;
        EclipseInfo.LunarEclipseDate = EclipseInfo.LunarEclipseStatus.Contains("certain", StringComparison.CurrentCultureIgnoreCase)
                    ? DateTime.Parse($"{occurrence.eventDateMonth}/{occurrence.eventDateDay}/{occurrence.eventDateYear}").ToLongDateString()
                    : "N/A";

        if (EclipseInfo.LunarEclipseDate == "N/A")
        {
            EclipseInfo.LunarEclipseWhen = "N/A";
        }
        else
        {
            DateTime? lunarEclipseDateTime = DateTime.Parse($"{occurrence.eventDateMonth}/{occurrence.eventDateDay}/{occurrence.eventDateYear}");
            DateOnly lunarEclipseDate = new DateOnly(occurrence.eventDateYear, occurrence.eventDateMonth, (int)occurrence.eventDateDay);
            DateOnly observationDate = PlanOptionsState.ObservationDate.ToDateOnly();

            EclipseInfo.LunarEclipseWhen = lunarEclipseDate.CompareTo(observationDate) switch
            {
                0 => "today",
                < 0 => "past",
                > 0 => "future"
            };
        }
    }

    private void UpdateMoonInfo()
    {
        PAMoon pAMoon = new();
        DateOnly checkDate = PlanOptionsState.ObservationDate.ToDateOnly();

        (double mrLTHour, double mrLTMin, double mrLocalDateDay, int mrLocalDateMonth, int mrLocalDateYear, double mrAzimuthDeg, double msLTHour, double msLTMin, double msLocalDateDay, int msLocalDateMonth, int msLocalDateYear, double msAzimuthDeg) riseSet = pAMoon.MoonriseAndMoonset(
            checkDate.Day, checkDate.Month, checkDate.Year,
            false,
            (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0,
            Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude)
        );

        DateOnly riseDate = DateOnly.FromDateTime(new DateTime(riseSet.mrLocalDateYear, riseSet.mrLocalDateMonth, (int)riseSet.mrLocalDateDay));
        DateOnly setDate = DateOnly.FromDateTime(new DateTime(riseSet.msLocalDateYear, riseSet.msLocalDateMonth, (int)riseSet.msLocalDateDay));

        DateTime riseTime = new DateTime(1, 1, 1, (int)riseSet.mrLTHour, (int)riseSet.mrLTMin, 0);
        DateTime setTime = new DateTime(1, 1, 1, (int)riseSet.msLTHour, (int)riseSet.msLTMin, 0);

        if (riseDate < checkDate)
        {
            checkDate = PlanOptionsState.ObservationDate.ToDateOnly().AddDays(1);

            riseSet = pAMoon.MoonriseAndMoonset(
                checkDate.Day, checkDate.Month, checkDate.Year,
                false,
                (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0,
                Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude)
            );

            riseDate = DateOnly.FromDateTime(new DateTime(riseSet.mrLocalDateYear, riseSet.mrLocalDateMonth, (int)riseSet.mrLocalDateDay));
            riseTime = new DateTime(1, 1, 1, (int)riseSet.mrLTHour, (int)riseSet.mrLTMin, 0);
        }

        if (setDate < checkDate || new DateTime(setDate.Year, setDate.Month, setDate.Day, setTime.Hour, setTime.Minute, setTime.Second) < new DateTime(riseDate.Year, riseDate.Month, riseDate.Day, riseTime.Hour, riseTime.Minute, riseTime.Second))
        {
            checkDate = PlanOptionsState.ObservationDate.ToDateOnly().AddDays(1);

            riseSet = pAMoon.MoonriseAndMoonset(
                checkDate.Day, checkDate.Month, checkDate.Year,
                false,
                (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0,
                Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude)
            );

            setDate = DateOnly.FromDateTime(new DateTime(riseSet.msLocalDateYear, riseSet.msLocalDateMonth, (int)riseSet.msLocalDateDay));
            setTime = new DateTime(1, 1, 1, (int)riseSet.msLTHour, (int)riseSet.msLTMin, 0);
        }

        MoonInfo.RiseTime = $"{riseDate.ToLongDateString()} at {riseTime.ToString("h:mm tt", CultureInfo.InvariantCulture)}";
        MoonInfo.SetTime = $"{setDate.ToLongDateString()} at {setTime.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

        checkDate = PlanOptionsState.ObservationDate.ToDateOnly();

        (double moonPhase, double paBrightLimbDeg) moonPhase = pAMoon.MoonPhase(20, 0, 0, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, PAAccuracyLevel.Precise);

        MoonInfo.PhaseDescription = moonPhase.moonPhase.ToString();

        MoonInfo.PhaseDescription = moonPhase.moonPhase switch
        {
            >= .99 => $"Full ({moonPhase.moonPhase * 100}%)",
            < .01 => $"New ({moonPhase.moonPhase * 100}%)",
            >= .48 and <= .52 => $"Half-Full ({moonPhase.moonPhase * 100}%)",
            < .48 => $"Crescent ({moonPhase.moonPhase * 100}%)",
            > .52 => $"Gibbous ({moonPhase.moonPhase * 100}%)",
            _ => "Unknown",
        };
    }

    private void UpdateSunInfo()
    {
        PASun pASun = new();
        DateOnly checkDate = PlanOptionsState.ObservationDate.ToDateOnly();

        // Astronomical Twilight
        (double amTwilightBeginsHour, double amTwilightBeginsMin, double pmTwilightEndsHour, double pmTwilightEndsMin, string status) twilightInfo = pASun.MorningAndEveningTwilight(checkDate.Day, checkDate.Month, checkDate.Year, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude), PATwilightType.Astronomical);

        SunInfo.AstronomicalTwilightBegins = (twilightInfo.status == "OK") ? new DateTime(1, 1, 1, (int)twilightInfo.amTwilightBeginsHour, (int)twilightInfo.amTwilightBeginsMin, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";
        SunInfo.AstronomicalTwilightEnds = (twilightInfo.status == "OK") ? new DateTime(1, 1, 1, (int)twilightInfo.pmTwilightEndsHour, (int)twilightInfo.pmTwilightEndsMin, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";

        // Nautical Twilight
        twilightInfo = pASun.MorningAndEveningTwilight(checkDate.Day, checkDate.Month, checkDate.Year, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude), PATwilightType.Nautical);

        SunInfo.NauticalTwilightBegins = (twilightInfo.status == "OK") ? new DateTime(1, 1, 1, (int)twilightInfo.amTwilightBeginsHour, (int)twilightInfo.amTwilightBeginsMin, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";
        SunInfo.NauticalTwilightEnds = (twilightInfo.status == "OK") ? new DateTime(1, 1, 1, (int)twilightInfo.pmTwilightEndsHour, (int)twilightInfo.pmTwilightEndsMin, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";

        // Civil Twilight
        twilightInfo = pASun.MorningAndEveningTwilight(checkDate.Day, checkDate.Month, checkDate.Year, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude), PATwilightType.Civil);

        SunInfo.CivilTwilightBegins = (twilightInfo.status == "OK") ? new DateTime(1, 1, 1, (int)twilightInfo.amTwilightBeginsHour, (int)twilightInfo.amTwilightBeginsMin, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";
        SunInfo.CivilTwilightEnds = (twilightInfo.status == "OK") ? new DateTime(1, 1, 1, (int)twilightInfo.pmTwilightEndsHour, (int)twilightInfo.pmTwilightEndsMin, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";

        // Sunrise and Sunset
        (double localSunriseHour, double localSunriseMinute, double localSunsetHour, double localSunsetMinute, double azimuthOfSunriseDeg, double azimuthOfSunsetDeg, string status) riseSet = pASun.SunriseAndSunset(checkDate.Day, checkDate.Month, checkDate.Year, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude));

        SunInfo.SunRise = (riseSet.status == "OK") ? new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, (int)riseSet.localSunriseHour, (int)riseSet.localSunriseMinute, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";
        SunInfo.SunSet = (riseSet.status == "OK") ? new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, (int)riseSet.localSunsetHour, (int)riseSet.localSunsetMinute, 0).ToString("h:mm tt", CultureInfo.InvariantCulture) : "";
    }
}
