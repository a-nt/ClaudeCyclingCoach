using System.Text.Json;
using CoachCli;

if (args.Length == 0)
{
    Console.WriteLine("Usage: coach-cli <command> [options]");
    Console.WriteLine("\nCommands:");
    Console.WriteLine("  profile                  Get athlete profile and training zones");
    Console.WriteLine("  activities [--limit N]   List recent activities (default: 10)");
    Console.WriteLine("  activity <id>            Get single activity with streams");
    Console.WriteLine("  wellness [--days N]      Get CTL/ATL/TSB data (default: 90 days)");
    Console.WriteLine("  analyze <id>             Analyze activity with calculations");
    return 1;
}

var command = args[0].ToLower();

try
{
    var config = ConfigurationService.Load();
    var client = new IntervalsApiClient(config.ApiKey, config.AthleteId);

    switch (command)
    {
        case "profile":
            await HandleProfile(client);
            break;

        case "activities":
            var limit = GetOptionValue(args, "--limit", 10);
            await HandleActivities(client, limit);
            break;

        case "activity":
            if (args.Length < 2)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { success = false, error = "Missing activity ID" }));
                return 1;
            }
            await HandleActivity(client, args[1]);
            break;

        case "wellness":
            var days = GetOptionValue(args, "--days", 90);
            await HandleWellness(client, days);
            break;

        case "analyze":
            if (args.Length < 2)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { success = false, error = "Missing activity ID" }));
                return 1;
            }
            await HandleAnalyze(client, args[1]);
            break;

        default:
            Console.WriteLine(JsonSerializer.Serialize(new { success = false, error = $"Unknown command: {command}" }));
            return 1;
    }

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(JsonSerializer.Serialize(new { success = false, error = ex.Message }));
    return 1;
}

static int GetOptionValue(string[] args, string option, int defaultValue)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == option && int.TryParse(args[i + 1], out var value))
        {
            return value;
        }
    }
    return defaultValue;
}

static async Task HandleProfile(IntervalsApiClient client)
{
    var profile = await client.GetProfileAsync();

    var cyclingSetting = profile.SportSettings?
        .FirstOrDefault(s => s.Types?.Contains("Ride") == true || s.Types?.Contains("VirtualRide") == true);

    // Generate charts
    var charts = new Dictionary<string, string>();

    if (cyclingSetting?.Ftp != null && cyclingSetting?.PowerZones != null)
    {
        charts["powerZones"] = ChartService.GeneratePowerZones(cyclingSetting.Ftp.Value, cyclingSetting.PowerZones);
    }

    if (cyclingSetting?.HrZones != null && cyclingSetting?.MaxHr != null)
    {
        charts["hrZones"] = ChartService.GenerateHrZones(cyclingSetting.HrZones, cyclingSetting.MaxHr.Value);
    }

    var result = new
    {
        success = true,
        data = new
        {
            name = profile.Name,
            athleteId = profile.Id,
            ftp = cyclingSetting?.Ftp,
            indoorFtp = cyclingSetting?.IndoorFtp,
            ftpWattsPerKg = cyclingSetting?.Ftp != null && profile.IcuWeight != null
                ? (cyclingSetting.Ftp.Value / profile.IcuWeight.Value).ToString("F2")
                : null,
            weight = profile.IcuWeight ?? profile.Weight,
            powerZones = cyclingSetting?.PowerZones,
            hrZones = cyclingSetting?.HrZones,
            maxHr = cyclingSetting?.MaxHr,
            restingHr = cyclingSetting?.RestingHr,
            lthr = cyclingSetting?.Lthr,
            charts = charts.Count > 0 ? charts : null
        }
    };

    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}

static async Task HandleActivities(IntervalsApiClient client, int limit)
{
    var activities = await client.GetActivitiesAsync(limit);

    var result = new
    {
        success = true,
        data = activities.Select(a => new
        {
            id = a.Id,
            name = a.Name,
            startDate = a.StartDateLocal ?? a.StartDate,
            type = a.Type,
            distance = a.Distance,
            movingTime = a.MovingTime,
            averagePower = a.GetAveragePower(),
            normalizedPower = a.GetNormalizedPower(),
            tss = a.GetTrainingLoad(),
            averageHr = a.GetAverageHr()
        }).ToList()
    };

    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}

static async Task HandleActivity(IntervalsApiClient client, string id)
{
    var activity = await client.GetActivityAsync(id);

    var result = new
    {
        success = true,
        data = new
        {
            id = activity.Id,
            name = activity.Name,
            startDate = activity.StartDateLocal ?? activity.StartDate,
            type = activity.Type,
            distance = activity.Distance,
            duration = activity.ElapsedTime ?? activity.MovingTime,
            movingTime = activity.MovingTime,
            averagePower = activity.GetAveragePower(),
            normalizedPower = activity.GetNormalizedPower(),
            maxPower = activity.MaxPower,
            intensityFactor = activity.GetIntensityFactor(),
            variabilityIndex = activity.GetVariabilityIndex(),
            trainingStress = activity.GetTrainingLoad(),
            averageHr = activity.GetAverageHr(),
            maxHr = activity.GetMaxHr(),
            averageCadence = activity.AverageCadence ?? activity.AvgCadence,
            efficiency = activity.GetEfficiencyFactor(),
            powerCurve = activity.PowerCurve,
            hrCurve = activity.HrCurve,
            streams = activity.Streams,
            intervals = activity.Intervals,
            icu_zone_times = activity.IcuZoneTimes,
            icu_hr_zone_times = activity.IcuHrZoneTimes
        }
    };

    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}

static async Task HandleWellness(IntervalsApiClient client, int days)
{
    var wellness = await client.GetWellnessAsync(days);

    var wellnessData = wellness.Select(w => new
    {
        date = w.Id,
        ctl = w.Ctl,
        atl = w.Atl,
        tsb = w.Ctl - w.Atl,
        rampRate = w.RampRate,
        weight = w.Weight,
        restingHr = w.RestingHr,
        hrvSdnn = w.HrvSdnn
    }).ToList();

    // Generate fitness trend chart
    var dataPoints = wellness.Select(w => new WellnessDataPoint
    {
        Ctl = w.Ctl,
        Atl = w.Atl,
        Tsb = w.Ctl - w.Atl,
        RampRate = w.RampRate
    }).ToList();

    var charts = new Dictionary<string, string>();
    if (dataPoints.Count > 0)
    {
        charts["fitnessTrend"] = ChartService.GenerateFitnessTrend(dataPoints, Math.Min(30, days));
    }

    var result = new
    {
        success = true,
        data = new
        {
            wellness = wellnessData,
            charts = charts.Count > 0 ? charts : null
        }
    };

    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}

static async Task HandleAnalyze(IntervalsApiClient client, string id)
{
    var activity = await client.GetActivityAsync(id);
    var profile = await client.GetProfileAsync();

    var analysis = AnalysisService.Analyze(activity, profile);

    var result = new
    {
        success = true,
        data = analysis
    };

    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}
