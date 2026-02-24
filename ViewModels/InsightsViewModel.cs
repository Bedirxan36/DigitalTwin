using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalTwin.Data;
using DigitalTwin.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwin.ViewModels;

public partial class InsightsViewModel : ViewModelBase
{
    private readonly DigitalTwinDbContext _context;

    [ObservableProperty]
    private ObservableCollection<DailySummary> _weeklySummaries = new();

    [ObservableProperty]
    private ObservableCollection<KeyValuePair<string, int>> _topApps = new();

    [ObservableProperty]
    private ObservableCollection<int> _productiveHours = new();

    [ObservableProperty]
    private string _selectedTimeRange = "Last 7 Days";

    public InsightsViewModel(DigitalTwinDbContext context)
    {
        _context = context;
        _ = LoadInsightsAsync();
    }

    [RelayCommand]
    private async Task LoadInsightsAsync()
    {
        try
        {
            var days = SelectedTimeRange switch
            {
                "Last 7 Days" => 7,
                "Last 30 Days" => 30,
                _ => 7
            };

            var summaries = await _context.DailySummaries
                .Where(d => d.Date >= DateTime.Today.AddDays(-days))
                .OrderByDescending(d => d.Date)
                .ToListAsync();

            WeeklySummaries = new ObservableCollection<DailySummary>(summaries);

            // Aggregate top apps (placeholder data for now)
            var appStats = new Dictionary<string, int>
            {
                { "Visual Studio Code", 180 },
                { "Chrome", 120 },
                { "Slack", 45 },
                { "Spotify", 30 }
            };

            TopApps = new ObservableCollection<KeyValuePair<string, int>>(
                appStats.OrderByDescending(kvp => kvp.Value).Take(10)
            );
        }
        catch (Exception)
        {
            // Handle errors gracefully
            TopApps = new ObservableCollection<KeyValuePair<string, int>>();
        }
    }

    [RelayCommand]
    private void ChangeTimeRange(string range)
    {
        SelectedTimeRange = range;
        _ = LoadInsightsAsync();
    }
}
