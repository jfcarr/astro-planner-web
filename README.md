# astro-planner-web

A simple web-based astronomical observation plan generator.

## Local Setup

1. Clone the repo.

2. Open a command prompt in the src/AstroPlanner directory.

3. Build and run the project (you'll need .NET 8 installed):

```bash
make watch
```

or

```bash
dotnet watch
```

After the project loads in your default browser, select the "Observing Plan" page, open the Observer Options, and fill in a zip code and observation date.  Information specific to that location and date will load in the main UI.

## Publish

If you want to generate a standalone web project suitable for uploading to a web host:

1. Open a command prompt in the src/AstroPlanner directory.

2. Publish the project:

```bash
make publish
```

or

```bash
dotnet publish -c Release
```

A website with all of the required WebAssembly binaries will be generated in `src/AstroPlanner/bin/Release/publish/wwwroot`.  Upload all of the contents of the wwwroot directory to your web host, including all subdirectories.  You can also run locally with a development server, e.g., `php -S localhost:8088`.

## Some Technical Details

The (published) project runs completely client-side, i.e., there are no server-side dependencies: IIS and the .NET runtime are not required on the host.

The project uses the following 3rd-party .NET libraries:

* [FluentUI](https://www.fluentui-blazor.net/)
* [GeoTimeZone](https://www.nuget.org/packages/GeoTimeZone)
* [Practical Astronomy .NET](https://www.nuget.org/packages/PracticalAstronomyDotNet)

These are referenced by the .csproj file and will be loaded automatically.

The project also uses the [Zippopotam.us postal code API](https://zippopotam.us/) and manages "sticky settings" using local browser storage.
