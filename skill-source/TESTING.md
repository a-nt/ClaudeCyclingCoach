# Testing Guide for Cycling Coach Skill

## Pre-Testing Setup

1. **Verify installation:**
   ```bash
   ls -la ~/.claude/skills/cycling-coach/
   ```
   Should see: SKILL.md, scripts/, examples/, README.md, LICENSE.txt, .config.json.example

2. **Check script permissions:**
   ```bash
   ls -l ~/.claude/skills/cycling-coach/scripts/
   ```
   Both .js files should be executable (have x permission)

## Manual Script Testing

### Test 1: intervals-api.js Help

```bash
node ~/.claude/skills/cycling-coach/scripts/intervals-api.js
```

Expected: Error message with available commands listed

### Test 2: analyze-activity.js with Sample Data

```bash
echo '{"activity": {"id": "test", "name": "Test", "startDate": "2024-01-01", "type": "Ride", "movingTime": 3600, "distance": 25000, "averagePower": 200, "normalizedPower": 210, "averageHr": 140}, "profile": {"ftp": 250}}' | node ~/.claude/skills/cycling-coach/scripts/analyze-activity.js
```

Expected: JSON output with success: true and calculated metrics

### Test 3: Configuration Check (No Config)

```bash
unset INTERVALS_ICU_API_KEY
unset INTERVALS_ICU_ATHLETE_ID
node ~/.claude/skills/cycling-coach/scripts/intervals-api.js profile
```

Expected: Error with "Configuration missing" message

## End-to-End Testing in Claude Code

### Test Scenario 1: First-Time Setup

1. Remove any existing config:
   ```bash
   rm ~/.claude/skills/cycling-coach/.config.json 2>/dev/null
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
   - Config file created at ~/.claude/skills/cycling-coach/.config.json

4. **Verify:**
   ```bash
   cat ~/.claude/skills/cycling-coach/.config.json
   ls -l ~/.claude/skills/cycling-coach/.config.json  # Should be -rw-------
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
   - Current CTL, ATL, TSB
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
     - Wellness correlations (if available)
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

### Test Scenario 7: Follow-up Conversation

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

### Test Scenario 8: General Coaching Conversation

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
   rm ~/.claude/skills/cycling-coach/.config.json
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
   echo '{"apiKey": "invalid", "athleteId": "i99999"}' > ~/.claude/skills/cycling-coach/.config.json
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

## Performance Testing

### Context Preservation

1. Run: `/coach analyze`
2. Ask: "What was my average power?"
3. Ask: "What about my heart rate?"
4. Ask: "Should I test my FTP?"

**Verify:**
- All questions answered without re-running scripts
- Activity data is referenced from context
- No unnecessary API calls

### Multiple Commands

1. Run: `/coach profile`
2. Run: `/coach analyze`
3. Run: `/coach trends`

**Verify:**
- Profile data stored after first command
- Analyze uses stored profile (check logs for script calls)
- Trends uses stored profile

## Distribution Testing

### Test on Fresh System

1. Package the skill:
   ```bash
   tar -czf cycling-coach-skill.tar.gz -C ~/.claude/skills cycling-coach
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

## Success Criteria Checklist

- [ ] Setup command stores credentials successfully
- [ ] Profile command displays athlete data
- [ ] Analyze command works for last activity
- [ ] Analyze command works for specific activity ID
- [ ] Trends command shows 30-day progression
- [ ] Trends command accepts custom day range
- [ ] Follow-up questions work without re-fetching
- [ ] Error messages are clear and actionable
- [ ] Output formatting is consistent and professional
- [ ] Context is maintained across conversation
- [ ] Scripts work with only built-in Node.js modules
- [ ] Config file permissions set correctly (600)
- [ ] Skill works for any intervals.icu user
- [ ] README provides complete setup instructions

## Known Limitations

1. **Rate limits:** intervals.icu API has rate limits - avoid rapid repeated calls
2. **Data availability:** Some metrics require power meter and heart rate monitor
3. **Activity types:** Analysis optimized for cycling rides with power data
4. **Node.js required:** Scripts won't work without Node.js installed

## Debugging

### Check Script Output

Run scripts directly to see raw output:

```bash
# Profile data
node ~/.claude/skills/cycling-coach/scripts/intervals-api.js profile | jq

# Recent activities
node ~/.claude/skills/cycling-coach/scripts/intervals-api.js activities --limit 5 | jq

# Specific activity
node ~/.claude/skills/cycling-coach/scripts/intervals-api.js activity <id> | jq

# Wellness data
node ~/.claude/skills/cycling-coach/scripts/intervals-api.js wellness --days 30 | jq
```

### Check Configuration

```bash
# Environment variables
echo $INTERVALS_ICU_API_KEY
echo $INTERVALS_ICU_ATHLETE_ID

# Config file
cat ~/.claude/skills/cycling-coach/.config.json

# File permissions
ls -l ~/.claude/skills/cycling-coach/.config.json
```

### Common Issues

**"node: command not found"**
- Install Node.js: `brew install node` (macOS) or your OS equivalent

**"ENOTFOUND intervals.icu"**
- Check internet connection
- Verify intervals.icu is accessible

**Empty analysis results**
- Check that activity has power/HR data
- Some calculations require minimum duration (e.g., 10+ minutes for decoupling)

**"Permission denied" on scripts**
- Run: `chmod +x ~/.claude/skills/cycling-coach/scripts/*.js`
