using Spectre.Console;
using Spectre.Console.Rendering;

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
    /// Render a Spectre.Console IRenderable to a string with ANSI colors
    /// </summary>
    private static string RenderToString(IRenderable renderable)
    {
        var stringWriter = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.Standard,
            Out = new AnsiConsoleOutput(stringWriter)
        });

        console.Write(renderable);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Generate a horizontal bar chart for zone distribution
    /// </summary>
    public static string GenerateZoneDistributionChart(Dictionary<int, double> zoneTimes, int totalSeconds, string[] zoneNames)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Time in Power Zones[/]")
            .AddColumn(new TableColumn("Zone").LeftAligned())
            .AddColumn(new TableColumn("Distribution").Centered())
            .AddColumn(new TableColumn("% Time").RightAligned())
            .AddColumn(new TableColumn("Minutes").RightAligned());

        var zoneColors = new[] { "grey", "blue", "green", "yellow", "orange1", "red", "red" };

        for (int zone = 1; zone <= 7; zone++)
        {
            if (!zoneTimes.ContainsKey(zone)) continue;

            var seconds = zoneTimes[zone];
            var percentage = (seconds / totalSeconds) * 100;
            var minutes = (int)(seconds / 60);

            var barLength = (int)(percentage / 5); // 20 chars = 100%
            var filledBar = new string('█', Math.Min(barLength, 20));
            var emptyBar = new string('░', Math.Max(20 - barLength, 0));

            var zoneName = zone <= zoneNames.Length ? zoneNames[zone - 1] : $"Z{zone}";
            var color = zone <= zoneColors.Length ? zoneColors[zone - 1] : "white";

            var coloredBar = $"[{color}]{filledBar}[/]{emptyBar}";

            table.AddRow(
                $"[{color}]{zoneName}[/]",
                coloredBar,
                $"{percentage:F0}%",
                $"{minutes}min"
            );
        }

        return RenderToString(table);
    }

    /// <summary>
    /// Generate a horizontal bar chart for HR zone distribution
    /// </summary>
    public static string GenerateHrZoneDistributionChart(Dictionary<int, double> zoneTimes, int totalSeconds, string[] zoneNames)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Time in Heart Rate Zones[/]")
            .AddColumn(new TableColumn("Zone").LeftAligned())
            .AddColumn(new TableColumn("Distribution").Centered())
            .AddColumn(new TableColumn("% Time").RightAligned())
            .AddColumn(new TableColumn("Minutes").RightAligned());

        // HR zones typically use: grey, blue, green, yellow, red
        var zoneColors = new[] { "grey", "blue", "green", "yellow", "red" };

        for (int zone = 1; zone <= 5; zone++)
        {
            if (!zoneTimes.ContainsKey(zone)) continue;

            var seconds = zoneTimes[zone];
            var percentage = (seconds / totalSeconds) * 100;
            var minutes = (int)(seconds / 60);

            var barLength = (int)(percentage / 5); // 20 chars = 100%
            var filledBar = new string('█', Math.Min(barLength, 20));
            var emptyBar = new string('░', Math.Max(20 - barLength, 0));

            var zoneName = zone <= zoneNames.Length ? zoneNames[zone - 1] : $"Z{zone}";
            var color = zone <= zoneColors.Length ? zoneColors[zone - 1] : "white";

            var coloredBar = $"[{color}]{filledBar}[/]{emptyBar}";

            table.AddRow(
                $"[{color}]{zoneName}[/]",
                coloredBar,
                $"{percentage:F0}%",
                $"{minutes}min"
            );
        }

        return RenderToString(table);
    }

    /// <summary>
    /// Generate a simple power curve chart
    /// </summary>
    public static string GeneratePowerCurveChart(Dictionary<string, int> powerCurve, int? ftp)
    {
        if (powerCurve == null || powerCurve.Count == 0)
            return string.Empty;

        var panel = new Panel(GeneratePowerCurveContent(powerCurve, ftp))
            .Header("[bold]Power Curve Analysis[/]")
            .Border(BoxBorder.Rounded);

        return RenderToString(panel);
    }

    private static string GeneratePowerCurveContent(Dictionary<string, int> powerCurve, int? ftp)
    {
        var content = new System.Text.StringBuilder();

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
            return "No power curve data available";

        // Show values
        foreach (var (label, power) in values)
        {
            var color = power > 400 ? "red" : power > 300 ? "orange1" : power > 250 ? "yellow" : "green";
            content.AppendLine($"[{color}]{label,4}: {power}W[/]");
        }

        // FTP estimation from 20-min power
        if (powerCurve.TryGetValue("1200", out int power20min))
        {
            var estimatedFtp = (int)(power20min * 0.95);
            content.AppendLine();
            content.AppendLine($"[cyan]20-min power: {power20min}W[/]");
            content.AppendLine($"[bold cyan]Estimated FTP: {estimatedFtp}W[/]");

            if (ftp.HasValue)
            {
                var diff = estimatedFtp - ftp.Value;
                var diffColor = diff > 0 ? "green" : diff < 0 ? "red" : "grey";
                var sign = diff >= 0 ? "+" : "";
                content.AppendLine($"Current FTP: {ftp}W ([{diffColor}]{sign}{diff}W[/])");
            }
        }

        return content.ToString();
    }

    /// <summary>
    /// Generate a simple timeline showing intervals
    /// </summary>
    public static string GenerateIntervalTimeline(List<DetectedInterval> intervals, int totalSeconds)
    {
        if (intervals == null || intervals.Count == 0)
            return string.Empty;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Detected Intervals[/] [dim](Sustained threshold work)[/]")
            .AddColumn("Time")
            .AddColumn("Duration")
            .AddColumn("Avg Power")
            .AddColumn("% FTP");

        foreach (var interval in intervals.OrderBy(i => i.Start))
        {
            var duration = interval.End - interval.Start;
            var startTime = FormatTime(interval.Start);
            var endTime = FormatTime(interval.End);
            var durationStr = FormatTime(duration);

            var ftpColor = interval.FtpPercent >= 105 ? "red" :
                          interval.FtpPercent >= 95 ? "orange1" :
                          interval.FtpPercent >= 85 ? "yellow" : "green";

            table.AddRow(
                $"{startTime} - {endTime}",
                durationStr,
                $"[cyan]{interval.AveragePower:F0}W[/]",
                interval.FtpPercent.HasValue ? $"[{ftpColor}]{interval.FtpPercent:F0}%[/]" : "N/A"
            );
        }

        return RenderToString(table);
    }

    /// <summary>
    /// Generate decoupling visualization
    /// </summary>
    public static string GenerateDecouplingChart(double firstHalfRatio, double secondHalfRatio, double decouplingPercent)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Aerobic Decoupling Analysis[/]")
            .AddColumn("Metric")
            .AddColumn(new TableColumn("Value").RightAligned());

        table.AddRow("First Half", $"[cyan]{firstHalfRatio:F2} W/bpm[/]");
        table.AddRow("Second Half", $"[cyan]{secondHalfRatio:F2} W/bpm[/]");

        var decouplingColor = Math.Abs(decouplingPercent) < 5 ? "green" :
                             Math.Abs(decouplingPercent) < 10 ? "yellow" : "red";
        table.AddRow("Decoupling", $"[{decouplingColor}]{decouplingPercent:F1}%[/]");

        string rating;
        string message;
        if (Math.Abs(decouplingPercent) < 5)
        {
            rating = "[green]Excellent ✓[/]";
            message = "Strong aerobic fitness - minimal cardiac drift";
        }
        else if (Math.Abs(decouplingPercent) < 10)
        {
            rating = "[yellow]Good ○[/]";
            message = "Solid aerobic base - some cardiac drift under load";
        }
        else
        {
            rating = "[red]Needs Work ⚠[/]";
            message = "Focus on Z2 endurance work to improve aerobic efficiency";
        }

        table.AddEmptyRow();
        table.AddRow("[bold]Rating[/]", rating);
        table.AddRow("[dim]Assessment[/]", $"[dim]{message}[/]");

        return RenderToString(table);
    }

    /// <summary>
    /// Generate a compact ride summary box
    /// </summary>
    public static string GenerateRideSummary(ApiActivity activity, int? ftp)
    {
        var duration = FormatTime(activity.MovingTime ?? 0);
        var distance = activity.Distance.HasValue ? $"{(activity.Distance / 1000):F1}km" : "N/A";
        var avgPower = activity.GetAveragePower();
        var np = activity.GetNormalizedPower();
        var vi = activity.GetVariabilityIndex();
        var avgHr = activity.GetAverageHr();
        var maxHr = activity.GetMaxHr();
        var ef = activity.GetEfficiencyFactor();

        var name = activity.Name ?? "Activity";
        if (name.Length > 45)
            name = name.Substring(0, 42) + "...";

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold cyan]{name}[/]")
            .AddColumn(new TableColumn("Metric").Width(15))
            .AddColumn(new TableColumn("Value").Width(20))
            .AddColumn(new TableColumn("Metric").Width(15))
            .AddColumn(new TableColumn("Value").Width(20));

        table.AddRow(
            "[dim]Duration[/]", $"[cyan]{duration}[/]",
            "[dim]Distance[/]", $"[cyan]{distance}[/]"
        );

        if (avgPower.HasValue && np.HasValue)
        {
            var viStr = vi.HasValue ? $"{vi:F2}" : "N/A";
            var ifStr = ftp.HasValue && np.HasValue ? $"{(np / ftp):F2}" : "N/A";

            table.AddRow(
                "[dim]Avg Power[/]", $"[yellow]{avgPower:F0}W[/]",
                "[dim]NP[/]", $"[yellow]{np:F0}W[/]"
            );
            table.AddRow(
                "[dim]VI[/]", $"[cyan]{viStr}[/]",
                "[dim]IF[/]", $"[orange1]{ifStr}[/]"
            );
        }

        if (avgHr.HasValue)
        {
            var efStr = ef.HasValue ? $"{ef:F2}" : "N/A";
            table.AddRow(
                "[dim]Avg HR[/]", $"[red]{avgHr} bpm[/]",
                "[dim]Max HR[/]", $"[red]{maxHr} bpm[/]"
            );
            table.AddRow(
                "[dim]Efficiency[/]", $"[green]{efStr}[/]",
                "", ""
            );
        }
        else if (!avgPower.HasValue)
        {
            // Show basic info if no power/HR
            var tss = activity.GetTrainingLoad();
            var cadence = activity.AverageCadence ?? activity.AvgCadence;

            if (tss.HasValue || cadence.HasValue)
            {
                table.AddRow(
                    tss.HasValue ? "[dim]TSS[/]" : "",
                    tss.HasValue ? $"[yellow]{tss:F0}[/]" : "",
                    cadence.HasValue ? "[dim]Cadence[/]" : "",
                    cadence.HasValue ? $"[cyan]{cadence:F0} rpm[/]" : ""
                );
            }
        }

        return RenderToString(table);
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
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]Power Zones[/] [dim](FTP: {ftp}W)[/]")
            .AddColumn(new TableColumn("Zone").Centered())
            .AddColumn(new TableColumn("Name").LeftAligned())
            .AddColumn(new TableColumn("% FTP").Centered())
            .AddColumn(new TableColumn("Watts").RightAligned());

        var zoneNames = new[] { "Recovery", "Endurance", "Tempo", "Threshold", "VO2max", "Anaerobic", "Neuromuscular" };
        var zoneColors = new[] { "grey", "blue", "green", "yellow", "orange1", "red", "red" };

        for (int i = 0; i < Math.Min(zones.Length - 1, zoneNames.Length); i++)
        {
            var lowerPercent = zones[i];
            var upperPercent = zones[i + 1];
            var lowerWatts = (int)(ftp * lowerPercent / 100.0);
            var upperWatts = i < zones.Length - 2 ? (int)(ftp * upperPercent / 100.0) : 999;

            var range = upperWatts < 999 ? $"{lowerWatts}-{upperWatts}W" : $"{lowerWatts}W+";
            var color = i < zoneColors.Length ? zoneColors[i] : "white";

            table.AddRow(
                $"[bold {color}]Z{i + 1}[/]",
                $"[{color}]{zoneNames[i]}[/]",
                $"[dim]{lowerPercent}-{upperPercent}%[/]",
                $"[{color}]{range}[/]"
            );
        }

        return RenderToString(table);
    }

    /// <summary>
    /// Generate HR zones visualization
    /// </summary>
    public static string GenerateHrZones(int[] zones, int maxHr)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]Heart Rate Zones[/] [dim](Max HR: {maxHr} bpm)[/]")
            .AddColumn(new TableColumn("Zone").Centered())
            .AddColumn(new TableColumn("Name").LeftAligned())
            .AddColumn(new TableColumn("BPM Range").RightAligned());

        var zoneNames = new[] { "Recovery", "Aerobic", "Tempo", "Threshold", "Max" };
        var zoneColors = new[] { "grey", "blue", "green", "yellow", "red" };

        for (int i = 0; i < Math.Min(zones.Length, zoneNames.Length); i++)
        {
            var lowerBpm = i == 0 ? 0 : zones[i - 1];
            var upperBpm = zones[i];
            var color = i < zoneColors.Length ? zoneColors[i] : "white";

            table.AddRow(
                $"[bold {color}]Z{i + 1}[/]",
                $"[{color}]{zoneNames[i]}[/]",
                $"[{color}]{lowerBpm}-{upperBpm} bpm[/]"
            );
        }

        return RenderToString(table);
    }

    /// <summary>
    /// Generate CTL/ATL/TSB trend sparkline chart
    /// </summary>
    public static string GenerateFitnessTrend(List<WellnessDataPoint> data, int days = 30)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        var recentData = data.TakeLast(days).ToList();
        if (recentData.Count == 0)
            return string.Empty;

        var latest = recentData.Last();
        var oldest = recentData.First();
        var ctlChange = latest.Ctl - oldest.Ctl;
        var tsbCurrent = latest.Tsb;

        // Determine form zone
        string formZone;
        string formColor;
        if (tsbCurrent >= 15)
        {
            formZone = "Fresh/Racing";
            formColor = "green";
        }
        else if (tsbCurrent >= -10)
        {
            formZone = "Transitional";
            formColor = "yellow";
        }
        else if (tsbCurrent >= -30)
        {
            formZone = "Optimal Training";
            formColor = "blue";
        }
        else if (tsbCurrent >= -50)
        {
            formZone = "Overreaching";
            formColor = "orange1";
        }
        else
        {
            formZone = "High Risk";
            formColor = "red";
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]Fitness Trend[/] [dim](Last {recentData.Count} Days)[/]")
            .AddColumn(new TableColumn("Metric").Width(20))
            .AddColumn(new TableColumn("Value").Width(30));

        var ctlColor = ctlChange >= 0 ? "green" : "red";
        var ctlSign = ctlChange >= 0 ? "+" : "";

        table.AddRow(
            "[cyan]CTL (Fitness)[/]",
            $"[bold]{latest.Ctl:F1}[/] ([{ctlColor}]{ctlSign}{ctlChange:F1}[/])"
        );
        table.AddRow(
            "[yellow]ATL (Fatigue)[/]",
            $"[bold]{latest.Atl:F1}[/]"
        );
        table.AddRow(
            "[bold]TSB (Form)[/]",
            $"[{formColor}]{tsbCurrent:F1}[/] [dim]({formZone})[/]"
        );

        if (latest.RampRate != null)
        {
            var rampColor = latest.RampRate.Value > 8 ? "red" :
                           latest.RampRate.Value > 5 ? "green" : "yellow";
            var rampWarning = latest.RampRate.Value > 8 ? " ⚠️" : "";

            table.AddRow(
                "[dim]Ramp Rate[/]",
                $"[{rampColor}]{latest.RampRate:F1} TSS/week{rampWarning}[/]"
            );
        }

        // Add sparkline
        var sparkline = GenerateSparkline(recentData.Select(d => d.Ctl).ToList(), 40);
        table.AddEmptyRow();
        table.AddRow("[dim]CTL Progression[/]", $"[cyan]{sparkline}[/]");

        return RenderToString(table);
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
}

public class DetectedInterval
{
    public int Start { get; set; }
    public int End { get; set; }
    public double AveragePower { get; set; }
    public double? FtpPercent { get; set; }
}
