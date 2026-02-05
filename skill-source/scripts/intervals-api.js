#!/usr/bin/env node

/**
 * intervals.icu API Client
 *
 * Fetches cycling training data from intervals.icu.
 * Uses only Node.js built-in modules (no external dependencies).
 *
 * Usage:
 *   node intervals-api.js profile
 *   node intervals-api.js activities --limit 10
 *   node intervals-api.js activity <id>
 *   node intervals-api.js wellness --days 30
 */

const https = require('https');
const fs = require('fs');
const path = require('path');

const API_BASE = 'intervals.icu';
const CONFIG_PATH = path.join(process.env.HOME, '.claude', 'skills', 'cycling-coach', '.config.json');

/**
 * Load configuration from environment variables or .config.json
 */
function loadConfig() {
  let apiKey = process.env.INTERVALS_ICU_API_KEY;
  let athleteId = process.env.INTERVALS_ICU_ATHLETE_ID;

  // Fallback to config file if env vars not set
  if (!apiKey || !athleteId) {
    try {
      if (fs.existsSync(CONFIG_PATH)) {
        const config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
        apiKey = apiKey || config.apiKey;
        athleteId = athleteId || config.athleteId;
      }
    } catch (err) {
      // Config file doesn't exist or is invalid
    }
  }

  if (!apiKey || !athleteId) {
    console.error(JSON.stringify({
      error: 'Configuration missing',
      message: 'Please set INTERVALS_ICU_API_KEY and INTERVALS_ICU_ATHLETE_ID environment variables, or run /coach setup',
      details: {
        hasApiKey: !!apiKey,
        hasAthleteId: !!athleteId,
        configPath: CONFIG_PATH
      }
    }, null, 2));
    process.exit(1);
  }

  return { apiKey, athleteId };
}

/**
 * Make HTTPS request to intervals.icu API
 */
function makeRequest(path, accept = 'application/json') {
  const config = loadConfig();
  const auth = Buffer.from(`API_KEY:${config.apiKey}`).toString('base64');

  return new Promise((resolve, reject) => {
    const options = {
      hostname: API_BASE,
      path: path,
      method: 'GET',
      headers: {
        'Authorization': `Basic ${auth}`,
        'Accept': accept
      }
    };

    const req = https.request(options, (res) => {
      let data = '';

      res.on('data', (chunk) => {
        data += chunk;
      });

      res.on('end', () => {
        if (res.statusCode === 200) {
          try {
            if (accept === 'text/csv') {
              resolve(data);
            } else {
              resolve(JSON.parse(data));
            }
          } catch (err) {
            reject(new Error(`Failed to parse response: ${err.message}`));
          }
        } else if (res.statusCode === 401) {
          reject(new Error('Authentication failed. Please check your API key and athlete ID.'));
        } else if (res.statusCode === 404) {
          reject(new Error('Resource not found. Please check the activity ID or athlete ID.'));
        } else if (res.statusCode === 429) {
          reject(new Error('Rate limit exceeded. Please try again later.'));
        } else {
          reject(new Error(`API request failed with status ${res.statusCode}: ${data}`));
        }
      });
    });

    req.on('error', (err) => {
      reject(new Error(`Network error: ${err.message}`));
    });

    req.setTimeout(10000, () => {
      req.destroy();
      reject(new Error('Request timeout'));
    });

    req.end();
  });
}

/**
 * Fetch athlete profile
 */
async function getProfile() {
  const config = loadConfig();
  try {
    const profile = await makeRequest(`/api/v1/athlete/${config.athleteId}`);

    // Get cycling sport settings (first one with "Ride" type)
    const cyclingSettings = profile.sportSettings?.find(s =>
      s.types?.includes('Ride') || s.types?.includes('VirtualRide')
    );

    return {
      success: true,
      data: {
        name: profile.name,
        athleteId: profile.id,
        ftp: cyclingSettings?.ftp || null,
        indoorFtp: cyclingSettings?.indoor_ftp || null,
        ftpWattsPerKg: cyclingSettings?.ftp && profile.icu_weight
          ? (cyclingSettings.ftp / profile.icu_weight).toFixed(2)
          : null,
        weight: profile.icu_weight || profile.weight || null,
        maxHr: cyclingSettings?.max_hr || null,
        restingHr: profile.icu_resting_hr || cyclingSettings?.resting_hr || null,
        ltHr: cyclingSettings?.lthr || null,
        powerZones: cyclingSettings?.power_zones || null,
        powerZoneNames: cyclingSettings?.power_zone_names || null,
        hrZones: cyclingSettings?.hr_zones || null,
        hrZoneNames: cyclingSettings?.hr_zone_names || null,
        ctl: profile.ctl || null,
        atl: profile.atl || null,
        tsb: profile.rr || null
      }
    };
  } catch (err) {
    return {
      success: false,
      error: err.message
    };
  }
}

/**
 * Fetch recent activities list
 */
