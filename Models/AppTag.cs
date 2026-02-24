using System;

namespace DigitalTwin.Models;

public enum TagType
{
    Productive,
    Neutral,
    Distraction
}

public class AppTag
{
    public int Id { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public TagType Type { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
