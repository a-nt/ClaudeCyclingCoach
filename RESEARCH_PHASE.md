# Research Phase: .NET Port Validation

**Goal:** Validate key technical decisions before committing to full 7-week port.

**Duration:** 1 week (20-25 hours)

---

## Research Tasks

### 1. Prototype: SQLite Sync & Performance (6-8 hours)

**Objective:** Validate that SQLite can handle full training history with acceptable performance.

**Prototype:** `CoachCliPrototype.Sync/`

```bash
mkdir -p research/sync-prototype
cd research/sync-prototype
dotnet new console -n SyncPrototype
cd SyncPrototype
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package System.Text.Json
```

**Tasks:**
- [ ] Create minimal DbContext with Activities + Wellness tables
- [ ] Download 500 activities from intervals.icu API
- [ ] Deserialize JSON and store in SQLite
- [ ] Measure:
  - [ ] Download time
  - [ ] Insert time (bulk vs individual)
  - [ ] Database size
  - [ ] Query performance (search, filter, aggregate)

**Success Criteria:**
- ✅ 500 activities download + store in < 60 seconds
- ✅ Database size < 100MB for 500 activities
- ✅ Search queries return in < 100ms
- ✅ LINQ queries work as expected

**Code Structure:**
```csharp
// Models/Activity.cs
public class Activity
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public double? Tss { get; set; }
    public string PowerStream { get; set; }  // JSON array
}

// Data/TrainingContext.cs
public class TrainingContext : DbContext
{
    public DbSet<Activity> Activities { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=training.db");
}

// Program.cs
var context = new TrainingContext();
await context.Database.EnsureCreatedAsync();

// Test sync
var client = new HttpClient();
// ... download from intervals.icu
// ... insert into database
// ... measure performance

// Test queries
var threshold = await context.Activities
    .Where(a => a.Name.Contains("threshold"))
    .OrderByDescending(a => a.Date)
    .Take(10)
    .ToListAsync();

Console.WriteLine($"Found {threshold.Count} threshold workouts in {stopwatch.ElapsedMilliseconds}ms");
```

**Measurements to Record:**
```
Download 500 activities: ____ seconds
Insert 500 activities: ____ seconds
Database size: ____ MB
Search "threshold": ____ ms
Complex query (date range + TSS filter): ____ ms
```

---

### 2. Prototype: Native AOT Compilation (3-4 hours)

**Objective:** Verify Native AOT works with our dependencies and measure startup time.

**Prototype:** `CoachCliPrototype.Aot/`

```bash
mkdir -p research/aot-prototype
cd research/aot-prototype
dotnet new console -n AotPrototype
cd AotPrototype
```

**Project Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>false</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
    <PackageReference Include="System.CommandLine" Version="2.0.*" />
    <PackageReference Include="System.Text.Json" Version="8.0.*" />
  </ItemGroup>
