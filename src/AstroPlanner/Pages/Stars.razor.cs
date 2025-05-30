using System.Net.Http.Json;
using AstroPlanner.Models;

namespace AstroPlanner.Pages;

public partial class Stars
{
    private Star[]? stars;

    protected override async Task OnInitializedAsync()
    {
        stars = await Http.GetFromJsonAsync<Star[]>("astro-data/stars.json");
    }
}
