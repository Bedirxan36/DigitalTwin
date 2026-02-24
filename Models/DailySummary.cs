using System;

namespace DigitalTwin.Models;

public class DailySummary
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalActiveSeconds { get; set; }
    public int TotalIdleSeconds { get; set; }
    public int ProductiveSeconds { get; set; }
    public int DistractionSeconds { get; set; }
    public int FocusSessionCount { get; set; }
    public double AverageProductivityScore { get; set; }
    public string? TopProductiveHours { get; set; } // JSON: [14,15,16]
    public string? TopDistractingApps { get; set; } // JSON: [{app, seconds}]
}
