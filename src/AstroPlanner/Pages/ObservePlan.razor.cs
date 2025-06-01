using AstroPlanner.Services;
using AstroPlanner.Util.Models;
using AstroPlanner.Util.Services;

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
        {
            if (await LocationProvider.GetCoordinates(zipCode))
                await UpdateState();
        }

        if (!String.IsNullOrEmpty(PlanOptionsState.PlaceName) && PlanOptionsState.ObservationDate is not null)
        {
            AstroData.UpdateEclipseInfo();
            AstroData.UpdateMoonInfo();
            AstroData.UpdateSunInfo();
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
            AstroData.UpdateEclipseInfo();
            AstroData.UpdateMoonInfo();
            AstroData.UpdateSunInfo();
        }
    }
}
