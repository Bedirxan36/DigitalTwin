using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalTwin.Models;
using DigitalTwin.Services.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwin.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ActivityTrackingService _trackingService;
    private readonly AnalysisService _analysisService;

    [ObservableProperty]
    private bool _isTracking;

    [ObservableProperty]
    private string _trackingStatus = "Takip durduruldu";

    [ObservableProperty]
    private string _currentActivity = "Idle";

    [ObservableProperty]
    private string _totalTimeToday = "0h 0m";

    [ObservableProperty]
    private string _focusSessionCount = "0";

    [ObservableProperty]
    private string _productivityScore = "0%";

    [ObservableProperty]
    private int _todayActiveMinutes;

    [ObservableProperty]
    private int _todayProductiveMinutes;

    [ObservableProperty]
    private ObservableCollection<ActivityLog> _recentActivities = new();

    [ObservableProperty]
    private ObservableCollection<Recommendation> _recentRecommendations = new();

    public DashboardViewModel(ActivityTrackingService trackingService, AnalysisService analysisService)
    {
        _trackingService = trackingService;
        _analysisService = analysisService;
        
        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    private void StartTracking()
    {
        _trackingService.StartTracking();
        IsTracking = true;
        TrackingStatus = "✅ Takip aktif";
    }

    [RelayCommand]
    private void StopTracking()
    {
        _trackingService.StopTracking();
        IsTracking = false;
        TrackingStatus = "⏸️ Takip durduruldu";
    }

    [RelayCommand]
    private void ToggleTracking()
    {
        if (IsTracking)
        {
            StopTracking();
        }
        else
        {
            StartTracking();
        }
    }

    [RelayCommand]
    private void StartFocusSession()
    {
        _trackingService.StartFocusSession("Focus Session");
    }

    [RelayCommand]
    private void EndFocusSession()
    {
        _trackingService.EndFocusSession();
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var summary = await _analysisService.GenerateDailySummary(DateTime.Today);
            
            TodayActiveMinutes = summary.TotalActiveSeconds / 60;
            TodayProductiveMinutes = summary.ProductiveSeconds / 60;
            
            // Format display values
            int hours = TodayActiveMinutes / 60;
            int minutes = TodayActiveMinutes % 60;
            TotalTimeToday = $"{hours}h {minutes}m";
            
            ProductivityScore = $"{(int)summary.AverageProductivityScore}%";

            var recommendations = await _analysisService.GenerateRecommendations();
            RecentRecommendations = new ObservableCollection<Recommendation>(recommendations.Take(5).ToList());
            
            // Load recent activities (placeholder - you can implement actual data loading)
            RecentActivities = new ObservableCollection<ActivityLog>();
        }
        catch (Exception)
        {
            // Handle errors gracefully
            TotalTimeToday = "0h 0m";
            ProductivityScore = "0%";
            FocusSessionCount = "0";
        }
    }
}
