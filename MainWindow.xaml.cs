using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;

namespace WPFWeatherApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{



    private readonly CacheService _cache;

    private readonly APIService _apiService;
    public MainWindow(CacheService cache, APIService apiService)
    {
        _cache = cache;
        _apiService = apiService;
        Title = "Weather App";
        Width = 500;
        Height = 400;

        var tabControl = new TabControl();

        tabControl.Items.Add(new TabItem
        {
            Header = "Current Forecast",
            Content = new CurrentForecastTab(_cache, _apiService),
        });
        tabControl.Items.Add(new TabItem
        {
            Header = "Future Forecast",
            Content = new FutureForecastTab(_cache, _apiService)
        });
        this.SizeToContent = SizeToContent.WidthAndHeight;

        tabControl.SelectionChanged += (s, e) =>
        {
            this.SizeToContent = SizeToContent.WidthAndHeight;
        };

        Content = tabControl;
    }

}