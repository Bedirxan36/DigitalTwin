using System;

namespace DigitalTwin.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string WindowTitle { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool IsIdle { get; set; }
    public int DurationSeconds { get; set; }
    public int? FocusSessionId { get; set; }
    public FocusSession? FocusSession { get; set; }
}
