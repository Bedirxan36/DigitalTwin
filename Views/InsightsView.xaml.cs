using System.Windows.Controls;
using DigitalTwin.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalTwin.Views;

public partial class InsightsView : Page
{
    public InsightsView()
    {
        InitializeComponent();
        if (App.ServiceProvider != null)
        {
            DataContext = App.ServiceProvider.GetService<InsightsViewModel>();
        }
    }
}
