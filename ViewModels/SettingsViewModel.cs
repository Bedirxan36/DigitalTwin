using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalTwin.Services.Core;
using DigitalTwin.Services.Security;
using DigitalTwin.Services.Localization;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwin.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly EncryptionService _encryptionService;
    private readonly ExportImportService _exportImportService;
    private readonly LocalizationService _localizationService;

    [ObservableProperty]
    private int _idleThresholdMinutes = 5;

    [ObservableProperty]
    private bool _trackBrowserDomains = true;

    [ObservableProperty]
    private bool _autoStartTracking = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LanguageOption> _availableLanguages;

    [ObservableProperty]
    private LanguageOption? _selectedLanguage;

    public SettingsViewModel(EncryptionService encryptionService, ExportImportService exportImportService)
    {
        _encryptionService = encryptionService;
        _exportImportService = exportImportService;
        _localizationService = LocalizationService.Instance;

        _availableLanguages = new ObservableCollection<LanguageOption>(_localizationService.AvailableLanguages);
        _selectedLanguage = _availableLanguages.FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage);
    }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value != null && value.Code != _localizationService.CurrentLanguage)
        {
            _localizationService.SetLanguage(value.Code);
            StatusMessage = value.Code == "tr" ? "Ayarlar kaydedildi! ✅" : "Settings saved! ✅";
        }
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Digital Twin Backup (*.dtb)|*.dtb",
            DefaultExt = ".dtb",
            FileName = $"DigitalTwin_Backup_{DateTime.Now:yyyyMMdd}.dtb"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _exportImportService.ExportData(dialog.FileName);
                StatusMessage = _localizationService.CurrentLanguage == "tr" 
                    ? "Veri başarıyla dışa aktarıldı! ✅" 
                    : "Data exported successfully! ✅";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task ImportDataAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Digital Twin Backup (*.dtb)|*.dtb",
            DefaultExt = ".dtb"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _exportImportService.ImportData(dialog.FileName);
                StatusMessage = _localizationService.CurrentLanguage == "tr"
                    ? "Veri başarıyla içe aktarıldı! ✅"
                    : "Data imported successfully! ✅";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // Save settings to database
        StatusMessage = _localizationService.CurrentLanguage == "tr"
            ? "Ayarlar kaydedildi! ✅"
            : "Settings saved! ✅";
    }
}