</Project>
```

**Tasks:**
- [ ] Create simple CLI with System.CommandLine
- [ ] Add EF Core with SQLite
- [ ] Add JSON serialization
- [ ] Publish as Native AOT:
  ```bash
  dotnet publish -r osx-arm64 -c Release -p:PublishAot=true
  ```
- [ ] Measure:
  - [ ] Binary size
  - [ ] Cold start time
  - [ ] Warm start time
  - [ ] Memory usage

**Success Criteria:**
- ✅ Compiles successfully with AOT
- ✅ Binary size < 20MB
- ✅ Cold start < 100ms
- ✅ No runtime errors

**Measurements to Record:**
```
Binary size: ____ MB
Cold start (first run): ____ ms
Warm start (second run): ____ ms
Peak memory usage: ____ MB
```

**Known AOT Limitations to Test:**
- [ ] Reflection in EF Core (migrations)
- [ ] JSON serialization (System.Text.Json source generation)
- [ ] Dynamic LINQ queries

**Workarounds if needed:**
- Use source generators for JSON
- Pre-compile EF Core migrations
- Use compiled queries

---

### 3. Prototype: intervals.icu API Client (3-4 hours)

**Objective:** Validate API client works reliably with proper error handling.

**Prototype:** `CoachCliPrototype.ApiClient/`

```bash
mkdir -p research/api-client-prototype
cd research/api-client-prototype
dotnet new console -n ApiClientPrototype
```

**Tasks:**
- [ ] Implement IntervalsApiClient with HttpClient
- [ ] Test all endpoints:
  - [ ] GET /api/v1/athlete/{id} (profile)
  - [ ] GET /api/v1/athlete/{id}/activities (activities list)
  - [ ] GET /api/v1/activity/{id} (single activity with streams)
  - [ ] GET /api/v1/athlete/{id}/wellness (CTL/ATL/TSB)
- [ ] Handle errors:
  - [ ] 401 Unauthorized
  - [ ] 404 Not Found
  - [ ] 429 Rate Limit
  - [ ] Network timeouts
  - [ ] Invalid JSON
- [ ] Test with real intervals.icu credentials

**Success Criteria:**
- ✅ All endpoints return valid data
- ✅ Error handling works correctly
- ✅ Rate limiting is respected
- ✅ Streams (power/HR) deserialize correctly

**Code Structure:**
```csharp
public interface IIntervalsApiClient
{
    Task<ApiProfile> GetProfileAsync();
    Task<List<ApiActivity>> GetActivitiesAsync(DateTime? oldest = null);
    Task<ApiActivity> GetActivityAsync(string id);
    Task<List<ApiWellness>> GetWellnessAsync(int days = 90);
}

public class IntervalsApiClient : IIntervalsApiClient
{
    private readonly HttpClient _httpClient;

    public IntervalsApiClient(HttpClient httpClient, string apiKey, string athleteId)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://intervals.icu");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"API_KEY:{apiKey}")));
    }

    public async Task<ApiProfile> GetProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/athlete/{_athleteId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ApiProfile>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new AuthenticationException("Invalid API key or athlete ID");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new RateLimitException("Rate limit exceeded, please wait");
        }
    }
}
```

**Test Checklist:**
- [ ] Profile with nested sportSettings (FTP, zones)
- [ ] Activities with date range filtering
- [ ] Activity with full power/HR streams
- [ ] Wellness data (90 days of CTL/ATL/TSB)
- [ ] Invalid credentials (401)
- [ ] Non-existent activity (404)

---

### 4. Prototype: System.CommandLine Interface (2-3 hours)

**Objective:** Validate System.CommandLine provides good UX for CLI.

**Prototype:** `CoachCliPrototype.Cli/`

```bash
mkdir -p research/cli-prototype
cd research/cli-prototype
dotnet new console -n CliPrototype
cd CliPrototype
dotnet add package System.CommandLine
```

**Tasks:**
- [ ] Create sample commands:
  - [ ] `coach-cli analyze [id]` (with optional argument)
  - [ ] `coach-cli sync --full` (with flag option)
  - [ ] `coach-cli search <query> --type <type> --date-range <range>` (multiple options)
- [ ] Test help generation (`coach-cli --help`)
- [ ] Test argument validation
- [ ] Test error messages

**Success Criteria:**
- ✅ Commands are intuitive
- ✅ Help text is clear
- ✅ Validation errors are helpful
- ✅ Supports subcommands, arguments, and options

**Code Structure:**
```csharp
using System.CommandLine;

var rootCommand = new RootCommand("AI Cycling Coach");

// Analyze command
var analyzeCommand = new Command("analyze", "Analyze an activity");
var activityIdArg = new Argument<string?>("activity-id", "Activity ID (optional, defaults to latest)");
activityIdArg.SetDefaultValue(null);
analyzeCommand.AddArgument(activityIdArg);
analyzeCommand.SetHandler(async (string? id) =>
{
    Console.WriteLine($"Analyzing activity: {id ?? "latest"}");
    // Implementation
}, activityIdArg);
rootCommand.AddCommand(analyzeCommand);

// Sync command
var syncCommand = new Command("sync", "Sync data from intervals.icu");
var fullOption = new Option<bool>("--full", "Full re-sync");
syncCommand.AddOption(fullOption);
syncCommand.SetHandler(async (bool full) =>
{
    Console.WriteLine($"Syncing (full: {full})");
    // Implementation
}, fullOption);
rootCommand.AddCommand(syncCommand);

