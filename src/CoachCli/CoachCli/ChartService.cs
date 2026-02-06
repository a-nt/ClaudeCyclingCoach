namespace CoachCli;

public static class ChartService
{
    /// <summary>
    /// Generate a horizontal bar chart for zone distribution
    /// </summary>
    public static string GenerateZoneDistributionChart(Dictionary<int, double> zoneTimes, int totalSeconds, string[] zoneNames)
    {
        var chart = new System.Text.StringBuilder();
        chart.AppendLine("\nTime in Power Zones\n");

        for (int zone = 1; zone <= 5; zone++)
        {
            if (!zoneTimes.ContainsKey(zone)) continue;

            var seconds = zoneTimes[zone];
            var percentage = (seconds / totalSeconds) * 100;
            var minutes = (int)(seconds / 60);

            var barLength = (int)(percentage / 5); // 20 chars = 100%
            var bar = new string('█', Math.Min(barLength, 20)) + new string('░', Math.Max(20 - barLength, 0));

            var zoneName = zone <= zoneNames.Length ? zoneNames[zone - 1] : $"Z{zone}";
            chart.AppendLine($"{zoneName,-4} {bar} {percentage,3:F0}%  ━━━ {minutes,3}min ━━━");
        }

        chart.AppendLine("     └────────────────────┴");
        chart.AppendLine("     0%                 100%");

        return chart.ToString();
    }

    /// <summary>
    /// Generate a simple power curve chart
    /// </summary>
    public static string GeneratePowerCurveChart(Dictionary<string, int> powerCurve, int? ftp)
    {
        if (powerCurve == null || powerCurve.Count == 0)
            return string.Empty;

        var chart = new System.Text.StringBuilder();
        chart.AppendLine("\nPower Curve Analysis\n");

        // Get key durations we care about
        var durations = new[] { "5", "60", "300", "1200" };
        var labels = new[] { "5s", "1m", "5m", "20m" };
        var values = new List<(string label, int power)>();

        for (int i = 0; i < durations.Length; i++)
        {
            if (powerCurve.TryGetValue(durations[i], out int power))
            {
                values.Add((labels[i], power));
            }
        }

        if (values.Count == 0)
            return string.Empty;

        // Find max for scaling
        int maxPower = values.Max(v => v.power);
        int scale = 10; // Each line represents this many watts

        // Draw chart
        for (int w = maxPower; w >= 0; w -= scale * 5)
        {
            var line = $"{w,4}W ┤";

            foreach (var (label, power) in values)
            {
                if (power >= w - scale && power < w + scale)
                    line += " ●";
                else
                    line += "  ";
            }

            chart.AppendLine(line);
        }

        chart.AppendLine("     └────┴────┴────┴────►");
        chart.Append("      ");
        foreach (var (label, _) in values)
        {
            chart.Append($"{label,-5}");
        }
        chart.AppendLine();

        // Add annotations
        chart.AppendLine();
        foreach (var (label, power) in values)
        {
            chart.AppendLine($"{label,4}: {power}W");
        }

        // FTP estimation from 20-min power
        if (powerCurve.TryGetValue("1200", out int power20min))
        {
            var estimatedFtp = (int)(power20min * 0.95);
            chart.AppendLine();
            chart.AppendLine($"20-min power: {power20min}W → Estimated FTP: {estimatedFtp}W");
            if (ftp.HasValue)
            {
                var diff = estimatedFtp - ftp.Value;
                var sign = diff >= 0 ? "+" : "";
                chart.AppendLine($"Current FTP: {ftp}W ({sign}{diff}W difference)");
            }
        }

        return chart.ToString();
    }

