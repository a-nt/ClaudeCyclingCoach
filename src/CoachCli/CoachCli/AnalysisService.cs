namespace CoachCli;

public class AnalysisResult
{
    public required object Activity { get; set; }
    public object? Decoupling { get; set; }
    public object? Intervals { get; set; }
    public object? PowerHrRelationship { get; set; }
}

public static class AnalysisService
{
    public static AnalysisResult Analyze(ApiActivity activity, ApiProfile? profile)
    {
        var result = new AnalysisResult
        {
            Activity = new
            {
                id = activity.Id,
                name = activity.Name,
                date = activity.StartDateLocal ?? activity.StartDate,
                type = activity.Type,
                distance = activity.Distance,
                duration = activity.ElapsedTime ?? activity.MovingTime,
                movingTime = activity.MovingTime,
                averagePower = activity.GetAveragePower(),
                normalizedPower = activity.GetNormalizedPower(),
                intensityFactor = activity.GetIntensityFactor(),
                variabilityIndex = activity.GetVariabilityIndex(),
                trainingLoad = activity.GetTrainingLoad(),
                averageHr = activity.GetAverageHr(),
                maxHr = activity.MaxHr,
                averageCadence = activity.AverageCadence ?? activity.AvgCadence,
                efficiencyFactor = activity.GetEfficiencyFactor(),
                powerCurve = activity.PowerCurve,
                zoneTime = activity.IcuZoneTimes
            }
        };

        // Calculate decoupling if streams available
        if (activity.Streams != null)
        {
            result.Decoupling = CalculateDecoupling(activity.Streams);
        }

        // Detect intervals if FTP available
        if (activity.Streams != null && profile != null)
        {
            var cyclingSetting = profile.SportSettings?
                .FirstOrDefault(s => s.Types?.Contains("Ride") == true || s.Types?.Contains("VirtualRide") == true);

            if (cyclingSetting?.Ftp != null)
            {
                result.Intervals = DetectIntervals(activity.Streams, cyclingSetting.Ftp.Value);
            }
        }

        // Calculate power/HR relationship
        result.PowerHrRelationship = CalculatePowerHrRelationship(activity);

        return result;
    }

    private static object? CalculateDecoupling(ApiStreams streams)
    {
        if (streams.Watts == null || streams.Heartrate == null)
        {
            return null;
        }

        var watts = streams.Watts;
        var hr = streams.Heartrate;
        var length = Math.Min(watts.Length, hr.Length);

        if (length < 600) // Need at least 10 minutes
        {
            return null;
        }

        var midpoint = length / 2;

        // Calculate average power and HR for first and second half
        double firstHalfPower = 0, firstHalfHr = 0;
        double secondHalfPower = 0, secondHalfHr = 0;
        int firstCount = 0, secondCount = 0;

        for (int i = 0; i < midpoint; i++)
        {
            if (watts[i] > 0 && hr[i] > 0)
            {
                firstHalfPower += watts[i];
                firstHalfHr += hr[i];
                firstCount++;
            }
        }

        for (int i = midpoint; i < length; i++)
        {
            if (watts[i] > 0 && hr[i] > 0)
            {
                secondHalfPower += watts[i];
                secondHalfHr += hr[i];
                secondCount++;
            }
        }

        if (firstCount == 0 || secondCount == 0)
        {
            return null;
        }

        var avgFirstPower = firstHalfPower / firstCount;
        var avgFirstHr = firstHalfHr / firstCount;
        var avgSecondPower = secondHalfPower / secondCount;
        var avgSecondHr = secondHalfHr / secondCount;

        // Calculate efficiency (watts per bpm)
        var firstEfficiency = avgFirstPower / avgFirstHr;
        var secondEfficiency = avgSecondPower / avgSecondHr;

        // Decoupling percentage (positive = worse efficiency in second half)
        var decoupling = ((avgFirstHr / avgFirstPower) - (avgSecondHr / avgSecondPower)) / (avgFirstHr / avgFirstPower) * 100;

        return new
        {
            firstHalf = new
            {
                avgPower = (int)Math.Round(avgFirstPower),
                avgHr = (int)Math.Round(avgFirstHr),
                efficiency = firstEfficiency.ToString("F2")
            },
            secondHalf = new
            {
                avgPower = (int)Math.Round(avgSecondPower),
                avgHr = (int)Math.Round(avgSecondHr),
                efficiency = secondEfficiency.ToString("F2")
            },
            decouplingPercent = decoupling.ToString("F1"),
            interpretation = InterpretDecoupling(decoupling)
        };
    }

    private static string InterpretDecoupling(double percent)
    {
        var absPercent = Math.Abs(percent);
        if (absPercent < 5)
        {
            return "Excellent - minimal decoupling";
        }
        else if (absPercent < 10)
        {
            return "Good - acceptable decoupling";
        }
        else
        {
            return "Significant - may indicate fatigue or insufficient aerobic fitness";
        }
    }

    private static object? DetectIntervals(ApiStreams streams, int ftp)
    {
        if (streams.Watts == null)
        {
            return null;
        }

        var watts = streams.Watts;
        var threshold = ftp * 0.95; // 95% of FTP
        var minDuration = 120; // 2 minutes minimum

        var intervals = new List<object>();
        int? intervalStart = null;
        double intervalSum = 0;
        int intervalCount = 0;

        for (int i = 0; i < watts.Length; i++)
        {
            if (watts[i] >= threshold)
            {
                if (intervalStart == null)
                {
                    intervalStart = i;
                }
                intervalSum += watts[i];
                intervalCount++;
            }
            else
            {
                if (intervalStart != null && intervalCount >= minDuration)
                {
                    intervals.Add(new
                    {
                        startTime = intervalStart.Value,
                        duration = intervalCount,
                        avgPower = (int)Math.Round(intervalSum / intervalCount),
                        percentFtp = (intervalSum / intervalCount / ftp * 100).ToString("F1")
                    });
                }
                intervalStart = null;
                intervalSum = 0;
                intervalCount = 0;
            }
        }

        // Check if there's an ongoing interval at the end
        if (intervalStart != null && intervalCount >= minDuration)
        {
            intervals.Add(new
            {
                startTime = intervalStart.Value,
                duration = intervalCount,
                avgPower = (int)Math.Round(intervalSum / intervalCount),
                percentFtp = (intervalSum / intervalCount / ftp * 100).ToString("F1")
            });
        }

        return intervals;
    }

    private static object? CalculatePowerHrRelationship(ApiActivity activity)
    {
        var avgPower = activity.GetAveragePower();
        var avgHr = activity.GetAverageHr();

        if (avgPower == null || avgHr == null)
        {
            return null;
        }

        var efficiency = avgPower.Value / avgHr.Value;

        return new
        {
            avgPower = avgPower.Value,
            avgHr = avgHr.Value,
            wattsPerBpm = efficiency.ToString("F2"),
            interpretation = InterpretEfficiency(efficiency, avgPower.Value)
        };
    }

    private static string InterpretEfficiency(double efficiency, double power)
    {
        if (power < 100)
        {
            return "Power too low for meaningful efficiency analysis";
        }
        else if (efficiency > 1.5)
        {
            return "Excellent efficiency - strong aerobic base";
        }
        else if (efficiency > 1.2)
        {
            return "Good efficiency";
        }
        else if (efficiency > 0.9)
        {
            return "Moderate efficiency - room for improvement";
        }
        else
        {
            return "Low efficiency - consider aerobic base development";
        }
    }
}
