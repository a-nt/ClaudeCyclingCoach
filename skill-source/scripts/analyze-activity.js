#!/usr/bin/env node

/**
 * Activity Analysis Script
 *
 * Performs calculations on cycling activity data from intervals.icu.
 * Calculates zone distribution, decoupling, interval detection, and power/HR relationships.
 *
 * Usage:
 *   echo '{"activity": {...}, "profile": {...}}' | node analyze-activity.js
 */

const fs = require('fs');

/**
 * Read input from stdin
 */
function readStdin() {
  return new Promise((resolve, reject) => {
    let data = '';
    process.stdin.on('data', chunk => data += chunk);
    process.stdin.on('end', () => {
      try {
        resolve(JSON.parse(data));
      } catch (err) {
        reject(new Error(`Failed to parse input JSON: ${err.message}`));
      }
    });
    process.stdin.on('error', reject);
  });
}

/**
 * Calculate zone distribution from time-in-zone data
 */
function calculateZoneDistribution(zoneTimesArray, totalTime) {
  if (!zoneTimesArray || zoneTimesArray.length === 0) {
    return null;
  }

  return zoneTimesArray.map((seconds, index) => ({
    zone: index + 1,
    seconds: seconds,
    percentage: totalTime > 0 ? (seconds / totalTime * 100).toFixed(1) : 0
  }));
}

/**
 * Calculate aerobic decoupling
 * Compares first half vs second half HR at same power
 */
function calculateDecoupling(streams) {
  if (!streams || !streams.watts || !streams.heartrate) {
    return null;
  }

  const watts = streams.watts;
  const hr = streams.heartrate;
  const length = Math.min(watts.length, hr.length);

  if (length < 600) { // Need at least 10 minutes
    return null;
  }

  const midpoint = Math.floor(length / 2);

  // Calculate average power and HR for first and second half
  let firstHalfPower = 0, firstHalfHr = 0;
  let secondHalfPower = 0, secondHalfHr = 0;
  let firstCount = 0, secondCount = 0;

  for (let i = 0; i < midpoint; i++) {
    if (watts[i] > 0 && hr[i] > 0) {
      firstHalfPower += watts[i];
      firstHalfHr += hr[i];
      firstCount++;
    }
  }

  for (let i = midpoint; i < length; i++) {
    if (watts[i] > 0 && hr[i] > 0) {
      secondHalfPower += watts[i];
      secondHalfHr += hr[i];
      secondCount++;
    }
  }

  if (firstCount === 0 || secondCount === 0) {
    return null;
  }

  const avgFirstPower = firstHalfPower / firstCount;
  const avgFirstHr = firstHalfHr / firstCount;
  const avgSecondPower = secondHalfPower / secondCount;
  const avgSecondHr = secondHalfHr / secondCount;

  // Calculate efficiency (watts per bpm)
  const firstEfficiency = avgFirstPower / avgFirstHr;
  const secondEfficiency = avgSecondPower / avgSecondHr;

  // Decoupling percentage (positive = worse efficiency in second half)
  const decoupling = ((avgFirstHr / avgFirstPower) - (avgSecondHr / avgSecondPower)) / (avgFirstHr / avgFirstPower) * 100;

  return {
    firstHalf: {
      avgPower: Math.round(avgFirstPower),
      avgHr: Math.round(avgFirstHr),
      efficiency: firstEfficiency.toFixed(2)
    },
    secondHalf: {
      avgPower: Math.round(avgSecondPower),
      avgHr: Math.round(avgSecondHr),
      efficiency: secondEfficiency.toFixed(2)
    },
    decouplingPercent: decoupling.toFixed(1),
    interpretation: interpretDecoupling(decoupling)
  };
}

/**
 * Interpret decoupling percentage
 */
function interpretDecoupling(percent) {
  const absPercent = Math.abs(percent);
  if (absPercent < 5) {
    return 'Excellent - minimal decoupling';
  } else if (absPercent < 10) {
    return 'Good - acceptable decoupling';
  } else {
    return 'Significant - may indicate fatigue or insufficient aerobic fitness';
  }
}

/**
 * Detect sustained intervals above threshold
 */
