using System.Windows;
using System.Windows.Controls;
using DigitalTwin.Services.Localization;
using DigitalTwin.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalTwin.Views;

public partial class SettingsView : Page
{
    public SettingsView()
    {
        InitializeComponent();
        if (App.ServiceProvider != null)
        {
            DataContext = App.ServiceProvider.GetService<SettingsViewModel>();
        }
    }

    private void SetEnglish(object sender, RoutedEventArgs e)
    {
        LocalizationService.Instance.SetLanguage("en");
        if (DataContext is SettingsViewModel vm)
        {
            vm.StatusMessage = "Language changed to English! ✅";
        }
    }

    private void SetTurkish(object sender, RoutedEventArgs e)
    {
        LocalizationService.Instance.SetLanguage("tr");
        if (DataContext is SettingsViewModel vm)
        {
            vm.StatusMessage = "Dil Türkçe olarak değiştirildi! ✅";
        }
    }
}
