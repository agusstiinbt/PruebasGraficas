namespace CigoWeb.Core.Helpers.Time;

public static class TimeHelper
{
    /// <summary>
    /// Generates a list of time options covering a full 24-hour day,
    /// using a fixed interval (default: 15 minutes).
    /// Each option contains a TimeSpan value and a formatted
    /// display string in 12-hour (AM/PM) format.
    /// </summary>
    public static List<TimeOption> GenerateTimeOptions(int intervalMinutes = 15)
    {
        var list = new List<TimeOption>();

        for (int i = 0; i < 24 * 60; i += intervalMinutes)
        {
            var time = new TimeOnly(i / 60, i % 60);

            list.Add(new TimeOption
            {
                Value = time,
                Display = FormatToAmPm(time)
            });
        }

        return list;
    }

    private static string FormatToAmPm(TimeOnly time)
    {
        return time.ToString("hh:mm tt");// Format as 12-hour with AM/PM
    }
}
