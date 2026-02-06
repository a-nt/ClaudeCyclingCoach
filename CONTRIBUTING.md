# Contributing to Cycling Coach Skill

Thanks for your interest in contributing! This skill helps cyclists get AI-powered coaching insights from their training data.

## How to Contribute

### Reporting Issues

Found a bug or have a feature request?

1. Check [existing issues](https://github.com/a-nt/ClaudeCyclingCoach/issues) first
2. Create a new issue with:
   - Clear description of the problem/feature
   - Steps to reproduce (for bugs)
   - Your environment (OS, .NET version)
   - Example data if relevant (redact personal info)

### Code Contributions

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow existing code style (C# conventions)
   - Test thoroughly (see `TESTING.md`)
   - Update documentation if needed

4. **Test with real data**
   - Ensure CLI works with intervals.icu API
   - Test all commands: setup, profile, analyze, trends, check-in
   - Verify error handling

5. **Commit with clear messages**
   ```bash
   git commit -m "Add feature: description of what you added"
   ```

6. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   Then open a pull request with:
   - Description of changes
   - Why the change is needed
   - Any breaking changes

## Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/ClaudeCyclingCoach.git
cd ClaudeCyclingCoach

# Link to Claude Code skills directory (root is the skill)
ln -s "$(pwd)" ~/.claude/skills/coach

# Set up test credentials
export INTERVALS_ICU_API_KEY="your-test-key"
export INTERVALS_ICU_ATHLETE_ID="i12345"

# Test the CLI
dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll profile

# Or rebuild from source
cd src/CoachCli/CoachCli
dotnet build -c Release
dotnet publish -c Release -r osx-arm64 --self-contained false \
  -o ../../../bin/coach-cli
```

## Areas for Contribution

### High Priority
- [ ] Support for non-Strava activities (direct intervals.icu uploads)
- [ ] Better handling of missing data (FTP, weight, zones)
- [ ] Weekly training summary feature
- [ ] Training plan generation

### Good First Issues
- [ ] Add more example responses to SKILL.md
- [ ] Improve error messages
- [ ] Add unit tests for analysis calculations
- [ ] Documentation improvements

### Advanced
- [ ] Goal tracking system
- [ ] Race preparation mode
- [ ] Equipment recommendations
- [ ] Integration with other platforms (TrainingPeaks, etc.)

## Code Style

- **C#:** Follow standard C# conventions (.NET coding standards)
- **Dependencies:** Minimal - Spectre.Console for UI, System.CommandLine for CLI
- **Charts:** Use Spectre.Console tables/panels for consistent formatting
- **Colors:** Follow zone color scheme (grey→blue→green→yellow→orange→red)
- **Formatting:** 4-space indentation, follow .editorconfig if present
- **Comments:** Explain why, not what (code should be self-documenting)
- **Error handling:** Return structured JSON with clear error messages

## Testing

Before submitting:

1. **Manual testing** (see `TESTING.md`)
   - Test all commands with real intervals.icu account
   - Test error scenarios
   - Verify conversational follow-ups work

2. **CLI testing**
   ```bash
   # Test all commands
   dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll profile
   dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activities --limit 5
   dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll wellness --days 30

   # Test analysis
   ACTIVITY_ID=$(dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll activities --limit 1 | jq -r '.[0].id')
   dotnet ~/.claude/skills/coach/bin/coach-cli/CoachCli.dll analyze $ACTIVITY_ID
   ```

3. **Build testing**
   ```bash
   cd src/CoachCli/CoachCli
   dotnet clean
   dotnet build -c Release
   ```

4. **Check for credentials leaks**
   ```bash
   git diff | grep -i "api.*key\|athlete.*id"
   ```

## SKILL.md Guidelines

The SKILL.md file is what Claude reads. When updating:

- **Keep it concise:** Claude works better with focused instructions
- **Be specific:** Show examples of good vs bad responses
- **Test changes:** Use `/coach` commands to verify behavior changed
- **Maintain tone:** Conversational coach, not report generator

## Communication Style

This skill is designed to feel like chatting with a coach, not reading reports:

✅ **Good:** "Your CTL is climbing - up to 38. Easy week ahead?"

❌ **Bad:** "Based on comprehensive analysis of CTL progression metrics..."

Keep responses short and conversational unless a detailed report is requested.

## Questions?

Open an issue or discussion on GitHub. We're happy to help!

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