// Search command
var searchCommand = new Command("search", "Search activities");
var queryArg = new Argument<string>("query", "Search query");
var typeOption = new Option<string?>("--type", "Activity type");
var dateRangeOption = new Option<string?>("--date-range", "Date range (YYYY-MM)");
searchCommand.AddArgument(queryArg);
searchCommand.AddOption(typeOption);
searchCommand.AddOption(dateRangeOption);
searchCommand.SetHandler(async (string query, string? type, string? dateRange) =>
{
    Console.WriteLine($"Searching: {query}, type: {type}, date: {dateRange}");
    // Implementation
}, queryArg, typeOption, dateRangeOption);
rootCommand.AddCommand(searchCommand);

return await rootCommand.InvokeAsync(args);
```

**Test Cases:**
```bash
# Test help
coach-cli --help
coach-cli analyze --help
coach-cli sync --help

# Test commands
coach-cli analyze
coach-cli analyze abc123
coach-cli sync
coach-cli sync --full
coach-cli search "threshold"
coach-cli search "threshold" --type Ride --date-range 2024-01

# Test validation
coach-cli search  # Should error: missing required argument
coach-cli analyze --invalid  # Should error: unknown option
```

---

### 5. Analysis: Performance Calculations (2-3 hours)

**Objective:** Port core analysis algorithms and verify correctness.

**Prototype:** Test calculations match Node.js implementation

**Tasks:**
- [ ] Port decoupling calculation
- [ ] Port VI (Variability Index) calculation
- [ ] Port EF (Efficiency Factor) calculation
- [ ] Port power curve analysis
- [ ] Port zone distribution calculation
- [ ] Create unit tests comparing to known values
- [ ] Compare output to Node.js version for same activity

**Success Criteria:**
- ✅ All calculations match Node.js output (within rounding)
- ✅ Performance is acceptable (< 100ms per activity)
- ✅ Code is maintainable and testable

**Code Structure:**
```csharp
public static class MetricsCalculator
{
    public static double CalculateVariabilityIndex(double avgPower, double normalizedPower)
    {
        if (avgPower <= 0) return 0;
        return normalizedPower / avgPower;
    }

    public static double CalculateEfficiencyFactor(double normalizedPower, double avgHr)
    {
        if (avgHr <= 0) return 0;
        return normalizedPower / avgHr;
    }

    public static DecouplingResult CalculateDecoupling(List<int> powerStream, List<int> hrStream)
    {
        var midpoint = powerStream.Count / 2;

        var firstHalfPower = powerStream.Take(midpoint).Average();
        var firstHalfHr = hrStream.Take(midpoint).Average();
        var secondHalfPower = powerStream.Skip(midpoint).Average();
        var secondHalfHr = hrStream.Skip(midpoint).Average();

        var firstEfficiency = firstHalfPower / firstHalfHr;
        var secondEfficiency = secondHalfPower / secondHalfHr;

        var decouplingPercent = ((firstHalfHr / firstHalfPower) - (secondHalfHr / secondHalfPower))
            / (firstHalfHr / firstHalfPower) * 100;

        return new DecouplingResult
        {
            FirstHalfPower = firstHalfPower,
            FirstHalfHr = firstHalfHr,
            SecondHalfPower = secondHalfPower,
            SecondHalfHr = secondHalfHr,
            DecouplingPercent = decouplingPercent,
            Interpretation = InterpretDecoupling(decouplingPercent)
        };
    }

    public static string InterpretDecoupling(double percent)
    {
        return percent switch
        {
            < 5.0 => "Excellent - minimal decoupling",
            < 10.0 => "Good - acceptable decoupling",
            _ => "Poor - significant decoupling, aerobic base needs work"
        };
    }
}

// Unit tests
[Fact]
public void CalculateVI_PerfectPacing_Returns1()
{
    var vi = MetricsCalculator.CalculateVariabilityIndex(250, 250);
    Assert.Equal(1.0, vi, precision: 2);
}

