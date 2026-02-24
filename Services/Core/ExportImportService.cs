using DigitalTwin.Services.Security;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DigitalTwin.Services.Core;

public class ExportImportService
{
    private readonly EncryptionService _encryptionService;

    public ExportImportService(EncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public async Task<string> ExportData(string outputPath)
    {
        if (!_encryptionService.IsInitialized)
            throw new InvalidOperationException("Encryption not initialized");

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DigitalTwin",
            "digitaltwin.db"
        );

        // Read database file
        var dbBytes = await File.ReadAllBytesAsync(dbPath);
        var dbBase64 = Convert.ToBase64String(dbBytes);

        // Encrypt
        var encrypted = _encryptionService.Encrypt(dbBase64);

        // Create export package
        var exportData = new
        {
            Version = "1.0",
            ExportDate = DateTime.UtcNow,
            Data = encrypted
        };

        var json = JsonSerializer.Serialize(exportData);
        await File.WriteAllTextAsync(outputPath, json);

        return outputPath;
    }

    public async Task ImportData(string importPath)
    {
        if (!_encryptionService.IsInitialized)
            throw new InvalidOperationException("Encryption not initialized");

        var json = await File.ReadAllTextAsync(importPath);
        var importData = JsonSerializer.Deserialize<JsonElement>(json);

        var encrypted = importData.GetProperty("Data").GetString()!;
        var decrypted = _encryptionService.Decrypt(encrypted);
        var dbBytes = Convert.FromBase64String(decrypted);

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DigitalTwin",
            "digitaltwin.db"
        );

        // Backup existing
        if (File.Exists(dbPath))
        {
            File.Copy(dbPath, dbPath + ".backup", true);
        }

        await File.WriteAllBytesAsync(dbPath, dbBytes);
    }
}
