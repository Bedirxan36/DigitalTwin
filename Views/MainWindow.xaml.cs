using System;
using System.Windows;
using System.Windows.Media.Animation;
using DigitalTwin.Services.Localization;

namespace DigitalTwin.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavigateWithAnimation(new DashboardView());
        
        // Subscribe to language changes
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    private void NavigateWithAnimation(object page)
    {
        // Fade out current page
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2));
        fadeOut.Completed += (s, e) =>
        {
            ContentFrame.Navigate(page);
            
            // Fade in new page
            var fadeIn = FindResource("PageFadeIn") as Storyboard;
            if (fadeIn != null)
            {
                ContentFrame.BeginStoryboard(fadeIn);
            }
        };
        
        ContentFrame.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Refresh current page to apply new language
        var currentPage = ContentFrame.Content;
        ContentFrame.Content = null;
        ContentFrame.Content = currentPage;
    }

    private void NavigateToDashboard(object sender, RoutedEventArgs e)
    {
        NavigateWithAnimation(new DashboardView());
    }

    private void NavigateToInsights(object sender, RoutedEventArgs e)
    {
        NavigateWithAnimation(new InsightsView());
    }

    private void NavigateToFocusSession(object sender, RoutedEventArgs e)
    {
        NavigateWithAnimation(new FocusSessionView());
    }

    private void NavigateToRules(object sender, RoutedEventArgs e)
    {
        NavigateWithAnimation(new RulesView());
    }

    private void NavigateToSettings(object sender, RoutedEventArgs e)
    {
        NavigateWithAnimation(new SettingsView());
    }

    private void NavigateToAbout(object sender, RoutedEventArgs e)
    {
        NavigateWithAnimation(new AboutView());
    }

    protected override void OnClosed(EventArgs e)
    {
        LocalizationService.Instance.LanguageChanged -= OnLanguageChanged;
        base.OnClosed(e);
    }
}
