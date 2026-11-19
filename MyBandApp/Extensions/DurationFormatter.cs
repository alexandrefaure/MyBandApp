namespace MyBandApp.Extensions;

public static class DurationFormatter
{
    public static string FormatTotal(IEnumerable<TimeSpan> durations)
    {
        var total = durations.Aggregate(TimeSpan.Zero, (acc, d) => acc + d);

        if (total.TotalHours >= 1)
        {
            return $"{(int)total.TotalHours:00}h{total.Minutes:00}";
        }

        return $"{(int)total.TotalMinutes:00}:{total.Seconds:00}";
    }
}