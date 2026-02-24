using DigitalTwin.Data;
using DigitalTwin.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DigitalTwin.Services.Core;

public class AnalysisService
{
    private readonly DigitalTwinDbContext _context;

    public AnalysisService()
    {
        _context = new DigitalTwinDbContext();
    }

    public async Task<DailySummary> GenerateDailySummary(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var activities = await _context.ActivityLogs
            .Where(a => a.Timestamp >= startOfDay && a.Timestamp < endOfDay)
            .ToListAsync();

        var focusSessions = await _context.FocusSessions
            .Where(f => f.StartTime >= startOfDay && f.StartTime < endOfDay)
            .ToListAsync();

        var totalActive = activities.Where(a => !a.IsIdle).Sum(a => a.DurationSeconds);
        var totalIdle = activities.Where(a => a.IsIdle).Sum(a => a.DurationSeconds);

        var productiveSeconds = await CalculateProductiveTime(activities);
        var distractionSeconds = await CalculateDistractionTime(activities);

        var topHours = GetTopProductiveHours(activities);
        var topDistractingApps = GetTopDistractingApps(activities);

        var summary = new DailySummary
        {
            Date = date.Date,
            TotalActiveSeconds = totalActive,
            TotalIdleSeconds = totalIdle,
            ProductiveSeconds = productiveSeconds,
            DistractionSeconds = distractionSeconds,
            FocusSessionCount = focusSessions.Count,
            AverageProductivityScore = focusSessions.Any() ? focusSessions.Average(f => f.ProductivityScore) : 0,
            TopProductiveHours = JsonSerializer.Serialize(topHours),
            TopDistractingApps = JsonSerializer.Serialize(topDistractingApps)
        };

        var existing = await _context.DailySummaries.FirstOrDefaultAsync(d => d.Date == date.Date);
        if (existing != null)
        {
            _context.DailySummaries.Remove(existing);
        }

        _context.DailySummaries.Add(summary);
        await _context.SaveChangesAsync();

        return summary;
    }

    public async Task<List<Recommendation>> GenerateRecommendations()
    {
        var recommendations = new List<Recommendation>();
        var last7Days = await _context.DailySummaries
            .Where(d => d.Date >= DateTime.Now.AddDays(-7))
            .OrderByDescending(d => d.Date)
            .ToListAsync();

        if (last7Days.Count < 3) return recommendations;

        // Get localization service
        var loc = Services.Localization.LocalizationService.Instance;

        // Optimal work time recommendation
        var optimalHours = AnalyzeOptimalWorkHours(last7Days);
        if (optimalHours.Any())
        {
            var hoursText = string.Join(", ", optimalHours.Select(h => $"{h}:00-{h + 1}:00"));
            var productivity = CalculateProductivityPercentage(last7Days, optimalHours);
            
            recommendations.Add(new Recommendation
            {
                GeneratedAt = DateTime.Now,
                Type = RecommendationType.OptimalWorkTime,
                Title = loc.CurrentLanguage == "tr" 
                    ? "En Verimli Çalışma Saatlerin" 
                    : "Your Most Productive Hours",
                Message = loc.CurrentLanguage == "tr"
                    ? $"Verilerine göre {hoursText} saatleri arasında en üretkensin."
                    : $"According to your data, you're most productive between {hoursText}.",
                Reasoning = loc.CurrentLanguage == "tr"
                    ? $"Son 7 günde bu saatlerde ortalama %{productivity:F0} verimlilik gösterdin."
                    : $"You showed an average of {productivity:F0}% productivity during these hours in the last 7 days.",
                ConfidenceScore = 0.85,
                IsRead = false
            });
        }

        // Distraction warning
        var topDistractions = AnalyzeTopDistractions(last7Days);
        if (topDistractions.Any())
        {
            var topApp = topDistractions.First();
            recommendations.Add(new Recommendation
            {
                GeneratedAt = DateTime.Now,
                Type = RecommendationType.AvoidDistraction,
                Title = loc.CurrentLanguage == "tr"
                    ? "Dikkat Dağıtan Uygulama"
                    : "Distracting Application",
                Message = loc.CurrentLanguage == "tr"
                    ? $"{topApp.Key} uygulaması günde ortalama {topApp.Value / 60} dakika dikkatini dağıtıyor."
                    : $"{topApp.Key} is distracting you for an average of {topApp.Value / 60} minutes per day.",
                Reasoning = loc.CurrentLanguage == "tr"
                    ? "Bu uygulamayı odak oturumları sırasında kapatmayı dene."
                    : "Try closing this app during focus sessions.",
                ConfidenceScore = 0.75,
                IsRead = false
            });
        }

        foreach (var rec in recommendations)
        {
            _context.Recommendations.Add(rec);
        }
        await _context.SaveChangesAsync();

        return recommendations;
    }

