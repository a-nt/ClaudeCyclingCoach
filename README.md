# 🚴 Cycling Coach - AI Training Analysis Skill for Claude Code

> **Intelligent cycling coach powered by Claude AI that analyzes your training data from intervals.icu**

Transform your training data into actionable coaching insights with power/heart rate analysis, fitness trends, and personalized recommendations.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![intervals.icu](https://img.shields.io/badge/intervals.icu-integrated-blue.svg)](https://intervals.icu)

## ✨ Features

- 🎯 **Smart Check-ins** - Holistic coaching that analyzes recent training and provides personalized next-step recommendations
- 📈 **Activity Analysis** - Deep dive into power/HR coupling, zone distribution, aerobic decoupling, VI, and EF
- 📊 **Fitness Trends** - Track CTL/ATL/TSB progression with research-based interpretation and ramp rate monitoring
- 🧠 **Context-Aware** - Remembers your goals, training phase, weekly hours, and philosophy for tailored advice
- 🎨 **Training Philosophy** - Choose from Polarized (80/20), Sweet Spot, Traditional, or define your own
- 💡 **Power Curve Analysis** - FTP estimation from 20-min power, rider profiling, and automatic test recommendations
- 👤 **Profile Management** - View your FTP, zones, and key metrics at a glance
- 💬 **Conversational Coaching** - Ask follow-up questions and get personalized advice
- 🔒 **Privacy First** - Your data stays on your machine, no cloud processing
- 🚀 **Minimal Dependencies** - Built with .NET 10, Spectre.Console for UI, System.CommandLine for CLI
- ⚡ **Type-Safe** - C# implementation with strict models for reliable API parsing

## 🎯 Quick Start

### Prerequisites

- [.NET 10 Runtime](https://dotnet.microsoft.com/download) installed
- [Claude Code](https://claude.ai/download) CLI tool
- [intervals.icu](https://intervals.icu) account with API access

**⚠️ Important: Data Quality**

This skill works best with activities uploaded **directly** to intervals.icu from:
- Wahoo head units
- Garmin devices (via Garmin Connect sync to intervals.icu)
- Zwift
- TrainerRoad
- Other platforms that provide full power/HR streams

**Strava-synced activities have limited data** due to Strava's API restrictions. Activities synced through Strava will show basic metrics but lack the detailed second-by-second power and heart rate streams needed for:
- Aerobic decoupling analysis
- Variability Index (VI) calculations
- Interval detection
- Detailed zone distribution

For best results, configure your device or platform to sync directly to intervals.icu.

### Installation

```bash
# Clone the repository
git clone https://github.com/a-nt/ClaudeCyclingCoach.git
cd ClaudeCyclingCoach

# Symlink repository root to Claude Code skills directory
ln -s "$(pwd)" ~/.claude/skills/coach

# Or copy directly
cp -r . ~/.claude/skills/coach
```

### Configuration

Start Claude Code and run:

```
/coach setup
```

Follow the prompts to enter your intervals.icu credentials:
1. **API Key**: Get from intervals.icu → Settings → Developer Settings
2. **Athlete ID**: Found in your profile URL (format: `i12345`)

## 🎮 Commands

| Command | Description |
|---------|-------------|
| `/coach setup` | Configure your intervals.icu credentials |
| `/coach context` | Set training context (goals, hours, phase) with smart inference ⭐ |
| `/coach philosophy` | Choose your training approach (Polarized, Sweet Spot, etc.) |
| `/coach profile` | View your athlete profile and training zones |
| `/coach check-in` | Get personalized recommendations based on recent training ⭐ NEW |
| `/coach analyze [id]` | Analyze your last ride or a specific activity |
| `/coach trends [days]` | Show fitness trends (default: 30 days) |
| `/coach` | Start a general coaching conversation |

## 📸 Example Usage

### Check-in (Personalized Coaching)

```
You: /coach check-in

Coach: Nice rhythm - you're averaging 6.5 hours weekly with CTL climbing from
38 to 42 over the past month. Form at -16 is optimal for building fitness. One
thing: 35% of your time is in Z3 "no man's land" which doesn't align with
Polarized. Next week: two 3-hour Z2 rides and one proper interval session (Z4-5).
Keep the hard days hard and easy days easy.

You: Why is Z3 bad?

Coach: Z3 is the "tempo" zone - hard enough to create fatigue but not hard enough
to drive VO2 max adaptations. With Polarized training, you want 80% easy (Z1-2)
and 20% hard (Z4-5). Z3 gives you the worst of both worlds - too hard to recover
from, too easy to build top-end fitness.
```

### Activity Analysis

```
You: /coach analyze

Coach: 1h32 threshold ride - solid 20-min power at 268W suggests FTP around 255W
(your current is 250W). VI of 1.03 shows excellent pacing. Decoupling only 2.1%
confirms strong aerobic base. Ready for an FTP test next week?

You: What does this mean for my FTP?

Coach: Your 20-min power of 268W × 0.95 = 255W estimated FTP, which is 5W higher
than your current 250W setting. Since you've been building fitness (CTL 38→42)
and this wasn't a maximal test, you're likely ready. Test when fresh - TSB around
+5 to +10 is ideal.
```

## 🧠 What Makes This Different

### Holistic Check-ins
- **Smart Pattern Detection** - Identifies if you're overreaching, coasting, detraining, or building well
- **Context-Aware Advice** - Considers your goals, training phase, and philosophy
- **No Questions Asked** - Analyzes data automatically and gives direct recommendations
- **Works Anytime** - Daily check-in or after months away, adjusts advice to your situation

### Comprehensive Metrics
- **Aerobic Decoupling** - Detects cardiac drift to assess aerobic fitness
- **Variability Index (VI)** - Measures pacing consistency (1.0 = perfect steady state)
- **Efficiency Factor (EF)** - Tracks aerobic efficiency trends over time (NP/avg HR)
- **Power Curve Analysis** - FTP estimation from 20-min power, rider profiling (sprinter vs TT)
- **Interval Detection** - Automatically identifies sustained threshold efforts
- **Zone Distribution** - Analyzes time in both power zones (7 zones) and HR zones (5 zones) with philosophy alignment
- **Color-Coded Charts** - ASCII visualizations with ANSI colors for power and HR zone distribution

### Training Load Management (Research-Based)
- **CTL (Chronic Training Load)** - 42-day fitness trend with safe ramp rate monitoring (5-8 TSS/week)
- **ATL (Acute Training Load)** - 7-day fatigue monitoring
- **TSB (Training Stress Balance)** - Evidence-based interpretation (-10 to -30 = optimal training zone)
- **Overtraining Detection** - Warns when ramping too fast (>10 TSS/week)
- **Philosophy Alignment** - Checks if training matches your chosen approach (Polarized, Sweet Spot, etc.)

### Conversational Context
- Ask follow-up questions naturally
- Maintains analysis context throughout conversation
- No unnecessary API calls or re-fetching
- Personalized coaching based on your goals, phase, and constraints

## 🏗️ Architecture

**Implementation: C# .NET 10** ✅

```
cycling-coach/                  # Root IS the skill (symlink to ~/.claude/skills/coach)
├── SKILL.md                    # Main skill definition (Claude reads this)
├── bin/
│   ├── coach-cli/              # C# compiled binaries
│   │   └── CoachCli.dll        # Main CLI application
│   └── coach-cli.sh            # Wrapper script
├── src/                        # C# source code (~880 LOC)
│   └── CoachCli/
│       ├── CoachCli/
│       │   ├── Program.cs              # CLI entry point
│       │   ├── Configuration.cs        # Config service
│       │   ├── Models.cs               # API models
│       │   ├── IntervalsApiClient.cs   # HTTP client
│       │   └── AnalysisService.cs      # Analysis engine
├── examples/
│   ├── activity-analysis.md    # Output template for activities
│   └── trend-report.md         # Output template for trends
├── README.md                   # User documentation
├── TESTING.md                  # Testing procedures
└── .config.json.example        # Config template (real .config.json created on setup)
```

**Why C#?**
- ✅ Type-safe API models prevent parsing errors
- ✅ Integrated analysis (single command vs piping)
- ✅ Better error handling and diagnostics
- ✅ Maintainable architecture
- ✅ ~250ms startup (acceptable for API-bound operations)

**UI Library:**
- 🎨 **Spectre.Console** for professional terminal output
- ✅ Automatic table formatting and alignment
- ✅ ANSI color support embedded in JSON output
- ✅ Color-coded power zones (grey→blue→green→yellow→orange→red)

**Deployment:** Root directory is symlinked to `~/.claude/skills/coach/`

## 🔒 Privacy & Security

- ✅ Credentials stored locally in `~/.claude/skills/coach/.config.json`
- ✅ File permissions set to `600` (owner read/write only)
- ✅ No data sent to external services (except intervals.icu API)
- ✅ Environment variables supported for extra security
- ✅ API keys never logged or displayed

## 📖 Understanding the Metrics

### Power Zones (based on FTP) - Color Coded! 🎨
- **Z1 (0-55%)** - 🔘 Grey - Recovery
- **Z2 (55-75%)** - 🔵 Blue - Endurance base (most time here for Polarized training)
- **Z3 (75-90%)** - 🟢 Green - Tempo ("no man's land" - avoid excessive time here)
- **Z4 (90-105%)** - 🟡 Yellow - Threshold
- **Z5 (105-120%)** - 🟠 Orange - VO2 max
- **Z6 (120-150%)** - 🔴 Red - Anaerobic
- **Z7 (>150%)** - 🔴 Red - Neuromuscular

### Heart Rate Zones (based on Max HR) - Color Coded! 🎨
- **Z1 (0-66%)** - 🔘 Grey - Recovery
- **Z2 (66-81%)** - 🔵 Blue - Aerobic/Endurance
- **Z3 (81-87%)** - 🟢 Green - Tempo
- **Z4 (87-93%)** - 🟡 Yellow - Threshold
- **Z5 (93-100%)** - 🔴 Red - Max effort

All power and HR charts include ANSI color codes for rich terminal display!

### Training Load (Research-Based Interpretation)
- **CTL** - Your fitness level (40-60 recreational, 80-100 competitive, 120+ elite)
- **ATL** - Your fatigue level (7-day average)
- **TSB** - Your form (CTL - ATL)
  - **-10 to -30**: OPTIMAL training zone (build fitness here!)
  - **-30 to -50**: Sustainable for high fitness athletes (CTL >100)
  - **+15 to +25**: Peak freshness for racing
  - **-10 to +10**: Transitional (should be brief)

**Safe CTL ramp rate**: 5-8 TSS/week (>10/week = injury risk)

### Efficiency Metrics
- **Aerobic Decoupling**: <5% excellent, 5-10% acceptable, >10% needs work
- **Variability Index (VI)**: <1.05 great pacing, >1.10 high variability
- **Efficiency Factor (EF)**: NP/avg HR - track trends over time (improving = better fitness)
- **Power Curve**: Best power at 5s, 1min, 5min, 20min (used for FTP estimation)

## 🛠️ Development

### Building from Source

```bash
# Build the C# project
cd src/CoachCli/CoachCli
dotnet build -c Release

# Publish to bin directory (skill root is symlinked)
dotnet publish -c Release -r osx-arm64 --self-contained false \
  -o ../../../bin/coach-cli
```

### Running Tests

```bash
# Run unit tests (recommended after any code changes)
./test.sh

# Or use dotnet directly
dotnet test

# Test CLI commands manually
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll profile
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activities --limit 5
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll wellness --days 30

# Or use wrapper script
~/.claude/skills/coach/bin/coach-cli.sh profile

# See TESTING.md for comprehensive test procedures
```

**Test Coverage:**
- ✅ Analysis calculations (decoupling, intervals, power/HR relationship)
- ✅ Chart generation (9 chart types: power zones, HR zones, zone distribution, HR zone distribution, power curve, decoupling, intervals, ride summary, fitness trend)
- ✅ Configuration serialization
- ✅ Edge cases and null handling

Run `./test.sh` after making edits to verify everything still works.

### Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Test thoroughly (see `TESTING.md`)
4. Submit a pull request

## 🐛 Troubleshooting

### "Configuration not found"
Run `/coach setup` or set environment variables:
```bash
export INTERVALS_ICU_API_KEY="your-key"
export INTERVALS_ICU_ATHLETE_ID="i12345"
```

### "Authentication failed"
- Verify your API key in intervals.icu → Settings → Developer Settings
- Check your athlete ID format (should start with 'i')
- Run `/coach setup` to reconfigure

### "No recent activities found"
- Ensure activities are uploaded to intervals.icu
- Check sync from Garmin/Strava/etc.
- Wait a few minutes for processing

### Missing analysis metrics (VI, decoupling, intervals)
**Problem:** Activity shows basic stats but missing detailed analysis like VI, aerobic decoupling, or interval detection.

**Cause:** Strava-synced activities lack detailed power/HR streams due to Strava's API restrictions.

**Solution:**
- Configure your device to upload directly to intervals.icu (Wahoo, Garmin Connect, Zwift, etc.)
- Or: Enable Garmin Connect → intervals.icu sync instead of Strava → intervals.icu
- Check intervals.icu activity page - if the power/HR graph shows only summary stats (not second-by-second), the data isn't available

### Network errors
- Check internet connection
- Verify intervals.icu is accessible
- Check for API rate limits (wait and retry)

## 📚 Additional Resources

- [intervals.icu Documentation](https://intervals.icu/api)
- [Training with Power (book)](https://www.amazon.com/Training-Racing-Power-Meter-2nd/dp/1934030554)
- [CTL/ATL/TSB Explained](https://www.trainingpeaks.com/blog/applying-the-numbers-ctl-atl-and-tsb/)

## 🤝 Credits

Built with:
- [Claude Code](https://claude.ai/code) by Anthropic
- [intervals.icu](https://intervals.icu) by David Tinker
- [.NET 10](https://dotnet.microsoft.com/) by Microsoft
- [Spectre.Console](https://spectreconsole.net/) for terminal UI and colored output
- [System.CommandLine](https://github.com/dotnet/command-line-api) for CLI parsing

## 📄 License

MIT License - see [LICENSE](LICENSE)

## 🌟 Star History

If this skill helps your training, please star the repository!