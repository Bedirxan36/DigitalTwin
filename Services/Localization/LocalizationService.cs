using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace DigitalTwin.Services.Localization;

public class LocalizationService
{
    private static LocalizationService? _instance;
    private ResourceManager _resourceManager;
    private CultureInfo _currentCulture;

    public static LocalizationService Instance => _instance ??= new LocalizationService();

    public event EventHandler? LanguageChanged;

    private LocalizationService()
    {
        _resourceManager = new ResourceManager("DigitalTwin.Resources.Strings", typeof(LocalizationService).Assembly);
        _currentCulture = CultureInfo.CurrentUICulture;
    }

    public string GetString(string key)
    {
        try
        {
            return _resourceManager.GetString(key, _currentCulture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    public void SetLanguage(string cultureName)
    {
        _currentCulture = new CultureInfo(cultureName);
        CultureInfo.CurrentUICulture = _currentCulture;
        CultureInfo.CurrentCulture = _currentCulture;
        
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string CurrentLanguage => _currentCulture.Name;

    public List<LanguageOption> AvailableLanguages => new()
    {
        new LanguageOption { Code = "en", Name = "English" },
        new LanguageOption { Code = "tr", Name = "Türkçe" }
    };
}

public class LanguageOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
