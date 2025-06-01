namespace AstroPlanner.Util.Helpers;

public static class Direction
{
    private static readonly string[] CardinalDirections =
{
        "N", "NNE", "NE", "ENE",
        "E", "ESE", "SE", "SSE",
        "S", "SSW", "SW", "WSW",
        "W", "WNW", "NW", "NNW"
    };

    public static string AzimuthToCardinalDirection(double azimuth)
    {
        azimuth %= 360;

        if (azimuth < 0)
            azimuth += 360;

        // Divide the circle into 16 equal slices.
        // Adding 11.25 (half of 22.5) ensures correct matching when at the edge.
        int index = (int)((azimuth + 11.25) / 22.5) % 16;

        return CardinalDirections[index];
    }
}
