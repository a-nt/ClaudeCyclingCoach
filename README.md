# 🚴 Cycling Coach - AI Training Analysis Skill for Claude Code

> **Intelligent cycling coach powered by Claude AI that analyzes your training data from intervals.icu**

Transform your training data into actionable coaching insights with power/heart rate analysis, fitness trends, and personalized recommendations.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Node.js](https://img.shields.io/badge/node.js-built--in%20modules-green.svg)](https://nodejs.org/)
[![intervals.icu](https://img.shields.io/badge/intervals.icu-integrated-blue.svg)](https://intervals.icu)

## ✨ Features

- 📈 **Activity Analysis** - Deep dive into power/HR coupling, zone distribution, and aerobic decoupling
- 📊 **Fitness Trends** - Track CTL/ATL/TSB progression and training load patterns
- 👤 **Profile Management** - View your FTP, zones, and key metrics at a glance
- 💬 **Conversational Coaching** - Ask follow-up questions and get personalized advice
- 🔒 **Privacy First** - Your data stays on your machine, no cloud processing
- 🚀 **Zero Dependencies** - Uses only Node.js built-in modules

## 🎯 Quick Start

### Prerequisites

- [Node.js](https://nodejs.org/) installed (any recent version)
- [Claude Code](https://claude.ai/download) CLI tool
- [intervals.icu](https://intervals.icu) account with API access

### Installation

```bash
# Clone the repository
git clone https://github.com/a-nt/cycling-coach-skill.git
cd cycling-coach-skill

# Create symlink to Claude Code skills directory
ln -s "$(pwd)/skill-source" ~/.claude/skills/cycling-coach

# Or copy directly
cp -r skill-source ~/.claude/skills/cycling-coach
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
| `/coach profile` | View your athlete profile and training zones |
| `/coach analyze [id]` | Analyze your last ride or a specific activity |
| `/coach trends [days]` | Show fitness trends (default: 30 days) |
| `/coach` | Start a general coaching conversation |

## 📸 Example Usage

```
You: /coach analyze

Coach: Let me analyze your last ride...

# Activity Analysis: Morning Training Ride

## Overview
- Date: Feb 5, 2026
- Duration: 1h 32m
- Average Power: 215W (2.8 W/kg)
- Normalized Power: 235W
- Average HR: 152 bpm

## Zone Distribution
[Detailed breakdown with coaching insights...]

## Aerobic Decoupling
First half: 213W @ 149 bpm (1.43 W/bpm)
Second half: 217W @ 155 bpm (1.40 W/bpm)
Decoupling: 2.3% - Excellent aerobic fitness!

## Coaching Recommendations
1. Your aerobic base is strong - maintain current Z2 volume
2. Consider adding 1-2 threshold sessions per week
3. Recovery metrics look good - you're ready for intensity

You: What does this mean for my FTP?

Coach: Based on your power curve and recent performances...
[Personalized FTP guidance without re-fetching data]
```

## 🧠 What Makes This Different

### Intelligent Analysis
- **Aerobic Decoupling** - Detects cardiac drift to assess aerobic fitness
- **Interval Detection** - Automatically identifies sustained threshold efforts
- **Zone Distribution** - Analyzes time in power and HR zones
- **Power/HR Coupling** - Evaluates aerobic efficiency

### Training Load Management
- **CTL (Chronic Training Load)** - 42-day fitness trend
- **ATL (Acute Training Load)** - 7-day fatigue monitoring
- **TSB (Training Stress Balance)** - Form and freshness tracking
- **Weekly Patterns** - Identifies load distribution and recovery

### Conversational Context
- Ask follow-up questions naturally
- Maintains analysis context throughout conversation
- No unnecessary API calls or re-fetching
- Personalized coaching based on your goals

## 🏗️ Architecture

```
skill-source/
├── SKILL.md                    # Main skill definition (Claude reads this)
├── scripts/
│   ├── intervals-api.js        # intervals.icu API client
│   └── analyze-activity.js     # Analysis calculations engine
├── examples/
│   ├── activity-analysis.md    # Output template for activities
│   └── trend-report.md         # Output template for trends
├── README.md                   # User documentation
├── TESTING.md                  # Testing procedures
└── .config.json               # Your credentials (created on setup)
```

## 🔒 Privacy & Security

- ✅ Credentials stored locally in `~/.claude/skills/cycling-coach/.config.json`
- ✅ File permissions set to `600` (owner read/write only)
- ✅ No data sent to external services (except intervals.icu API)
- ✅ Environment variables supported for extra security
- ✅ API keys never logged or displayed

## 📖 Understanding the Metrics

### Power Zones (based on FTP)
- **Z1 (<55%)** - Recovery
- **Z2 (55-75%)** - Endurance base
- **Z3 (75-90%)** - Tempo
- **Z4 (90-105%)** - Threshold
- **Z5 (>105%)** - VO2 max

### Training Load
- **CTL** - Your fitness level (higher = more fit)
- **ATL** - Your fatigue level (higher = more tired)
- **TSB** - Your form (positive = fresh, negative = fatigued)

Safe CTL ramp rate: 5-8 points/week

### Aerobic Decoupling
- **<5%** - Excellent aerobic fitness
- **5-10%** - Acceptable, room for improvement
- **>10%** - Needs attention (fatigue or insufficient base)

## 🛠️ Development

### Running Tests

```bash
# Test API client directly
node skill-source/scripts/intervals-api.js profile

# Test analysis with sample data
echo '{"activity": {...}, "profile": {...}}' | \
  node skill-source/scripts/analyze-activity.js

# See TESTING.md for comprehensive test procedures
```

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
- Node.js built-in modules only

## 📄 License

MIT License - see [LICENSE.txt](skill-source/LICENSE.txt)

## 🌟 Star History

If this skill helps your training, please star the repository!

---

**Made with ❤️ by cyclists, for cyclists**

Questions? Issues? [Open an issue](https://github.com/a-nt/cycling-coach-skill/issues) or reach out on the Claude Code forums.
