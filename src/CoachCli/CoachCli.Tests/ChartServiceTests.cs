namespace CoachCli.Tests;

public class ChartServiceTests
{
    [Fact]
    public void GenerateZoneDistributionChart_WithValidData_ReturnsChart()
    {
        // Arrange
        var zoneTimes = new Dictionary<int, double>
        {
            { 1, 300 },  // 5 min in Z1
            { 2, 1200 }, // 20 min in Z2
            { 3, 600 },  // 10 min in Z3
            { 4, 300 },  // 5 min in Z4
            { 5, 60 }    // 1 min in Z5
        };
        var totalSeconds = 2460;
        var zoneNames = new[] { "Z1", "Z2", "Z3", "Z4", "Z5" };

        // Act
        var chart = ChartService.GenerateZoneDistributionChart(zoneTimes, totalSeconds, zoneNames);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Time in Power Zones", chart);
        Assert.Contains("Z1", chart);
        Assert.Contains("Z2", chart);
        Assert.Contains("█", chart); // Should contain bar characters
    }

    [Fact]
    public void GeneratePowerCurveChart_WithValidData_ReturnsChart()
    {
        // Arrange
        var powerCurve = new Dictionary<string, int>
        {
            { "5", 400 },
            { "60", 300 },
            { "300", 250 },
            { "1200", 220 }
        };
        int? ftp = 250;

        // Act
        var chart = ChartService.GeneratePowerCurveChart(powerCurve, ftp);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Power Curve Analysis", chart);
        Assert.Contains("5s", chart);
        Assert.Contains("1m", chart);
        Assert.Contains("5m", chart);
        Assert.Contains("20m", chart);
        Assert.Contains("Estimated FTP", chart);
    }

    [Fact]
    public void GeneratePowerCurveChart_EstimatesFtpFrom20MinPower()
    {
        // Arrange
        var powerCurve = new Dictionary<string, int>
        {
            { "1200", 260 } // 20-min power of 260W
        };
        int? ftp = 250;

        // Act
        var chart = ChartService.GeneratePowerCurveChart(powerCurve, ftp);

        // Assert
        var estimatedFtp = (int)(260 * 0.95); // Should be 247W
        Assert.Contains($"Estimated FTP: {estimatedFtp}W", chart);
    }

    [Fact]
    public void GenerateIntervalTimeline_WithIntervals_ReturnsChart()
    {
        // Arrange
        var intervals = new List<DetectedInterval>
        {
            new DetectedInterval
            {
                Start = 300,
                End = 600,
                AveragePower = 240,
                FtpPercent = 96
            },
            new DetectedInterval
            {
                Start = 1200,
                End = 1500,
                AveragePower = 250,
                FtpPercent = 100
            }
        };
        var totalSeconds = 1800;

        // Act
        var chart = ChartService.GenerateIntervalTimeline(intervals, totalSeconds);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Detected Intervals", chart);
        Assert.Contains("█", chart); // Should contain interval markers
        Assert.Contains("240W", chart);
        Assert.Contains("250W", chart);
    }

    [Fact]
    public void GenerateDecouplingChart_Excellent_ReturnsCorrectRating()
    {
        // Arrange - Less than 5% decoupling
        double firstHalfRatio = 1.5;
        double secondHalfRatio = 1.48;
        double decouplingPercent = 1.3;

        // Act
        var chart = ChartService.GenerateDecouplingChart(firstHalfRatio, secondHalfRatio, decouplingPercent);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Aerobic Decoupling Analysis", chart);
        Assert.Contains("Excellent", chart);
        Assert.Contains("✓", chart);
    }

    [Fact]
    public void GenerateDecouplingChart_Acceptable_ReturnsCorrectRating()
    {
        // Arrange - 5-10% decoupling
        double firstHalfRatio = 1.5;
        double secondHalfRatio = 1.41;
        double decouplingPercent = 6.0;

        // Act
        var chart = ChartService.GenerateDecouplingChart(firstHalfRatio, secondHalfRatio, decouplingPercent);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Acceptable", chart);
        Assert.Contains("○", chart);
    }

    [Fact]
    public void GenerateDecouplingChart_NeedsWork_ReturnsCorrectRating()
    {
        // Arrange - More than 10% decoupling
        double firstHalfRatio = 1.5;
        double secondHalfRatio = 1.3;
        double decouplingPercent = 13.3;

        // Act
        var chart = ChartService.GenerateDecouplingChart(firstHalfRatio, secondHalfRatio, decouplingPercent);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Needs work", chart);
        Assert.Contains("⚠", chart);
        Assert.Contains("Z2 endurance", chart);
    }

    [Fact]
    public void GenerateRideSummary_WithValidActivity_ReturnsFormattedBox()
    {
        // Arrange
        var activity = new ApiActivity
        {
            Name = "Test Ride",
            MovingTime = 3600,
            Distance = 40000,
            AverageWatts = 200,
            NormalizedPower = 210,
            AverageHr = 145,
            MaxHr = 165
        };
        int? ftp = 250;

        // Act
        var chart = ChartService.GenerateRideSummary(activity, ftp);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Test Ride", chart);
        Assert.Contains("┌", chart); // Box drawing characters
        Assert.Contains("└", chart);
        Assert.Contains("200W", chart);
        Assert.Contains("210W", chart);
    }

    [Fact]
    public void GenerateZoneDistributionChart_EmptyZones_HandlesGracefully()
    {
        // Arrange
        var zoneTimes = new Dictionary<int, double>();
        var totalSeconds = 3600;
        var zoneNames = new[] { "Z1", "Z2", "Z3", "Z4", "Z5" };

        // Act
        var chart = ChartService.GenerateZoneDistributionChart(zoneTimes, totalSeconds, zoneNames);

        // Assert
        Assert.NotNull(chart);
        Assert.Contains("Time in Power Zones", chart);
    }

    [Fact]
    public void GeneratePowerCurveChart_NullOrEmpty_ReturnsEmpty()
    {
        // Act
        var chart1 = ChartService.GeneratePowerCurveChart(null!, 250);
        var chart2 = ChartService.GeneratePowerCurveChart(new Dictionary<string, int>(), 250);

        // Assert
        Assert.Equal(string.Empty, chart1);
        Assert.Equal(string.Empty, chart2);
    }

    [Fact]
    public void GenerateIntervalTimeline_NullOrEmpty_ReturnsEmpty()
    {
        // Act
        var chart1 = ChartService.GenerateIntervalTimeline(null!, 3600);
        var chart2 = ChartService.GenerateIntervalTimeline(new List<DetectedInterval>(), 3600);

        // Assert
        Assert.Equal(string.Empty, chart1);
        Assert.Equal(string.Empty, chart2);
    }
}
