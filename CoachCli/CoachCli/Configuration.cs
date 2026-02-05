using System.Text.Json;

namespace CoachCli;

public class CoachConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string AthleteId { get; set; } = string.Empty;
    public string? TrainingPhilosophy { get; set; }
    public TrainingContext? Context { get; set; }
}

public class TrainingContext
{
    public double? WeeklyHours { get; set; }
    public double? WeeklyHoursTarget { get; set; }
    public string[]? TrainingDays { get; set; }
    public string? CurrentPhase { get; set; }
    public double? TrainingConsistency { get; set; }
    public DateTime? LastContextUpdate { get; set; }
    public Goal[]? Goals { get; set; }
    public string[]? Constraints { get; set; }
    public FtpTestStatus? FtpTestStatus { get; set; }
}

public class Goal
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public DateTime? Date { get; set; }
    public string? Priority { get; set; }
}

public class FtpTestStatus
{
    public DateTime? LastTest { get; set; }
    public DateTime? NextDue { get; set; }
    public int? CurrentFtp { get; set; }
}

public static class ConfigurationService
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".claude/skills/coach/.config.json");

    public static CoachConfiguration Load()
    {
        if (!File.Exists(ConfigPath))
        {
            throw new FileNotFoundException("Configuration not found. Run /coach setup first.", ConfigPath);
        }

        var json = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<CoachConfiguration>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null || string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.AthleteId))
        {
            throw new InvalidOperationException("Invalid configuration. Missing apiKey or athleteId.");
        }

        return config;
    }
}
