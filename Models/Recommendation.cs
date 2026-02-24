using System;

namespace DigitalTwin.Models;

public enum RecommendationType
{
    OptimalWorkTime,
    AvoidDistraction,
    FocusImprovement,
    BreakSuggestion
}

public class Recommendation
{
    public int Id { get; set; }
    public DateTime GeneratedAt { get; set; }
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty; // Neden bu öneri
    public double ConfidenceScore { get; set; }
    public bool IsRead { get; set; }
}
