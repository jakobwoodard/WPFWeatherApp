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
using Microsoft.VisualBasic;

namespace WPFWeatherApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private TextBox townTextbox;
    private TextBox stateTextbox;
    private Button buttonSubmit;

    private Grid rootlayout;
    private Grid searchSection;
    private StackPanel resultsSection;
    private TextBlock resultsBlock;

    private readonly CacheService _cache;

    private readonly APIService _apiService;
    public MainWindow(CacheService cache, APIService apiService)
    {
        _cache = cache;
        _apiService = apiService;
        // Grid searchSection which is the content of the Window
        rootlayout = new Grid
        {
            Background = Brushes.White
        };




        searchSection = new() { Margin = new Thickness(5) };
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.ColumnDefinitions.Add(new ColumnDefinition());
        searchSection.ColumnDefinitions.Add(new ColumnDefinition());

        // Create the two labels, assign the second label to the second row
        Label labelName = new() { Content = "Town:" };
        searchSection.Children.Add(labelName);

        Label labelAddress = new() { Content = "State name or code:" };
        Grid.SetRow(labelAddress, 1);
        searchSection.Children.Add(labelAddress);

        // Create the two textboxes, assign both to the second column and
        // assign the second textbox to the second row.
        townTextbox = new() { Margin = new Thickness(2) };
        Grid.SetColumn(townTextbox, 1);
        searchSection.Children.Add(townTextbox);

        stateTextbox = new() { Margin = new Thickness(2) };
        Grid.SetRow(stateTextbox, 1);
        Grid.SetColumn(stateTextbox, 1);
        searchSection.Children.Add(stateTextbox);

        // Create the two buttons, assign both to the third row and
        // assign the second button to the second column.
        Button buttonReset = new() { Margin = new Thickness(2), Content = "Reset" };
        buttonReset.Click += Reset_Click;
        Grid.SetRow(buttonReset, 2);
        searchSection.Children.Add(buttonReset);

        buttonSubmit = new() { Margin = new Thickness(2), Content = "Submit" };
        buttonSubmit.Click += async (sender, e) => await Submit_ClickAsync(sender, e);
        Grid.SetColumn(buttonSubmit, 1);
        Grid.SetRow(buttonSubmit, 2);
        searchSection.Children.Add(buttonSubmit);

        resultsSection = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visibility = Visibility.Collapsed // start hidden
        };

        resultsBlock = new TextBlock
        {
            FontSize = 18,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var backButton = new Button
        {
            Content = "Back to Search",
            Width = 150
        };
        backButton.Click += (s, e) => ShowSearchSection();

        resultsSection.Children.Add(resultsBlock);
        resultsSection.Children.Add(backButton);

        rootlayout.Children.Add(searchSection);
        rootlayout.Children.Add(resultsSection);



        // bind this as the main
        this.Title = "Current Weather";
        this.Height = double.NaN;
        this.Width = 400;
        this.SizeToContent = SizeToContent.Height;
        this.Content = rootlayout;

    }

    private async Task Submit_ClickAsync(object sender, RoutedEventArgs e)
    {
        buttonSubmit.IsEnabled = false; // disable button until processing is done

        string town = townTextbox.Text.ToLower();
        string state = stateTextbox.Text.ToLower();

        await SubmitFormAsync(town, state);

        buttonSubmit.IsEnabled = true; // enable button after processing
    }

    private async Task SubmitFormAsync(string town, string state)
    {
        // Hide search section and show loading state
        searchSection.Visibility = Visibility.Collapsed;
        resultsSection.Visibility = Visibility.Visible;
        resultsBlock.Text = "Loading...";


        // Attempt to retrieve data
        if (_cache.TryGet<object>("current_" + town + state, out var weather))
        {
            string jsonResponse = _cache.Get("current_" + town + state).ToString();
            Console.WriteLine("Using cached data...");
            JsonNode? root = JsonNode.Parse(jsonResponse);
            resultsBlock.Text = root?["current"]?["temp_f"]?.ToString();
            Console.WriteLine(root?["current"]?["temp_f"]?.ToString());
        }

        // date not in cache, so add it
        else
        {
            Console.WriteLine("No cached data available... adding now....");
            JsonNode jsonNode = await _apiService.makeRequest("current", town, state);
            _cache.Set("current_" + town + state, jsonNode, TimeSpan.FromMinutes(5));
            resultsBlock.Text = jsonNode["current"]?["temp_f"]?.ToString();
            Console.WriteLine(jsonNode["current"]?["temp_f"]?.ToString());
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        townTextbox.Clear();
        stateTextbox.Clear();
    }

    // debug method to show all cache contents
    private void displayCacheContents()
    {
        if (_cache.GetAllKeys().ToList().Count == 0)
        {
            MessageBox.Show("The cache is empty.");
        }

        string result = "Cache contents: \n";
        foreach (var key in _cache.GetAllKeys())
        {
            if (_cache.TryGet<object>(key, out var value))
            {
                result += $"{key}: {value}\n";
            }
        }

        MessageBox.Show(result);
    }

    private void ShowSearchSection()
    {
        // clear forms before going back
        stateTextbox.Clear();
        townTextbox.Clear();


        // swap visibility
        resultsSection.Visibility = Visibility.Collapsed;
        searchSection.Visibility = Visibility.Visible;
    }

}