[Fact]
public void CalculateVI_HighVariability_ReturnsGreaterThan1()
{
    var vi = MetricsCalculator.CalculateVariabilityIndex(220, 250);
    Assert.Equal(1.14, vi, precision: 2);
}

[Fact]
public void CalculateDecoupling_TestActivity_MatchesNodeJs()
{
    // Use actual activity data from Node.js test
    var powerStream = new List<int> { /* actual data */ };
    var hrStream = new List<int> { /* actual data */ };

    var result = MetricsCalculator.CalculateDecoupling(powerStream, hrStream);

    // Compare to Node.js output
    Assert.Equal(2.3, result.DecouplingPercent, precision: 1);
    Assert.Equal("Excellent - minimal decoupling", result.Interpretation);
}
```

---

## Research Deliverables

At the end of research phase, produce:

### 1. Performance Report
```markdown
# Performance Validation Report

## SQLite Sync Performance
- 500 activities: X seconds
- Database size: X MB
- Search query: X ms
- Complex query: X ms
✅ PASS / ❌ FAIL

## Native AOT
- Binary size: X MB
- Cold start: X ms
- Warm start: X ms
- Memory usage: X MB
✅ PASS / ❌ FAIL

## API Client
- All endpoints working: ✅ / ❌
- Error handling: ✅ / ❌
- Rate limiting: ✅ / ❌
✅ PASS / ❌ FAIL

## CLI UX
- Intuitive commands: ✅ / ❌
- Clear help text: ✅ / ❌
- Good error messages: ✅ / ❌
✅ PASS / ❌ FAIL

## Calculations
- All metrics match Node.js: ✅ / ❌
- Performance acceptable: ✅ / ❌
✅ PASS / ❌ FAIL
```

### 2. Technical Decision Document
```markdown
# Technical Decisions

## Recommended Approach
- [ ] Framework-dependent (.NET runtime required)
- [ ] Self-contained (includes runtime)
- [ ] Native AOT (recommended if all tests pass)

## Database Strategy
- SQLite with EF Core: ✅ Validated
- Indexes: [list required indexes]
- Schema: [attach schema SQL]

## Known Issues
- [List any issues discovered]
- [Workarounds or mitigations]

## Risks
- [Any remaining risks]
- [Mitigation strategies]
```

### 3. Go/No-Go Recommendation

```markdown
# Recommendation: GO / NO-GO

## Decision: [GO or NO-GO]

### Reasoning:
- [Key factors supporting decision]
- [Evidence from prototypes]
- [Risk assessment]

### If GO:
- Start Phase 1 on [date]
- Expected completion: [date]
- Resource requirements: [hours/week]

### If NO-GO:
- Reasons for not proceeding
- Alternative approaches to consider
- When to revisit decision
```

---

## Research Schedule

| Day | Tasks | Hours |
|-----|-------|-------|
| Day 1-2 | Prototype 1: SQLite Sync | 6-8h |
| Day 2-3 | Prototype 2: Native AOT | 3-4h |
| Day 3-4 | Prototype 3: API Client | 3-4h |
| Day 4-5 | Prototype 4: System.CommandLine | 2-3h |
| Day 5-6 | Prototype 5: Calculations | 2-3h |
| Day 6-7 | Write reports, make decision | 3-4h |
| **Total** | | **20-25h** |

---

## Success Criteria for GO Decision

All of the following must be TRUE:

- ✅ SQLite can store 500+ activities with good performance
- ✅ Native AOT compiles successfully (or acceptable fallback exists)
- ✅ intervals.icu API client works reliably
- ✅ System.CommandLine provides good UX
- ✅ Calculations match Node.js implementation
- ✅ No critical blockers discovered
- ✅ Estimated effort still reasonable (< 200 hours)

If ANY critical blocker found → NO-GO or DEFER

---

## Next Step

Start with **Prototype 1: SQLite Sync & Performance**

Create the prototype:
```bash
mkdir -p research/sync-prototype
cd research/sync-prototype
dotnet new console -n SyncPrototype
cd SyncPrototype
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package System.Text.Json
```

Ready to build first prototype?
