using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CoachCli;

public class IntervalsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _athleteId;

    public IntervalsApiClient(string apiKey, string athleteId)
    {
        _athleteId = athleteId;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://intervals.icu")
        };

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"API_KEY:{apiKey}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<ApiProfile> GetProfileAsync()
    {
        var response = await _httpClient.GetAsync($"/api/v1/athlete/{_athleteId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<ApiProfile>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (profile == null)
        {
            throw new InvalidOperationException("Failed to deserialize profile");
        }

        return profile;
    }

    public async Task<List<ApiActivity>> GetActivitiesAsync(int limit = 10)
    {
        var oldest = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
        var newest = DateTime.Now.ToString("yyyy-MM-dd");

        var url = $"/api/v1/athlete/{_athleteId}/activities?oldest={oldest}&newest={newest}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var activities = JsonSerializer.Deserialize<List<ApiActivity>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return (activities ?? new List<ApiActivity>())
            .OrderByDescending(a => a.StartDateLocal ?? a.StartDate ?? DateTime.MinValue)
            .Take(limit)
            .ToList();
    }

    public async Task<ApiActivity> GetActivityAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/activity/{id}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var activity = JsonSerializer.Deserialize<ApiActivity>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (activity == null)
        {
            throw new InvalidOperationException($"Failed to deserialize activity {id}");
        }

        return activity;
    }

    public async Task<List<ApiWellness>> GetWellnessAsync(int days = 90)
    {
        var oldest = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        var newest = DateTime.Now.ToString("yyyy-MM-dd");

        var url = $"/api/v1/athlete/{_athleteId}/wellness?oldest={oldest}&newest={newest}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var wellness = JsonSerializer.Deserialize<List<ApiWellness>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return wellness ?? new List<ApiWellness>();
    }
}