    /// <summary>
    /// Generate a simple timeline showing intervals
    /// </summary>
    public static string GenerateIntervalTimeline(List<DetectedInterval> intervals, int totalSeconds)
    {
        if (intervals == null || intervals.Count == 0)
            return string.Empty;

        var chart = new System.Text.StringBuilder();
        chart.AppendLine("\nDetected Intervals (Sustained threshold work)\n");

        // Create timeline (60 chars = total duration)
        var timeline = new char[60];
        Array.Fill(timeline, '─');

        foreach (var interval in intervals)
        {
            var startPos = (int)((interval.Start / (double)totalSeconds) * 60);
            var endPos = (int)((interval.End / (double)totalSeconds) * 60);

            for (int i = startPos; i < endPos && i < 60; i++)
            {
                timeline[i] = '█';
            }
        }

        chart.AppendLine($"0:00 {new string(timeline)} {FormatTime(totalSeconds)}");
        chart.AppendLine();

        // List intervals
        foreach (var interval in intervals.OrderBy(i => i.Start))
        {
            var duration = interval.End - interval.Start;
            var pctFtp = interval.FtpPercent.HasValue ? $" ({interval.FtpPercent:F0}% FTP)" : "";
            chart.AppendLine($"• {FormatTime(interval.Start)} - {FormatTime(interval.End)}: " +
                           $"{FormatTime(duration)} @ {interval.AveragePower:F0}W{pctFtp}");
        }

        return chart.ToString();
    }

    /// <summary>
    /// Generate decoupling visualization
    /// </summary>
    public static string GenerateDecouplingChart(double firstHalfRatio, double secondHalfRatio, double decouplingPercent)
    {
        var chart = new System.Text.StringBuilder();
        chart.AppendLine("\nAerobic Decoupling Analysis\n");

        // Determine rating
        string rating;
        string indicator;
        if (Math.Abs(decouplingPercent) < 5)
        {
            rating = "Excellent";
            indicator = "✓";
        }
        else if (Math.Abs(decouplingPercent) < 10)
        {
            rating = "Acceptable";
            indicator = "○";
        }
        else
        {
            rating = "Needs work";
            indicator = "⚠";
        }

        chart.AppendLine($"First Half:   {firstHalfRatio:F2} W/bpm");
        chart.AppendLine($"Second Half:  {secondHalfRatio:F2} W/bpm");
        chart.AppendLine($"Decoupling:   {decouplingPercent:F1}%");
        chart.AppendLine();
        chart.AppendLine($"Rating: {rating} {indicator}");

        if (Math.Abs(decouplingPercent) < 5)
        {
            chart.AppendLine("Strong aerobic fitness - minimal cardiac drift");
        }
        else if (Math.Abs(decouplingPercent) < 10)
        {
            chart.AppendLine("Good aerobic base - some cardiac drift under load");
        }
        else
        {
            chart.AppendLine("Focus on Z2 endurance work to improve aerobic efficiency");
        }

        return chart.ToString();
    }

    /// <summary>
    /// Generate a compact ride summary box
    /// </summary>
    public static string GenerateRideSummary(ApiActivity activity, int? ftp)
    {
        var chart = new System.Text.StringBuilder();

        var duration = FormatTime(activity.MovingTime ?? 0);
        var distance = activity.Distance.HasValue ? $"{(activity.Distance / 1000):F1}km" : "N/A";
        var avgPower = activity.GetAveragePower();
        var np = activity.GetNormalizedPower();
        var vi = activity.GetVariabilityIndex();
        var avgHr = activity.GetAverageHr();
        var maxHr = activity.MaxHr;
        var ef = activity.GetEfficiencyFactor();

        chart.AppendLine("┌─────────────────────────────────────────────────────┐");
        chart.AppendLine($"│ 🚴 {activity.Name,-30} {duration,7} {distance,8} │");
        chart.AppendLine("├─────────────────────────────────────────────────────┤");

        if (avgPower.HasValue && np.HasValue)
        {
            var viStr = vi.HasValue ? $"VI: {vi:F2}" : "";
            var ifStr = ftp.HasValue && np.HasValue ? $"IF: {(np / ftp):F2}" : "";
            chart.AppendLine($"│ Power   Avg: {avgPower:F0}W │ NP: {np:F0}W │ {viStr,-9} {ifStr,-9} │");
        }

        if (avgHr.HasValue)
        {
            var efStr = ef.HasValue ? $"EF: {ef:F2}" : "";
            chart.AppendLine($"│ HR      Avg: {avgHr,3} │ Max: {maxHr,3}    │ {efStr,-22} │");
        }

        chart.AppendLine("└─────────────────────────────────────────────────────┘");

        return chart.ToString();
    }

    private static string FormatTime(int seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        return $"{ts.Minutes}:{ts.Seconds:D2}";
    }
}

public class DetectedInterval
{
    public int Start { get; set; }
    public int End { get; set; }
    public double AveragePower { get; set; }
    public double? FtpPercent { get; set; }
}