async function getActivities(limit = 10) {
  const config = loadConfig();
  try {
    // intervals.icu activities endpoint requires date range
    const newest = new Date();
    const oldest = new Date();
    oldest.setDate(oldest.getDate() - 90); // Get last 90 days

    const oldestStr = oldest.toISOString().split('T')[0];
    const newestStr = newest.toISOString().split('T')[0];

    // Fetch as JSON (the endpoint returns JSON array)
    const activities = await makeRequest(
      `/api/v1/athlete/${config.athleteId}/activities?oldest=${oldestStr}&newest=${newestStr}`
    );

    // Return limited number of most recent activities
    const limitedActivities = activities.slice(0, limit).map(activity => ({
      id: activity.id,
      name: activity.name,
      start_date_local: activity.start_date_local,
      type: activity.type,
      distance: activity.distance,
      moving_time: activity.moving_time,
      average_watts: activity.average_watts,
      average_hr: activity.average_hr,
      icu_training_load: activity.icu_training_load
    }));

    return {
      success: true,
      data: limitedActivities
    };
  } catch (err) {
    return {
      success: false,
      error: err.message
    };
  }
}

/**
 * Fetch specific activity with power/HR streams
 */
async function getActivity(activityId) {
  const config = loadConfig();
  try {
    const activity = await makeRequest(`/api/v1/activity/${activityId}`);

    // Return the full activity object with standardized field names
    return {
      success: true,
      data: {
        id: activity.id,
        name: activity.name || activity.description || 'Unnamed Activity',
        startDate: activity.start_date_local || activity.start_date,
        type: activity.type,
        distance: activity.distance,
        movingTime: activity.moving_time,
        elapsedTime: activity.elapsed_time,
        averagePower: activity.average_watts || activity.avg_watts,
        normalizedPower: activity.np || activity.normalized_power,
        intensityFactor: activity.icu_if || activity.intensity_factor,
        trainingStress: activity.icu_training_load || activity.training_load || activity.tss,
        variabilityIndex: activity.variability_index || activity.vi,
        efficiency: activity.efficiency_factor || activity.ef,
        averageHr: activity.average_hr || activity.avg_hr,
        maxHr: activity.max_hr,
        averageCadence: activity.average_cadence || activity.avg_cadence,
        powerCurve: activity.power_curve,
        hrCurve: activity.hr_curve,
        streams: activity.streams,
        intervals: activity.intervals,
        icu_zone_times: activity.icu_zone_times,
        // Include raw activity for debugging
        _raw: activity
      }
    };
  } catch (err) {
    return {
      success: false,
      error: err.message
    };
  }
}

/**
 * Fetch wellness/fitness data for date range
 */
async function getWellness(days = 30) {
  const config = loadConfig();
  try {
    const newest = new Date();
    const oldest = new Date();
    oldest.setDate(oldest.getDate() - days);

    const oldestStr = oldest.toISOString().split('T')[0];
    const newestStr = newest.toISOString().split('T')[0];

    const wellness = await makeRequest(
      `/api/v1/athlete/${config.athleteId}/wellness?oldest=${oldestStr}&newest=${newestStr}`
    );

    return {
      success: true,
      data: wellness.map(w => ({
        date: w.id,
        ctl: w.ctl,
        atl: w.atl,
        tsb: w.rr,
        weight: w.weight,
        restingHr: w.restingHR,
        hrv: w.hrv,
        mentalEnergy: w.mentalEnergy,
        sleepQuality: w.sleepQuality,
        sleepHours: w.sleepHours
      }))
    };
  } catch (err) {
    return {
      success: false,
      error: err.message
    };
  }
}

/**
 * Main CLI handler
 */
async function main() {
  const args = process.argv.slice(2);
  const command = args[0];

  let result;

  switch (command) {
    case 'profile':
      result = await getProfile();
      break;

    case 'activities':
      const limitIndex = args.indexOf('--limit');
      const limit = limitIndex !== -1 ? parseInt(args[limitIndex + 1]) : 10;
      result = await getActivities(limit);
      break;

    case 'activity':
      if (!args[1]) {
        result = {
          success: false,
          error: 'Activity ID required. Usage: node intervals-api.js activity <id>'
        };
      } else {
        result = await getActivity(args[1]);
      }
      break;

    case 'wellness':
      const daysIndex = args.indexOf('--days');
      const days = daysIndex !== -1 ? parseInt(args[daysIndex + 1]) : 30;
      result = await getWellness(days);
      break;

    default:
      result = {
        success: false,
        error: `Unknown command: ${command}\n\nAvailable commands:\n  profile\n  activities --limit <n>\n  activity <id>\n  wellness --days <n>`
      };
  }

  console.log(JSON.stringify(result, null, 2));
  process.exit(result.success ? 0 : 1);
}

// Run if called directly
if (require.main === module) {
  main().catch(err => {
    console.error(JSON.stringify({
      success: false,
      error: err.message
    }, null, 2));
    process.exit(1);
  });
}
