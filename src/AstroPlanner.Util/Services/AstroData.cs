using System.Globalization;
using System.Net.Http.Json;
using AstroPlanner.Util.Models;
using PALib;

namespace AstroPlanner.Util.Services;

public static class AstroData
{
    public static void UpdateEclipseInfo()
    {
        PAEclipses eclipseInfo = new();
        DateTime checkDate = (PlanOptionsState.ObservationDate is not null) ? PlanOptionsState.ObservationDate.Value.Date.AddDays(-2) : new DateTime();

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
            // DateTime? lunarEclipseDateTime = DateTime.Parse($"{occurrence.eventDateMonth}/{occurrence.eventDateDay}/{occurrence.eventDateYear}");
            DateTime lunarEclipseDate = new(occurrence.eventDateYear, occurrence.eventDateMonth, (int)occurrence.eventDateDay);
            DateTime observationDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();

            EclipseInfo.LunarEclipseWhen = lunarEclipseDate.CompareTo(observationDate) switch
            {
                0 => "today",
                < 0 => "past",
                > 0 => "upcoming"
            };
        }
    }

    public static void UpdateMoonInfo()
    {
        PAMoon pAMoon = new();
        PACoordinates pACoordinates = new();

        DateTime checkDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();
        DateTime? checkTime = PlanOptionsState.ObservationTime;

        (double mrLTHour, double mrLTMin, double mrLocalDateDay, int mrLocalDateMonth, int mrLocalDateYear, double mrAzimuthDeg, double msLTHour, double msLTMin, double msLocalDateDay, int msLocalDateMonth, int msLocalDateYear, double msAzimuthDeg) riseSet = pAMoon.MoonriseAndMoonset(
            checkDate.Day, checkDate.Month, checkDate.Year,
            false,
            (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0,
            Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude)
        );

        DateTime riseDate = new(riseSet.mrLocalDateYear, riseSet.mrLocalDateMonth, (int)riseSet.mrLocalDateDay);
        DateTime setDate = new(riseSet.msLocalDateYear, riseSet.msLocalDateMonth, (int)riseSet.msLocalDateDay);

        DateTime riseTime = new(1, 1, 1, (int)riseSet.mrLTHour, (int)riseSet.mrLTMin, 0);
        DateTime setTime = new(1, 1, 1, (int)riseSet.msLTHour, (int)riseSet.msLTMin, 0);

        if (riseDate < checkDate)
        {
            checkDate = checkDate.AddDays(1);

            riseSet = pAMoon.MoonriseAndMoonset(
                checkDate.Day, checkDate.Month, checkDate.Year,
                false,
                (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0,
                Convert.ToDouble(PlanOptionsState.Longitude), Convert.ToDouble(PlanOptionsState.Latitude)
            );

            riseDate = new DateTime(riseSet.mrLocalDateYear, riseSet.mrLocalDateMonth, (int)riseSet.mrLocalDateDay);
            riseTime = new DateTime(1, 1, 1, (int)riseSet.mrLTHour, (int)riseSet.mrLTMin, 0);
            setDate = new DateTime(riseSet.msLocalDateYear, riseSet.msLocalDateMonth, (int)riseSet.msLocalDateDay);
            setTime = new DateTime(1, 1, 1, (int)riseSet.msLTHour, (int)riseSet.msLTMin, 0);
        }

        if (
            new DateTime(setDate.Year, setDate.Month, setDate.Day, setTime.Hour, setTime.Minute, setTime.Second)
            <
            new DateTime(riseDate.Year, riseDate.Month, riseDate.Day, riseTime.Hour, riseTime.Minute, riseTime.Second)
        )
            setDate = setDate.AddDays(1);

        MoonInfo.RiseTime = $"{riseDate.ToLongDateString()} at {riseTime.ToString("h:mm tt", CultureInfo.InvariantCulture)}";
        MoonInfo.SetTime = $"{setDate.ToLongDateString()} at {setTime.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

        checkDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();

        (double moonPhase, double paBrightLimbDeg) moonPhase = pAMoon.MoonPhase(20, 0, 0, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, PAAccuracyLevel.Precise);

        MoonInfo.PhaseDescription = moonPhase.moonPhase.ToString();

        MoonInfo.PhaseDescription = moonPhase.moonPhase switch
        {
            >= .99 => $"Full ({Math.Floor(moonPhase.moonPhase * 100)}%)",
            <= .01 => $"New ({Math.Floor(moonPhase.moonPhase * 100)}%)",
            >= .48 and <= .52 => $"Half-Full ({Math.Floor(moonPhase.moonPhase * 100)}%)",
            < .48 => $"Crescent ({Math.Floor(moonPhase.moonPhase * 100)}%)",
            > .52 => $"Gibbous ({Math.Floor(moonPhase.moonPhase * 100)}%)",
            _ => "Unknown",
        };

        if (checkTime is not null)
        {
            (double moonRAHour, double moonRAMin, double moonRASec, double moonDecDeg, double moonDecMin, double moonDecSec) moonPosition = pAMoon.ApproximatePositionOfMoon(checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year);

            (double hourAngleHours, double hourAngleMinutes, double hourAngleSeconds) planetHourAngle = pACoordinates.RightAscensionToHourAngle(moonPosition.moonRAHour, moonPosition.moonRAMin, moonPosition.moonRASec, checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, Convert.ToDouble(PlanOptionsState.Longitude));

            (double azimuthDegrees, double azimuthMinutes, double azimuthSeconds, double altitudeDegrees, double altitudeMinutes, double altitudeSeconds) planetLocalPosition = pACoordinates.EquatorialCoordinatesToHorizonCoordinates(planetHourAngle.hourAngleHours, planetHourAngle.hourAngleMinutes, planetHourAngle.hourAngleSeconds, moonPosition.moonDecDeg, moonPosition.moonDecMin, moonPosition.moonDecSec, Convert.ToDouble(PlanOptionsState.Latitude));

            MoonInfo.AltitudeDegree = planetLocalPosition.altitudeDegrees;
            MoonInfo.AltitudeMinute = planetLocalPosition.altitudeMinutes;
            MoonInfo.AltitudeSecond = planetLocalPosition.altitudeSeconds;
            MoonInfo.AzimuthDegrees = planetLocalPosition.azimuthDegrees;
            MoonInfo.AzimuthMinute = planetLocalPosition.azimuthMinutes;
            MoonInfo.AzimuthSecond = planetLocalPosition.azimuthSeconds;
        }
    }

    public static void UpdateSunInfo()
    {
        PASun pASun = new();
        DateTime checkDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();

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

    public static void UpdatePlanetInfo(bool visibleOnly)
    {
        PAPlanet pAPlanet = new();
        PACoordinates pACoordinates = new();

        DateTime checkDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();
        DateTime? checkTime = PlanOptionsState.ObservationTime;

        PlanetInfo.PlanetDetails = [];
        PlanetInfo.PlanetDetailsFiltered = [];

        if (checkTime is not null)
        {
            string[] planetNames = ["Mercury", "Venus", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune"];

            foreach (var planetName in planetNames)
            {
                (double planetRAHour, double planetRAMin, double planetRASec, double planetDecDeg, double planetDecMin, double planetDecSec) planetPosition = pAPlanet.ApproximatePositionOfPlanet(checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, planetName);

                (double hourAngleHours, double hourAngleMinutes, double hourAngleSeconds) planetHourAngle = pACoordinates.RightAscensionToHourAngle(planetPosition.planetRAHour, planetPosition.planetRAMin, planetPosition.planetRASec, checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, Convert.ToDouble(PlanOptionsState.Longitude));

                (double azimuthDegrees, double azimuthMinutes, double azimuthSeconds, double altitudeDegrees, double altitudeMinutes, double altitudeSeconds) planetLocalPosition = pACoordinates.EquatorialCoordinatesToHorizonCoordinates(planetHourAngle.hourAngleHours, planetHourAngle.hourAngleMinutes, planetHourAngle.hourAngleSeconds, planetPosition.planetDecDeg, planetPosition.planetDecMin, planetPosition.planetDecSec, Convert.ToDouble(PlanOptionsState.Latitude));

                (double distanceAU, double angDiaArcsec, double phase, double lightTimeHour, double lightTimeMinutes, double lightTimeSeconds, double posAngleBrightLimbDeg, double approximateMagnitude) planetAspects = pAPlanet.VisualAspectsOfAPlanet(checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, planetName);

                PlanetInfo.PlanetDetails.Add(new PlanetDetail()
                {
                    Name = planetName,
                    RightAscensionHour = planetPosition.planetRAHour,
                    RightAscensionMinute = planetPosition.planetRAMin,
                    RightAscensionSecond = planetPosition.planetRASec,
                    DeclinationDegrees = planetPosition.planetDecDeg,
                    DeclinationMinute = planetPosition.planetDecMin,
                    DeclinationSecond = planetPosition.planetDecSec,
                    AltitudeDegree = planetLocalPosition.altitudeDegrees,
                    AltitudeMinute = planetLocalPosition.altitudeMinutes,
                    AltitudeSecond = planetLocalPosition.altitudeSeconds,
                    AzimuthDegrees = planetLocalPosition.azimuthDegrees,
                    AzimuthMinute = planetLocalPosition.azimuthMinutes,
                    AzimuthSecond = planetLocalPosition.azimuthSeconds,
                    Magnitude = planetAspects.approximateMagnitude
                });

            }

            PlanetInfo.PlanetDetailsFiltered = visibleOnly ? PlanetInfo.PlanetDetails.Where(x => x.AltitudeDegree > 0).ToList() : PlanetInfo.PlanetDetails;
        }
    }

    public static async Task LoadStarInfo(string baseAddress)
    {
        try
        {
            HttpClient http = new();

            StarInfo.stars = await http.GetFromJsonAsync<Star[]>($"{baseAddress}astro-data/stars.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void UpdateStarInfo(bool visibleOnly)
    {
        if (StarInfo.stars != null)
        {
            PACoordinates pACoordinates = new();

            DateTime checkDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();
            DateTime? checkTime = PlanOptionsState.ObservationTime;

            StarInfo.StarDetails = [];
            StarInfo.StarDetailsFiltered = [];

            if (checkTime is not null)
            {
                foreach (Star star in StarInfo.stars)
                {
                    (double hourAngleHours, double hourAngleMinutes, double hourAngleSeconds) starHourAngle = pACoordinates.RightAscensionToHourAngle(star.RightAscensionHours, star.RightAscensionMinutes, star.RightAscensionSeconds, checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, Convert.ToDouble(PlanOptionsState.Longitude));

                    (double azimuthDegrees, double azimuthMinutes, double azimuthSeconds, double altitudeDegrees, double altitudeMinutes, double altitudeSeconds) starLocalPosition = pACoordinates.EquatorialCoordinatesToHorizonCoordinates(starHourAngle.hourAngleHours, starHourAngle.hourAngleMinutes, starHourAngle.hourAngleSeconds, star.DeclinationDegrees, star.DeclinationMinutes, star.DeclinationSeconds, Convert.ToDouble(PlanOptionsState.Latitude));

                    StarInfo.StarDetails.Add(new StarDetail()
                    {
                        Name = star.Name,
                        AltitudeDegree = starLocalPosition.altitudeDegrees,
                        AltitudeMinute = starLocalPosition.altitudeMinutes,
                        AltitudeSecond = starLocalPosition.altitudeSeconds,
                        AzimuthDegrees = starLocalPosition.azimuthDegrees,
                        AzimuthMinute = starLocalPosition.azimuthMinutes,
                        AzimuthSecond = starLocalPosition.azimuthSeconds,
                        Constellation = star.Constellation,
                        Magnitude = star.Magnitude
                    });
                }

                StarInfo.StarDetailsFiltered = visibleOnly ? StarInfo.StarDetails.Where(x => x.AltitudeDegree > 0).ToList() : StarInfo.StarDetails;
            }
        }
    }

    public static async Task LoadDeepSkyObjectInfo(string baseAddress)
    {
        try
        {
            HttpClient http = new();

            DeepSkyObjectInfo.DeepSkyObjects = await http.GetFromJsonAsync<DeepSkyObject[]>($"{baseAddress}astro-data/dso.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void UpdateDeepSkyObjectInfo(bool visibleOnly)
    {
        if (DeepSkyObjectInfo.DeepSkyObjects != null)
        {
            PACoordinates pACoordinates = new();

            DateTime checkDate = (PlanOptionsState.ObservationDate is not null) ? (DateTime)PlanOptionsState.ObservationDate : new DateTime();
            DateTime? checkTime = PlanOptionsState.ObservationTime;

            DeepSkyObjectInfo.DeepSkyObjectDetails = [];
            DeepSkyObjectInfo.DeepSkyObjectDetailsFiltered = [];

            if (checkTime is not null)
            {
                foreach (DeepSkyObject deepSkyObject in DeepSkyObjectInfo.DeepSkyObjects)
                {
                    (double hourAngleHours, double hourAngleMinutes, double hourAngleSeconds) starHourAngle = pACoordinates.RightAscensionToHourAngle(deepSkyObject.RightAscensionHours, deepSkyObject.RightAscensionMinutes, deepSkyObject.RightAscensionSeconds, checkTime.Value.Hour, checkTime.Value.Minute, checkTime.Value.Second, false, (PlanOptionsState.TimeZoneOffset is not null) ? PlanOptionsState.TimeZoneOffset.Value.Hours : 0, checkDate.Day, checkDate.Month, checkDate.Year, Convert.ToDouble(PlanOptionsState.Longitude));

                    (double azimuthDegrees, double azimuthMinutes, double azimuthSeconds, double altitudeDegrees, double altitudeMinutes, double altitudeSeconds) starLocalPosition = pACoordinates.EquatorialCoordinatesToHorizonCoordinates(starHourAngle.hourAngleHours, starHourAngle.hourAngleMinutes, starHourAngle.hourAngleSeconds, deepSkyObject.DeclinationDegrees, deepSkyObject.DeclinationMinutes, deepSkyObject.DeclinationSeconds, Convert.ToDouble(PlanOptionsState.Latitude));

                    DeepSkyObjectInfo.DeepSkyObjectDetails.Add(new DeepSkyObjectDetail()
                    {
                        Name = deepSkyObject.Name,
                        Description = deepSkyObject.Description,
                        AltitudeDegree = starLocalPosition.altitudeDegrees,
                        AltitudeMinute = starLocalPosition.altitudeMinutes,
                        AltitudeSecond = starLocalPosition.altitudeSeconds,
                        AzimuthDegrees = starLocalPosition.azimuthDegrees,
                        AzimuthMinute = starLocalPosition.azimuthMinutes,
                        AzimuthSecond = starLocalPosition.azimuthSeconds,
                        Type = deepSkyObject.Type,
                        Constellation = deepSkyObject.Constellation,
                        Magnitude = deepSkyObject.Magnitude
                    });
                }

                DeepSkyObjectInfo.DeepSkyObjectDetailsFiltered = visibleOnly
                    ? DeepSkyObjectInfo.DeepSkyObjectDetails.Where(x => x.AltitudeDegree > 0).ToList()
                    : DeepSkyObjectInfo.DeepSkyObjectDetails;
            }
        }
    }
}
