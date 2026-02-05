# Cycling Coach Skill for Claude Code

AI-powered cycling coach that analyzes training data from intervals.icu. Get intelligent coaching insights based on power/heart rate relationships, fitness trends, and training load.

## Features

- **Activity Analysis** - Analyze workouts for power/HR coupling, zone distribution, and aerobic decoupling
- **Fitness Trends** - Review 30-day CTL/ATL/TSB progression to assess training load
- **Profile Management** - Fetch and understand your baseline (FTP, zones, fitness metrics)
- **Conversational Coaching** - Discuss training after analysis, answer follow-up questions

## Installation

### 1. Install the Skill

Copy this directory to your Claude Code skills folder:

```bash
cp -r cycling-coach ~/.claude/skills/
```

Or clone from a repository:

```bash
git clone <repo-url> ~/.claude/skills/cycling-coach
```

### 2. Get Your intervals.icu Credentials

You need two pieces of information:

**API Key:**
1. Log in to [intervals.icu](https://intervals.icu)
2. Go to Settings → Developer Settings
3. Generate or copy your API key

**Athlete ID:**
1. Go to your profile page
2. Look at the URL: `https://intervals.icu/athletes/i12345`
3. Your athlete ID is the part starting with 'i' (e.g., `i12345`)

### 3. Configure Credentials

Choose one of three methods:

#### Option A: Environment Variables (Recommended)

Add to your `~/.bashrc`, `~/.zshrc`, or `~/.profile`:

```bash
export INTERVALS_ICU_API_KEY="your-api-key-here"
export INTERVALS_ICU_ATHLETE_ID="i12345"
```

Then reload your shell:
```bash
source ~/.bashrc  # or ~/.zshrc
```

#### Option B: Use Setup Command

Start Claude Code and run:
```
/coach setup
```

Follow the prompts to enter your credentials.

#### Option C: Manual Config File

Copy the example config:
```bash
cp ~/.claude/skills/cycling-coach/.config.json.example ~/.claude/skills/cycling-coach/.config.json
```

Edit the file with your credentials:
```json
{
  "apiKey": "your-actual-api-key",
  "athleteId": "i12345"
}
```

Set secure permissions:
```bash
chmod 600 ~/.claude/skills/cycling-coach/.config.json
```

## Usage

Start Claude Code and use these commands:

### Setup and Profile

```
/coach setup          # Configure your API credentials
/coach profile        # View your athlete profile and zones
```

### Activity Analysis

```
/coach analyze        # Analyze your last ride
/coach analyze 123456 # Analyze specific activity by ID
```

You'll get:
- Zone distribution (power and heart rate)
- Aerobic decoupling analysis
- Sustained intervals detected
- Power/HR coupling assessment
- Coaching recommendations

### Fitness Trends

```
/coach trends         # Show 30-day fitness trends
/coach trends 60      # Show 60-day trends
```

You'll get:
- CTL/ATL/TSB progression
- Training load patterns
- Fitness direction assessment
- Strategic recommendations

### General Coaching

```
/coach                # Start a coaching conversation
```

Then ask questions like:
- "Should I do another FTP test?"
- "What should I focus on this week?"
- "Why was my heart rate so high on that ride?"

## Follow-up Questions

After any analysis, you can ask follow-up questions conversationally:

```
User: /coach analyze
[Analysis is displayed]

User: What does this mean for my FTP?
[Claude interprets power data and suggests test timing]

User: Should I take a recovery day?
[Claude references your TSB and recent load]
```

## Requirements

- **Node.js** - The skill uses Node.js scripts (built-in modules only, no dependencies)
- **intervals.icu account** - Free account with API access
- **Claude Code** - Latest version

## Troubleshooting

### "Configuration not found"

Run `/coach setup` or set environment variables as described above.

### "Authentication failed"

Double-check your API key and athlete ID:
- API key should be a long alphanumeric string
- Athlete ID should start with 'i' (e.g., i12345)

### "No recent activities found"

Make sure you have:
1. Uploaded rides to intervals.icu
2. Synced from Garmin/Strava/etc.
3. Waited a few minutes for processing

### "Network error"

Check your internet connection and try again. If the issue persists, intervals.icu might be experiencing downtime.

## Understanding the Metrics

### CTL (Chronic Training Load)
Your fitness level - 42-day rolling average of training stress. Higher = more fitness.

### ATL (Acute Training Load)
Your fatigue level - 7-day rolling average of training stress. Higher = more recent fatigue.

### TSB (Training Stress Balance)
Your form/freshness - CTL minus ATL. Positive = fresh, negative = fatigued.

### Aerobic Decoupling
Compares cardiac drift between first and second half of a ride:
- <5%: Excellent aerobic fitness
- 5-10%: Acceptable
- >10%: Needs attention (fatigue or aerobic base development needed)

### Power Zones
Based on your FTP (Functional Threshold Power):
- Z1: <55% FTP (Recovery)
- Z2: 55-75% FTP (Endurance)
- Z3: 75-90% FTP (Tempo)
- Z4: 90-105% FTP (Threshold)
- Z5: >105% FTP (VO2 max)

## Privacy & Security

- Your API credentials are stored locally on your machine
- Config file is set to read/write for owner only (chmod 600)
- No data is sent anywhere except intervals.icu
- Consider using environment variables for added security

## Distribution

This skill is designed to be easily shared:
1. Anyone can copy the directory to their skills folder
2. Each user configures with their own intervals.icu credentials
3. No athlete-specific data is hardcoded in the skill

## License

MIT License - See LICENSE.txt

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Verify your credentials and internet connection
3. Check intervals.icu API status
4. Open an issue in the GitHub repository

## Credits

Created for Claude Code by the cycling community.

Powered by [intervals.icu](https://intervals.icu) - an amazing training platform for cyclists and triathletes.
