using System;

namespace DigitalTwin.Models;

public class UserSettings
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string EncryptedValue { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
