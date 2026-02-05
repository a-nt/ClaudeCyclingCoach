# Contributing to Cycling Coach Skill

Thanks for your interest in contributing! This skill helps cyclists get AI-powered coaching insights from their training data.

## How to Contribute

### Reporting Issues

Found a bug or have a feature request?

1. Check [existing issues](https://github.com/a-nt/cycling-coach-skill/issues) first
2. Create a new issue with:
   - Clear description of the problem/feature
   - Steps to reproduce (for bugs)
   - Your environment (OS, Node.js version)
   - Example data if relevant (redact personal info)

### Code Contributions

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow existing code style
   - Test thoroughly (see `docs/TESTING.md`)
   - Update documentation if needed

4. **Test with real data**
   - Ensure scripts work with intervals.icu API
   - Test all commands: setup, profile, analyze, trends
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
git clone https://github.com/YOUR-USERNAME/cycling-coach-skill.git
cd cycling-coach-skill

# Link to Claude Code skills directory
ln -s "$(pwd)/skill-source" ~/.claude/skills/cycling-coach

# Set up test credentials
export INTERVALS_ICU_API_KEY="your-test-key"
export INTERVALS_ICU_ATHLETE_ID="i12345"

# Test the scripts
node skill-source/scripts/intervals-api.js profile
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

- **Node.js:** Use built-in modules only (no external dependencies)
- **Formatting:** 2-space indentation, semicolons
- **Comments:** Explain why, not what (code should be self-documenting)
- **Error handling:** Always return JSON with success/error status

## Testing

Before submitting:

1. **Manual testing** (see `docs/TESTING.md`)
   - Test all commands with real intervals.icu account
   - Test error scenarios
   - Verify conversational follow-ups work

2. **Script testing**
   ```bash
   # Test API client
   node skill-source/scripts/intervals-api.js profile

   # Test analysis with sample data
   echo '{"activity": {...}, "profile": {...}}' | \
     node skill-source/scripts/analyze-activity.js
   ```

3. **Check for credentials leaks**
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