    private async Task<int> CalculateProductiveTime(List<ActivityLog> activities)
    {
        var productiveTags = await _context.AppTags
            .Where(t => t.Type == TagType.Productive)
            .Select(t => t.ProcessName)
            .ToListAsync();

        return activities
            .Where(a => productiveTags.Contains(a.ProcessName))
            .Sum(a => a.DurationSeconds);
    }

    private async Task<int> CalculateDistractionTime(List<ActivityLog> activities)
    {
        var distractionTags = await _context.AppTags
            .Where(t => t.Type == TagType.Distraction)
            .Select(t => t.ProcessName)
            .ToListAsync();

        return activities
            .Where(a => distractionTags.Contains(a.ProcessName))
            .Sum(a => a.DurationSeconds);
    }

    private List<int> GetTopProductiveHours(List<ActivityLog> activities)
    {
        return activities
            .GroupBy(a => a.Timestamp.Hour)
            .OrderByDescending(g => g.Sum(a => a.DurationSeconds))
            .Take(3)
            .Select(g => g.Key)
            .ToList();
    }

    private List<object> GetTopDistractingApps(List<ActivityLog> activities)
    {
        return activities
            .GroupBy(a => a.ProcessName)
            .Select(g => new { app = g.Key, seconds = g.Sum(a => a.DurationSeconds) })
            .OrderByDescending(x => x.seconds)
            .Take(5)
            .Cast<object>()
            .ToList();
    }

    private List<int> AnalyzeOptimalWorkHours(List<DailySummary> summaries)
    {
        var hourCounts = new Dictionary<int, int>();
        
        foreach (var summary in summaries)
        {
            if (string.IsNullOrEmpty(summary.TopProductiveHours)) continue;
            
            var hours = JsonSerializer.Deserialize<List<int>>(summary.TopProductiveHours);
            if (hours != null)
            {
                foreach (var hour in hours)
                {
                    hourCounts[hour] = hourCounts.GetValueOrDefault(hour, 0) + 1;
                }
            }
        }

        return hourCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private Dictionary<string, int> AnalyzeTopDistractions(List<DailySummary> summaries)
    {
        var appTotals = new Dictionary<string, int>();

        foreach (var summary in summaries)
        {
            if (string.IsNullOrEmpty(summary.TopDistractingApps)) continue;

            var apps = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(summary.TopDistractingApps);
            if (apps != null)
            {
                foreach (var app in apps)
                {
                    var appName = app["app"].ToString() ?? "";
                    var seconds = Convert.ToInt32(app["seconds"]);
                    appTotals[appName] = appTotals.GetValueOrDefault(appName, 0) + seconds;
                }
            }
        }

        return appTotals.OrderByDescending(kvp => kvp.Value).Take(3).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private double CalculateProductivityPercentage(List<DailySummary> summaries, List<int> hours)
    {
        var totalProductive = summaries.Sum(s => s.ProductiveSeconds);
        var totalActive = summaries.Sum(s => s.TotalActiveSeconds);
        return totalActive > 0 ? (totalProductive * 100.0 / totalActive) : 0;
    }
}
