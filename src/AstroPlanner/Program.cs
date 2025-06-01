using AstroPlanner;
using AstroPlanner.Services;
using AstroPlanner.Util.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddFluentUIComponents();
        builder.Services.AddScoped<LocalStorage>();

        await DataProvider.InitStarsDb(builder.HostEnvironment.BaseAddress);

        await builder.Build().RunAsync();
    }
}