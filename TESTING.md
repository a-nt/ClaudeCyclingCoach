# Testing Guide for Cycling Coach Skill

## Pre-Testing Setup

1. **Verify installation:**
   ```bash
   ls -la ~/.claude/skills/coach/
   ```
   Should see: SKILL.md, bin/, src/, examples/, README.md, LICENSE.txt, .config.json.example

2. **Verify .NET runtime:**
   ```bash
   dotnet --version
   ```
   Should show .NET 9.0 or later

3. **Check binary exists:**
   ```bash
   ls -la ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll
   ```

## Manual CLI Testing

### Test 1: CLI Help

```bash
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll --help
```

Expected: Command list with available options

### Test 2: Profile Command

```bash
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll profile
```

Expected: JSON output with athlete profile, FTP, zones, and current CTL/ATL/TSB

### Test 3: Activities List

```bash
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activities --limit 5
```

Expected: JSON array of 5 most recent activities

### Test 4: Wellness Data

```bash
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll wellness --days 30
```

Expected: JSON array of CTL/ATL/TSB data for past 30 days

### Test 5: Activity Analysis

```bash
# Get recent activity ID first
ACTIVITY_ID=$(dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activities --limit 1 | jq -r '.[0].id')

# Analyze it
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll analyze $ACTIVITY_ID
```

Expected: JSON with comprehensive analysis (VI, EF, decoupling, intervals, zone distribution)

### Test 6: Configuration Check (No Config)

```bash
unset INTERVALS_ICU_API_KEY
unset INTERVALS_ICU_ATHLETE_ID
mv ~/.claude/skills/coach/.config.json ~/.claude/skills/coach/.config.json.bak 2>/dev/null
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll profile
```

Expected: Error message about missing configuration

Cleanup:
```bash
mv ~/.claude/skills/coach/.config.json.bak ~/.claude/skills/coach/.config.json 2>/dev/null
```

## End-to-End Testing in Claude Code

### Test Scenario 1: First-Time Setup

1. Remove any existing config:
   ```bash
   rm ~/.claude/skills/coach/.config.json 2>/dev/null
   unset INTERVALS_ICU_API_KEY
   unset INTERVALS_ICU_ATHLETE_ID
   ```

2. Start Claude Code and run:
   ```
   /coach setup
   ```

3. **Expected:**
   - Clear instructions on where to get credentials
   - Prompts for API key and athlete ID
   - Confirmation of successful setup
   - Config file created at ~/.claude/skills/coach/.config.json

4. **Verify:**
   ```bash
   cat ~/.claude/skills/coach/.config.json
   ls -l ~/.claude/skills/coach/.config.json  # Should be -rw-------
   ```

### Test Scenario 2: Profile Display

1. Run:
   ```
   /coach profile
   ```

2. **Expected:**
   - Display of athlete name and ID
   - FTP in watts and w/kg
   - Weight
   - Heart rate zones
   - Current CTL, ATL, TSB with interpretation
   - Power zones table

3. **Verify:**
   - All metrics are displayed clearly
   - No error messages
   - Formatting is clean and professional

### Test Scenario 3: Activity Analysis

1. Run:
   ```
   /coach analyze
   ```

2. **Expected:**
   - Fetches most recent activity
   - Displays comprehensive analysis:
     - Overview metrics
     - Zone distribution
     - Decoupling analysis (if ride long enough)
     - Intervals detected (if threshold work present)
     - Power/HR relationship
     - Coaching recommendations
   - Asks follow-up questions

3. **Verify:**
   - Analysis references example template format
   - Interpretations are clear and actionable
   - Recommendations are specific

### Test Scenario 4: Specific Activity

1. Get an activity ID from your recent activities
2. Run:
   ```
   /coach analyze [activity-id]
   ```

3. **Expected:**
   - Analyzes the specific activity requested
   - Same comprehensive output as Test 3

### Test Scenario 5: Fitness Trends

1. Run:
   ```
   /coach trends
   ```

2. **Expected:**
   - Displays 30-day trend analysis:
     - Current CTL/ATL/TSB
     - CTL progression with interpretation
     - ATL patterns
     - TSB trajectory
     - Weekly load breakdown
     - Strategic recommendations

3. **Verify:**
   - References trend report template
   - Identifies current training phase
   - Provides forward-looking guidance

### Test Scenario 6: Custom Trend Period

1. Run:
   ```
   /coach trends 60
   ```

