using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoachCli;

// Profile models
public class ApiProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("icu_weight")]
    public double? IcuWeight { get; set; }

    [JsonPropertyName("weight")]
    public double? Weight { get; set; }

    [JsonPropertyName("sportSettings")]
    public SportSettings[]? SportSettings { get; set; }
}

public class SportSettings
{
    [JsonPropertyName("types")]
    public string[]? Types { get; set; }

    [JsonPropertyName("ftp")]
    public int? Ftp { get; set; }

    [JsonPropertyName("indoor_ftp")]
    public int? IndoorFtp { get; set; }

    [JsonPropertyName("power_zones")]
    public int[]? PowerZones { get; set; }

    [JsonPropertyName("hr_zones")]
    public int[]? HrZones { get; set; }

    [JsonPropertyName("max_hr")]
    public int? MaxHr { get; set; }

    [JsonPropertyName("resting_hr")]
    public int? RestingHr { get; set; }

    [JsonPropertyName("lthr")]
    public int? Lthr { get; set; }
}

// Activity models
public class ApiActivity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("start_date_local")]
    public DateTime? StartDateLocal { get; set; }

    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("distance")]
    public double? Distance { get; set; }

    [JsonPropertyName("moving_time")]
    public int? MovingTime { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int? ElapsedTime { get; set; }

    [JsonPropertyName("average_watts")]
    public double? AverageWatts { get; set; }

    [JsonPropertyName("avg_watts")]
    public double? AvgWatts { get; set; }

    [JsonPropertyName("max_watts")]
    public double? MaxWatts { get; set; }

    [JsonPropertyName("max_power")]
    public double? MaxPower { get; set; }

    [JsonPropertyName("np")]
    public double? Np { get; set; }

    [JsonPropertyName("normalized_power")]
    public double? NormalizedPower { get; set; }

    [JsonPropertyName("icu_if")]
    public double? IcuIf { get; set; }

    [JsonPropertyName("intensity_factor")]
    public double? IntensityFactor { get; set; }

    [JsonPropertyName("variability_index")]
    public double? VariabilityIndex { get; set; }

    [JsonPropertyName("vi")]
    public double? Vi { get; set; }

    [JsonPropertyName("icu_training_load")]
    public double? IcuTrainingLoad { get; set; }

    [JsonPropertyName("training_load")]
    public double? TrainingLoad { get; set; }

    [JsonPropertyName("tss")]
    public double? Tss { get; set; }

    [JsonPropertyName("average_hr")]
    public int? AverageHr { get; set; }

    [JsonPropertyName("avg_hr")]
    public int? AvgHr { get; set; }

    [JsonPropertyName("average_heartrate")]
    public int? AverageHeartrate { get; set; }

    [JsonPropertyName("max_hr")]
    public int? MaxHr { get; set; }

    [JsonPropertyName("max_heartrate")]
    public int? MaxHeartrate { get; set; }

    [JsonPropertyName("average_cadence")]
    public double? AverageCadence { get; set; }

    [JsonPropertyName("avg_cadence")]
    public double? AvgCadence { get; set; }

    [JsonPropertyName("efficiency_factor")]
    public double? EfficiencyFactor { get; set; }

    [JsonPropertyName("ef")]
    public double? Ef { get; set; }

    [JsonPropertyName("power_curve")]
    public Dictionary<string, int>? PowerCurve { get; set; }

    [JsonPropertyName("hr_curve")]
    public Dictionary<string, int>? HrCurve { get; set; }

    [JsonPropertyName("streams")]
    public ApiStreams? Streams { get; set; }

    [JsonPropertyName("intervals")]
    public ApiInterval[]? Intervals { get; set; }

    [JsonPropertyName("icu_zone_times")]
    public JsonElement? IcuZoneTimes { get; set; }

    [JsonPropertyName("efficiency")]
    public double? Efficiency { get; set; }

    // Helper properties to get unified values
    public double? GetAveragePower() => AverageWatts ?? AvgWatts;
    public double? GetNormalizedPower() => Np ?? NormalizedPower;
    public double? GetIntensityFactor() => IcuIf ?? IntensityFactor;
    public double? GetVariabilityIndex() => VariabilityIndex ?? Vi;
    public double? GetTrainingLoad() => IcuTrainingLoad ?? TrainingLoad ?? Tss;
    public int? GetAverageHr() => AverageHr ?? AvgHr ?? AverageHeartrate;
    public int? GetMaxHr() => MaxHr ?? MaxHeartrate;
    public double? GetEfficiencyFactor() => EfficiencyFactor ?? Ef ?? Efficiency;
}

public class ApiStreams
{
    [JsonPropertyName("watts")]
    public int[]? Watts { get; set; }

    [JsonPropertyName("heartrate")]
    public int[]? Heartrate { get; set; }

    [JsonPropertyName("cadence")]
    public int[]? Cadence { get; set; }

    [JsonPropertyName("time")]
    public int[]? Time { get; set; }
}

public class ApiInterval
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("start")]
    public int? Start { get; set; }

    [JsonPropertyName("end")]
    public int? End { get; set; }

    [JsonPropertyName("average_watts")]
    public double? AverageWatts { get; set; }

    [JsonPropertyName("average_hr")]
    public int? AverageHr { get; set; }
}

// Wellness model
public class ApiWellness
{
    [JsonPropertyName("id")]
    public DateTime Id { get; set; }

    [JsonPropertyName("ctl")]
    public double Ctl { get; set; }

    [JsonPropertyName("atl")]
    public double Atl { get; set; }

    [JsonPropertyName("rampRate")]
    public double? RampRate { get; set; }

    [JsonPropertyName("icu_weight")]
    public double? Weight { get; set; }

    [JsonPropertyName("restingHR")]
    public int? RestingHr { get; set; }

    [JsonPropertyName("hrv_sdnn")]
    public double? HrvSdnn { get; set; }
}
