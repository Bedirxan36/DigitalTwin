using System;
using System.Collections.Generic;

namespace DigitalTwin.Models;

public class FocusSession
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Goal { get; set; }
    public int InterruptionCount { get; set; }
    public int TotalFocusSeconds { get; set; }
    public double ProductivityScore { get; set; }
    public ICollection<ActivityLog> Activities { get; set; } = new List<ActivityLog>();
}