2. **Expected:**
   - Same as Test 5, but for 60-day period

### Test Scenario 7: Check-in Command

1. Run:
   ```
   /coach check-in
   ```

2. **Expected:**
   - Holistic analysis of recent training
   - Pattern detection (overreaching, building, coasting, etc.)
   - Context-aware recommendations
   - Direct actionable advice without questions

3. **Verify:**
   - Analysis considers training philosophy if set
   - Recommendations match current training phase
   - Advice is specific to recent patterns

### Test Scenario 8: Training Context

1. Run:
   ```
   /coach context
   ```

2. **Expected:**
   - Menu to set training goals, weekly hours, phase, etc.
   - Smart inference from recent data
   - Confirmation of saved context

3. **Verify:**
   - Context stored in .config.json
   - Future commands reference this context

### Test Scenario 9: Training Philosophy

1. Run:
   ```
   /coach philosophy
   ```

2. **Expected:**
   - Options: Polarized (80/20), Sweet Spot, Traditional, etc.
   - Explanation of each approach
   - Confirmation of selection

3. **Verify:**
   - Philosophy stored in .config.json
   - Future analysis references this philosophy

### Test Scenario 10: Follow-up Conversation

1. Run any analysis command:
   ```
   /coach analyze
   ```

2. Then ask follow-up questions:
   ```
   What does this mean for my FTP?
   Should I do another threshold test?
   Should I take a recovery day?
   How should I train this week?
   ```

3. **Expected:**
   - Answers reference previous analysis data
   - No re-fetching of activity data
   - Context is maintained throughout conversation
   - Recommendations are specific to your situation

### Test Scenario 11: General Coaching Conversation

1. Run:
   ```
   /coach
   ```

2. Ask general questions:
   ```
   What's the best way to improve my FTP?
   How often should I do threshold intervals?
   What does decoupling mean?
   ```

3. **Expected:**
   - Expert cycling coaching advice
   - References to your profile/data if available
   - Educational explanations of concepts

## Error Scenario Testing

### Error 1: Missing Configuration

1. Remove config:
   ```bash
   rm ~/.claude/skills/coach/.config.json
   unset INTERVALS_ICU_API_KEY
   unset INTERVALS_ICU_ATHLETE_ID
   ```

2. Run: `/coach analyze`

3. **Expected:**
   - Clear error message
   - Instructions on how to set up
   - Guidance to run `/coach setup`

### Error 2: Invalid Credentials

1. Create config with bad credentials:
   ```bash
   echo '{"apiKey": "invalid", "athleteId": "i99999"}' > ~/.claude/skills/coach/.config.json
   ```

2. Run: `/coach profile`

3. **Expected:**
   - Authentication error (401)
   - Clear message about invalid credentials
   - Instructions to verify and reconfigure

### Error 3: Invalid Activity ID

1. Run: `/coach analyze 999999999`

2. **Expected:**
   - 404 error
   - Message explaining resource not found
   - Suggestion to check activity ID

### Error 4: No Recent Activities

Test with a new intervals.icu account with no activities.

**Expected:**
- Clear message that no activities found
- Instructions on uploading/syncing rides

### Error 5: Strava-Synced Activity (Limited Data)

1. Analyze an activity synced through Strava
2. Run: `/coach analyze [strava-activity-id]`

**Expected:**
- Basic metrics displayed
- Warning about missing detailed streams
- Explanation of Strava API limitations
- Suggestion to use direct upload (Wahoo/Garmin/Zwift)

## Performance Testing

### Context Preservation

1. Run: `/coach analyze`
2. Ask: "What was my average power?"
3. Ask: "What about my heart rate?"
4. Ask: "Should I test my FTP?"

**Verify:**
- All questions answered without re-running CLI commands
- Activity data is referenced from context
- No unnecessary API calls

### Multiple Commands

1. Run: `/coach profile`
2. Run: `/coach analyze`
3. Run: `/coach trends`

**Verify:**
- Profile data stored after first command
- Analyze uses stored profile (check for CLI calls)
- Trends uses stored profile

## Build and Deployment Testing

### Rebuild from Source

1. Navigate to source directory:
   ```bash
   cd ~/.claude/skills/coach/src/CoachCli/CoachCli
   ```

2. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build -c Release
   ```

3. **Expected:**
   - Build succeeds with no errors
   - Time elapsed < 5 seconds

### Publish New Binary

1. Publish to bin directory:
   ```bash
   cd ~/.claude/skills/coach/src/CoachCli/CoachCli
   dotnet publish -c Release -r osx-arm64 --self-contained false \
     -o ../../../bin/coach-cli
   ```

2. **Expected:**
   - Publish succeeds
   - CoachCli.dll updated in bin/coach-cli/
   - Skill continues to work in Claude Code

### Test Wrapper Script

1. Run commands using wrapper:
   ```bash
   ~/.claude/skills/coach/bin/coach-cli.sh profile
   ~/.claude/skills/coach/bin/coach-cli.sh activities --limit 3
   ```

2. **Expected:**
   - Same output as direct dotnet invocation
   - No errors

## Distribution Testing

### Test on Fresh System

1. Package the skill:
   ```bash
   tar -czf cycling-coach-skill.tar.gz -C ~/.claude/skills coach
   ```

2. On another machine or user account:
   ```bash
   mkdir -p ~/.claude/skills
   tar -xzf cycling-coach-skill.tar.gz -C ~/.claude/skills
   ```

3. Configure with different intervals.icu account
4. Test all commands work correctly

**Verify:**
- Skill works with any intervals.icu account
- No hardcoded athlete-specific data
- README instructions are complete and accurate
- .NET 9 runtime is available

## Success Criteria Checklist

- [ ] Setup command stores credentials successfully
- [ ] Profile command displays athlete data
- [ ] Analyze command works for last activity
- [ ] Analyze command works for specific activity ID
- [ ] Analyze command calculates VI, EF, decoupling, intervals
- [ ] Trends command shows 30-day progression
- [ ] Trends command accepts custom day range
- [ ] Check-in command provides holistic coaching
- [ ] Context command sets training parameters
- [ ] Philosophy command sets training approach
- [ ] Follow-up questions work without re-fetching
- [ ] Error messages are clear and actionable
- [ ] Output formatting is consistent and professional
- [ ] Context is maintained across conversation
- [ ] Config file permissions set correctly (600)
- [ ] Skill works for any intervals.icu user
- [ ] README provides complete setup instructions
- [ ] Build system works (dotnet build/publish)
- [ ] .NET 9 runtime dependency is documented

## Known Limitations

1. **Rate limits:** intervals.icu API has rate limits - avoid rapid repeated calls
2. **Data availability:** Some metrics require power meter and heart rate monitor
3. **Activity types:** Analysis optimized for cycling rides with power data
4. **Strava sync:** Activities synced through Strava lack detailed power/HR streams
5. **.NET required:** Skill requires .NET 9 runtime installed
6. **Startup time:** ~250ms CLI startup (acceptable for API-bound operations)

## Debugging

### Check CLI Output Directly

Run CLI commands directly to see raw JSON:

```bash
# Profile data
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll profile | jq

# Recent activities
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activities --limit 5 | jq

# Specific activity
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activity <id> | jq

# Wellness data
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll wellness --days 30 | jq

# Analyze activity
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll analyze <id> | jq
```

### Check Configuration

```bash
# Environment variables
echo $INTERVALS_ICU_API_KEY
echo $INTERVALS_ICU_ATHLETE_ID

# Config file
cat ~/.claude/skills/coach/.config.json

# File permissions
ls -l ~/.claude/skills/coach/.config.json
```

### Common Issues

**"dotnet: command not found"**
- Install .NET 9 Runtime: https://dotnet.microsoft.com/download
- Verify: `dotnet --version`

**"Could not find CoachCli.dll"**
- Rebuild binary: `cd src/CoachCli/CoachCli && dotnet publish ...`
- Check path: `ls ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll`

**"ENOTFOUND intervals.icu"**
- Check internet connection
- Verify intervals.icu is accessible

**Empty analysis results**
- Check that activity has power/HR data
- Some calculations require minimum duration (e.g., 10+ minutes for decoupling)
- Strava-synced activities may lack detailed streams

**404 on wellness endpoint**
- This was a known bug, fixed in IntervalsApiClient.cs
- Ensure using latest binary (check git commit)

## Performance Benchmarks

Expected CLI command times (with network latency):

- `profile`: ~500-800ms
- `activities --limit 5`: ~400-600ms
- `activity <id>`: ~800-1200ms (includes stream data)
- `wellness --days 30`: ~500-800ms
- `analyze <id>`: ~1000-1500ms (includes activity + streams + calculations)

Note: First run may be slower due to .NET JIT compilation.
