using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DigitalTwin.Data;

public static class DbInitializer
{
    public static void Initialize()
    {
        using var context = new DigitalTwinDbContext();
        
        // Create database and apply migrations
        context.Database.Migrate();
        
        // Seed default data if needed
        if (!context.UserSettings.Any())
        {
            context.UserSettings.Add(new Models.UserSettings
            {
                Key = "FirstRun",
                EncryptedValue = "true",
                UpdatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
        }
    }
}
