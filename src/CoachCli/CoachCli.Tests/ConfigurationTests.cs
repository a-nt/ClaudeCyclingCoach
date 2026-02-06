using System.Text.Json;

namespace CoachCli.Tests;

public class ConfigurationTests
{
    [Fact]
    public void CoachConfiguration_Serialization_RoundTrips()
    {
        // Arrange
        var config = new CoachConfiguration
        {
            ApiKey = "test-api-key-123",
            AthleteId = "i99999",
            TrainingPhilosophy = "Polarized",
            Context = new TrainingContext
            {
                WeeklyHours = 8.5,
                WeeklyHoursTarget = 10.0,
                TrainingDays = new string[] { "Mon", "Wed", "Fri", "Sat" },
                CurrentPhase = "Base",
                TrainingConsistency = 0.85,
                LastContextUpdate = DateTime.Now,
                Goals = new Goal[]
                {
                    new Goal
                    {
                        Type = "Event",
                        Name = "Spring Century",
                        Date = DateTime.Now.AddMonths(3),
                        Priority = "High"
                    }
                },
                Constraints = new string[] { "Limited weekday time" },
                FtpTestStatus = new FtpTestStatus
                {
                    LastTest = DateTime.Now.AddMonths(-2),
                    NextDue = DateTime.Now.AddDays(14),
                    CurrentFtp = 250
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        var deserialized = JsonSerializer.Deserialize<CoachConfiguration>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(config.ApiKey, deserialized.ApiKey);
        Assert.Equal(config.AthleteId, deserialized.AthleteId);
        Assert.Equal(config.TrainingPhilosophy, deserialized.TrainingPhilosophy);
        Assert.NotNull(deserialized.Context);
        Assert.Equal(config.Context.WeeklyHours, deserialized.Context.WeeklyHours);
        Assert.Equal(config.Context.WeeklyHoursTarget, deserialized.Context.WeeklyHoursTarget);
        Assert.Equal(config.Context.CurrentPhase, deserialized.Context.CurrentPhase);
        Assert.NotNull(deserialized.Context.Goals);
        Assert.Single(deserialized.Context.Goals);
        Assert.Equal("Spring Century", deserialized.Context.Goals[0].Name);
    }

    [Fact]
    public void SaveAndLoadConfiguration_CreatesValidFile()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.json");
        var config = new CoachConfiguration
        {
            ApiKey = "test-key",
            AthleteId = "i12345",
            TrainingPhilosophy = "Polarized"
        };

        try
        {
            // Act - Save
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(tempPath, json);

            // Assert - File exists
            Assert.True(File.Exists(tempPath));

            // Act - Load
            var loadedJson = File.ReadAllText(tempPath);
            var loadedConfig = JsonSerializer.Deserialize<CoachConfiguration>(loadedJson);

            // Assert - Data matches
            Assert.NotNull(loadedConfig);
            Assert.Equal("test-key", loadedConfig.ApiKey);
            Assert.Equal("i12345", loadedConfig.AthleteId);
            Assert.Equal("Polarized", loadedConfig.TrainingPhilosophy);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void TrainingContext_WithGoals_SerializesCorrectly()
    {
        // Arrange
        var context = new TrainingContext
        {
            Goals = new Goal[]
            {
                new Goal { Type = "Event", Name = "Race 1", Priority = "High" },
                new Goal { Type = "Training", Name = "FTP +20W", Priority = "Medium" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(context);
        var deserialized = JsonSerializer.Deserialize<TrainingContext>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Goals);
        Assert.Equal(2, deserialized.Goals.Length);
        Assert.Equal("Race 1", deserialized.Goals[0].Name);
        Assert.Equal("FTP +20W", deserialized.Goals[1].Name);
    }

    [Fact]
    public void FtpTestStatus_SerializesCorrectly()
    {
        // Arrange
        var ftpStatus = new FtpTestStatus
        {
            LastTest = new DateTime(2026, 1, 1),
            NextDue = new DateTime(2026, 3, 1),
            CurrentFtp = 250
        };

        // Act
        var json = JsonSerializer.Serialize(ftpStatus);
        var deserialized = JsonSerializer.Deserialize<FtpTestStatus>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(new DateTime(2026, 1, 1), deserialized.LastTest);
        Assert.Equal(new DateTime(2026, 3, 1), deserialized.NextDue);
        Assert.Equal(250, deserialized.CurrentFtp);
    }
}
