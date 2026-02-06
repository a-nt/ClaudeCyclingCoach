namespace CoachCli;

public class WellnessDataPoint
{
    public double Ctl { get; set; }
    public double Atl { get; set; }
    public double Tsb { get; set; }
    public double? RampRate { get; set; }
}

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

    /// <summary>
    /// Generate power zones visualization
    /// </summary>
    public static string GeneratePowerZones(int ftp, int[] zones)
    {
        var chart = new System.Text.StringBuilder();
        chart.AppendLine("\nPower Zones (based on FTP)\n");

        var zoneNames = new[] { "Z1 Recovery", "Z2 Endurance", "Z3 Tempo", "Z4 Threshold", "Z5 VO2max", "Z6 Anaerobic", "Z7 Neuromuscular" };
        var zoneColors = new[] { "░", "▒", "▓", "█", "█", "█", "█" };

        for (int i = 0; i < Math.Min(zones.Length - 1, zoneNames.Length); i++)
        {
            var lowerPercent = zones[i];
            var upperPercent = zones[i + 1];
            var lowerWatts = (int)(ftp * lowerPercent / 100.0);
            var upperWatts = i < zones.Length - 2 ? (int)(ftp * upperPercent / 100.0) : 999;

            var bar = new string(zoneColors[i][0], 10);
            var range = upperWatts < 999 ? $"{lowerWatts}-{upperWatts}W" : $"{lowerWatts}W+";

            chart.AppendLine($"{zoneNames[i],-18} {bar}  {lowerPercent,3}%-{upperPercent,3}%  │ {range}");
        }

        return chart.ToString();
    }

    /// <summary>
    /// Generate HR zones visualization
    /// </summary>
    public static string GenerateHrZones(int[] zones, int maxHr)
    {
        var chart = new System.Text.StringBuilder();
        chart.AppendLine("\nHeart Rate Zones\n");

        var zoneNames = new[] { "Z1 Recovery", "Z2 Aerobic", "Z3 Tempo", "Z4 Threshold", "Z5 Max" };
        var zoneColors = new[] { "░", "▒", "▓", "█", "█" };

        for (int i = 0; i < Math.Min(zones.Length, zoneNames.Length); i++)
        {
            var lowerBpm = i == 0 ? 0 : zones[i - 1];
            var upperBpm = zones[i];

            var bar = new string(zoneColors[i][0], 10);
            var range = $"{lowerBpm}-{upperBpm} bpm";

            chart.AppendLine($"{zoneNames[i],-18} {bar}  │ {range}");
        }

        // Add max HR zone
        if (zones.Length >= 5)
        {
            chart.AppendLine($"{"Max HR",-18} {"█",-10}  │ {zones[4]}+ bpm ({maxHr} max)");
        }

        return chart.ToString();
    }

    /// <summary>
    /// Generate CTL/ATL/TSB trend sparkline chart
    /// </summary>
    public static string GenerateFitnessTrend(List<WellnessDataPoint> data, int days = 30)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        // Take last N days
        var recentData = data.TakeLast(days).ToList();
        if (recentData.Count == 0)
            return string.Empty;

        var chart = new System.Text.StringBuilder();
        chart.AppendLine($"\nFitness Trend (Last {recentData.Count} Days)\n");

        // Get current values
        var latest = recentData.Last();
        var oldest = recentData.First();
        var ctlChange = latest.Ctl - oldest.Ctl;
        var tsbCurrent = latest.Tsb;

        // Determine form zone
        string formZone;
        string formIndicator;
        if (tsbCurrent >= 15)
        {
            formZone = "Fresh/Racing";
            formIndicator = "🟢";
        }
        else if (tsbCurrent >= -10)
        {
            formZone = "Transitional";
            formIndicator = "🟡";
        }
        else if (tsbCurrent >= -30)
        {
            formZone = "Optimal Training";
            formIndicator = "🔵";
        }
        else if (tsbCurrent >= -50)
        {
            formZone = "Overreaching";
            formIndicator = "🟠";
        }
        else
        {
            formZone = "High Risk";
            formIndicator = "🔴";
        }

        chart.AppendLine($"CTL (Fitness):  {latest.Ctl:F1} ({(ctlChange >= 0 ? "+" : "")}{ctlChange:F1} from {recentData.Count} days ago)");
        chart.AppendLine($"ATL (Fatigue):  {latest.Atl:F1}");
        chart.AppendLine($"TSB (Form):     {tsbCurrent:F1} {formIndicator} {formZone}");

        if (latest.RampRate != null)
        {
            var rampStr = latest.RampRate.Value.ToString("F1");
            var rampIndicator = latest.RampRate.Value > 8 ? "⚠️" : latest.RampRate.Value > 5 ? "✓" : "↓";
            chart.AppendLine($"Ramp Rate:      {rampStr} TSS/week {rampIndicator}");
        }

        // Generate sparkline for CTL
        chart.AppendLine();
        chart.AppendLine("CTL Progression:");
        chart.Append("  ");
        var sparkline = GenerateSparkline(recentData.Select(d => d.Ctl).ToList(), 50);
        chart.AppendLine(sparkline);

        // TSB bar
        chart.AppendLine();
        chart.AppendLine("Form Zone:");
        var tsbBar = GenerateTsbBar(tsbCurrent);
        chart.AppendLine(tsbBar);

        return chart.ToString();
    }

    private static string GenerateSparkline(List<double> values, int width)
    {
        if (values.Count == 0) return string.Empty;

        var min = values.Min();
        var max = values.Max();
        var range = max - min;

        if (range == 0)
            return new string('─', width);

        var sparkChars = new[] { '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█' };
        var result = new System.Text.StringBuilder();

        // Sample values to fit width
        var step = Math.Max(1, values.Count / width);
        for (int i = 0; i < values.Count; i += step)
        {
            var value = values[i];
            var normalized = (value - min) / range;
            var charIndex = (int)(normalized * (sparkChars.Length - 1));
            result.Append(sparkChars[charIndex]);
        }

        return result.ToString();
    }

    private static string GenerateTsbBar(double tsb)
    {
        var chart = new System.Text.StringBuilder();

        // Bar ranges: -50 to +25
        var barWidth = 60;
        var zeroPosition = 40; // Position of zero on the bar (at -50 is 0, +25 is 60)

        // Calculate position (-50 to +25 range)
        var position = (int)((tsb + 50) / 75.0 * barWidth);
        position = Math.Max(0, Math.Min(barWidth - 1, position));

        // Build bar with zones
        var bar = new char[barWidth];
        for (int i = 0; i < barWidth; i++)
        {
            if (i < zeroPosition)
            {
                // Negative TSB (left of zero)
                if (i < 20)
                    bar[i] = '█'; // High risk zone (< -30)
                else if (i < zeroPosition)
                    bar[i] = '▓'; // Optimal training (-30 to -10)
            }
            else
            {
                // Positive TSB (right of zero)
                if (i < zeroPosition + 8)
                    bar[i] = '▒'; // Transitional (-10 to +10)
                else
                    bar[i] = '░'; // Fresh/racing (>+10)
            }
        }

        // Place indicator
        bar[position] = '▼';

        chart.Append("  ");
        chart.Append(new string(bar));
        chart.AppendLine();
        chart.AppendLine("  -50        -30      -10   0   +10      +25");
        chart.AppendLine("  High Risk  Optimal  Trans Fresh/Race");

        return chart.ToString();
    }
}

public class DetectedInterval
{
    public int Start { get; set; }
    public int End { get; set; }
    public double AveragePower { get; set; }
    public double? FtpPercent { get; set; }
}
