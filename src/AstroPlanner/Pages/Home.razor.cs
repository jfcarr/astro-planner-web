using AstroPlanner.Services;
using AstroPlanner.Util.Models;
using AstroPlanner.Util.Services;

namespace AstroPlanner.Pages;

public partial class Home
{
    private bool showObserverOptions;
    private bool showSections = false;
    private bool showSunInfo = true;
    private bool showMoonInfo = true;
    private bool showPlanetInfo = true;
    private bool showBrightStarInfo = true;
    private bool showEclipseInfo = true;
    private bool showVisiblePlanetsOnly = true;
    private bool showVisibleStarsOnly = true;

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
        if (Boolean.TryParse(await LocalStorage.GetItemAsync("ShowSunInfo"), out bool showSunInfoValue))
            showSunInfo = showSunInfoValue;
        if (Boolean.TryParse(await LocalStorage.GetItemAsync("ShowMoonInfo"), out bool showMoonInfoValue))
            showMoonInfo = showMoonInfoValue;
        if (Boolean.TryParse(await LocalStorage.GetItemAsync("ShowPlanetInfo"), out bool showPlanetInfoValue))
            showPlanetInfo = showPlanetInfoValue;
        if (Boolean.TryParse(await LocalStorage.GetItemAsync("ShowBrightStarInfo"), out bool showBrightStarInfoValue))
            showBrightStarInfo = showBrightStarInfoValue;
        if (Boolean.TryParse(await LocalStorage.GetItemAsync("ShowEclipseInfo"), out bool showEclipseInfoValue))
            showEclipseInfo = showEclipseInfoValue;

        if (!String.IsNullOrEmpty(zipCode) && String.IsNullOrEmpty(PlanOptionsState.PlaceName))
        {
            if (await LocationProvider.GetCoordinates(zipCode))
                await UpdateState();
        }

        if (!String.IsNullOrEmpty(PlanOptionsState.PlaceName) && PlanOptionsState.ObservationDate is not null)
        {
            AstroData.UpdateEclipseInfo();
            AstroData.UpdateMoonInfo();
            AstroData.UpdateSunInfo();
            AstroData.UpdatePlanetInfo(showVisiblePlanetsOnly);
            AstroData.UpdateStarInfo(showVisibleStarsOnly);
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
        await LocalStorage.SetItemAsync("ShowSunInfo", showSunInfo.ToString());
        await LocalStorage.SetItemAsync("ShowMoonInfo", showMoonInfo.ToString());
        await LocalStorage.SetItemAsync("ShowPlanetInfo", showPlanetInfo.ToString());
        await LocalStorage.SetItemAsync("ShowBrightStarInfo", showBrightStarInfo.ToString());
        await LocalStorage.SetItemAsync("ShowEclipseInfo", showEclipseInfo.ToString());

        if (!String.IsNullOrEmpty(PlanOptionsState.PlaceName) && PlanOptionsState.ObservationDate is not null)
        {
            AstroData.UpdateEclipseInfo();
            AstroData.UpdateMoonInfo();
            AstroData.UpdateSunInfo();
            AstroData.UpdatePlanetInfo(showVisiblePlanetsOnly);
            AstroData.UpdateStarInfo(showVisibleStarsOnly);
        }
    }
}