function detectIntervals(streams, ftp) {
  if (!streams || !streams.watts || !ftp) {
    return null;
  }

  const watts = streams.watts;
  const threshold = ftp * 0.95; // 95% of FTP
  const minDuration = 120; // 2 minutes minimum

  const intervals = [];
  let intervalStart = null;
  let intervalSum = 0;
  let intervalCount = 0;

  for (let i = 0; i < watts.length; i++) {
    if (watts[i] >= threshold) {
      if (intervalStart === null) {
        intervalStart = i;
      }
      intervalSum += watts[i];
      intervalCount++;
    } else {
      if (intervalStart !== null && intervalCount >= minDuration) {
        intervals.push({
          startTime: intervalStart,
          duration: intervalCount,
          avgPower: Math.round(intervalSum / intervalCount),
          percentFtp: ((intervalSum / intervalCount) / ftp * 100).toFixed(1)
        });
      }
      intervalStart = null;
      intervalSum = 0;
      intervalCount = 0;
    }
  }

  // Check if there's an ongoing interval at the end
  if (intervalStart !== null && intervalCount >= minDuration) {
    intervals.push({
      startTime: intervalStart,
      duration: intervalCount,
      avgPower: Math.round(intervalSum / intervalCount),
      percentFtp: ((intervalSum / intervalCount) / ftp * 100).toFixed(1)
    });
  }

  return intervals;
}

/**
 * Calculate overall power/HR relationship
 */
function calculatePowerHrRelationship(activity) {
  if (!activity.averagePower || !activity.averageHr) {
    return null;
  }

  const efficiency = activity.averagePower / activity.averageHr;

  return {
    avgPower: activity.averagePower,
    avgHr: activity.averageHr,
    wattsPerBpm: efficiency.toFixed(2),
    interpretation: interpretEfficiency(efficiency, activity.averagePower)
  };
}

/**
 * Interpret watts per bpm efficiency
 */
function interpretEfficiency(efficiency, power) {
  if (power < 100) {
    return 'Power too low for meaningful efficiency analysis';
  } else if (efficiency > 1.5) {
    return 'Excellent efficiency - strong aerobic base';
  } else if (efficiency > 1.2) {
    return 'Good efficiency';
  } else if (efficiency > 0.9) {
    return 'Moderate efficiency - room for improvement';
  } else {
    return 'Low efficiency - consider aerobic base development';
  }
}

/**
 * Format duration in seconds to human-readable format
 */
function formatDuration(seconds) {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;

  if (hours > 0) {
    return `${hours}h ${minutes}m ${secs}s`;
  } else if (minutes > 0) {
    return `${minutes}m ${secs}s`;
  } else {
    return `${secs}s`;
  }
}

/**
 * Main analysis function
 */
async function analyze() {
  try {
    const input = await readStdin();
    const activity = input.activity;
    const profile = input.profile;

    if (!activity) {
      throw new Error('No activity data provided');
    }

    const analysis = {
      activity: {
        id: activity.id,
        name: activity.name,
        date: activity.startDate,
        type: activity.type,
        duration: formatDuration(activity.movingTime || activity.elapsedTime),
        distance: activity.distance ? `${(activity.distance / 1000).toFixed(2)} km` : null,
        averagePower: activity.averagePower,
        normalizedPower: activity.normalizedPower,
        intensityFactor: activity.intensityFactor,
        trainingStress: activity.trainingStress,
        averageHr: activity.averageHr,
        maxHr: activity.maxHr,
        averageCadence: activity.averageCadence,
        efficiency: activity.efficiency
      },
      zoneDistribution: null,
      decoupling: null,
      intervals: null,
      powerHrRelationship: null
    };

    // Zone distribution
    if (activity.icu_zone_times) {
      analysis.zoneDistribution = {
        power: calculateZoneDistribution(activity.icu_zone_times, activity.movingTime || activity.elapsedTime)
      };
    }

    // Decoupling analysis
    if (activity.streams) {
      analysis.decoupling = calculateDecoupling(activity.streams);
    }

    // Interval detection
    if (activity.streams && profile && profile.ftp) {
      analysis.intervals = detectIntervals(activity.streams, profile.ftp);
    }

    // Power/HR relationship
    analysis.powerHrRelationship = calculatePowerHrRelationship(activity);

    console.log(JSON.stringify({
      success: true,
      data: analysis
    }, null, 2));

  } catch (err) {
    console.error(JSON.stringify({
      success: false,
      error: err.message
    }, null, 2));
    process.exit(1);
  }
}

// Run if called directly
if (require.main === module) {
  analyze();
}
