using System.Security;
using System.Windows.Controls;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using System.Collections;
using System.Configuration;


public class FutureForecastTab : UserControl
{
    private TextBox townTextbox;
    private ComboBox stateDropdown;
    private ComboBox daysDropdown;
    private Label countryLabel;
    private TextBox countryTextBox;
    private Button buttonSubmit;
    private Button backButton;

    private Grid rootlayout;
    private Grid searchSection;
    private StackPanel resultsSection;
    private TextBlock resultsBlockTemp;
    private TextBlock resultsBlockTown;
    private TextBlock resultsBlockState;
    private ScrollViewer scrollViewer;
    private WrapPanel forecastPanel;


    private readonly CacheService _cache;

    private readonly APIService _apiService;
    public FutureForecastTab(CacheService cache, APIService apiService)
    {
        _cache = cache;
        _apiService = apiService;
        rootlayout = new Grid
        {
            Background = Brushes.Black
        };




        searchSection = new() { Margin = new Thickness(5) };
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.RowDefinitions.Add(new RowDefinition());
        searchSection.ColumnDefinitions.Add(new ColumnDefinition());
        searchSection.ColumnDefinitions.Add(new ColumnDefinition());

        // Create the two labels, assign the second label to the second row
        Label labelName = new() { Content = "City/Town:", Foreground = Brushes.DodgerBlue };
        searchSection.Children.Add(labelName);

        Label labelAddress = new() { Content = "State name or code:", Foreground = Brushes.DodgerBlue };
        Grid.SetRow(labelAddress, 1);
        searchSection.Children.Add(labelAddress);

        // Create the two textboxes, assign both to the second column and
        // assign the second textbox to the second row.
        townTextbox = new() { Margin = new Thickness(2) };
        Grid.SetColumn(townTextbox, 1);
        searchSection.Children.Add(townTextbox);

        stateDropdown = new() { Margin = new Thickness(2) };
        Grid.SetRow(stateDropdown, 1);
        Grid.SetColumn(stateDropdown, 1);
        stateDropdown.ItemsSource = GetUsStateAbbreviations();
        stateDropdown.SelectionChanged += StateDropdown_SelectionChanged;
        searchSection.Children.Add(stateDropdown);

        countryLabel = new() { Content = "Country Name:", Foreground = Brushes.DodgerBlue };
        Grid.SetRow(countryLabel, 2);
        countryLabel.Visibility = Visibility.Collapsed;
        searchSection.Children.Add(countryLabel);

        countryTextBox = new() { Margin = new Thickness(2) };
        Grid.SetColumn(countryTextBox, 1);
        Grid.SetRow(countryTextBox, 2);
        countryTextBox.Visibility = Visibility.Collapsed;
        searchSection.Children.Add(countryTextBox);

        Label daysLabel = new() { Content = "Days:", Foreground = Brushes.DodgerBlue };
        Grid.SetRow(daysLabel, 3);
        searchSection.Children.Add(daysLabel);

        daysDropdown = new() { Margin = new Thickness(2) };
        Grid.SetRow(daysDropdown, 3);
        Grid.SetColumn(daysDropdown, 1);
        daysDropdown.ItemsSource = GetDaysSelection();
        searchSection.Children.Add(daysDropdown);

        // Create the two buttons, assign both to the third row and
        // assign the second button to the second column.
        Button buttonReset = new() { Margin = new Thickness(2), Content = "Reset" };
        buttonReset.Click += Reset_Click;
        Grid.SetRow(buttonReset, 4);
        searchSection.Children.Add(buttonReset);

        buttonSubmit = new() { Margin = new Thickness(2), Content = "Submit" };
        buttonSubmit.Click += async (sender, e) => await Submit_ClickAsync(sender, e);
        Grid.SetColumn(buttonSubmit, 1);
        Grid.SetRow(buttonSubmit, 4);
        searchSection.Children.Add(buttonSubmit);

        resultsSection = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visibility = Visibility.Collapsed // start hidden
        };

        resultsBlockTown = new TextBlock
        {
            FontSize = 20,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2),
            Foreground = Brushes.DodgerBlue
        };

        resultsBlockState = new TextBlock
        {
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20),
            Foreground = Brushes.DodgerBlue
        };

        resultsBlockTemp = new TextBlock
        {
            FontSize = 25,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20),
            Foreground = Brushes.DodgerBlue
        };


        backButton = new Button
        {
            Content = "Back to Search",
            Width = 150
        };
        backButton.Click += (s, e) => ShowSearchSection();

        resultsSection.Children.Add(resultsBlockTown);
        resultsSection.Children.Add(resultsBlockState);
        resultsSection.Children.Add(resultsBlockTemp);

        resultsSection.Children.Add(backButton);

        rootlayout.Children.Add(searchSection);
        rootlayout.Children.Add(resultsSection);



        // bind this as the main
        //Title = "Current Weather";
        Height = double.NaN;
        Width = 400;
        //SizeToContent = SizeToContent.Height;
        Content = rootlayout;

    }

    private IEnumerable GetDaysSelection()
    {
        return new List<string>
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14"
        };
    }

    private async Task Submit_ClickAsync(object sender, RoutedEventArgs e)
    {
        buttonSubmit.IsEnabled = false; // disable button until processing is done

        string town = townTextbox.Text.ToLower();
        string state = stateDropdown.Text.ToLower();
        string country = countryTextBox.Text.ToLower();
        string days = daysDropdown.Text;
        if (state.Equals("international"))
        {
            await SubmitFormAsyncInternational(town.Trim(), country.Trim(), days);
        }
        else
        {
            await SubmitFormAsync(town.Trim(), state.Trim(), days);
        }



        buttonSubmit.IsEnabled = true; // enable button after processing
    }

    private async Task SubmitFormAsync(string town, string state, string days)
    {
        // Hide search section and show loading state
        searchSection.Visibility = Visibility.Collapsed;
        resultsSection.Visibility = Visibility.Visible;
        resultsBlockTemp.Text = "Loading...";
        JsonNode root;


        // Attempt to retrieve data
        if (_cache.TryGet<object>(days + "forecast_" + town + state, out var weather))
        {
            string jsonResponse = _cache.Get(days + "forecast_" + town + state).ToString();
            Console.WriteLine("Using cached data...");
            root = JsonNode.Parse(jsonResponse);
            resultsBlockTown.Text = $"{makeTitleCase(town)}";
            resultsBlockState.Text = $"{state.ToUpper()}";
            resultsBlockTemp.Text = root["current"]?["temp_f"]?.ToString() + "\u00b0F";
        }

        // date not in cache, so add it
        else
        {
            Console.WriteLine("No cached data available... adding now....");
            root = await _apiService.makeRequest("forecast", town, state, days);
            _cache.Set(days + "forecast_" + town + state, root, TimeSpan.FromMinutes(5));
            resultsBlockTown.Text = $"{makeTitleCase(town)}";
            resultsBlockState.Text = $"{state.ToUpper()}";
            resultsBlockTemp.Text = root["current"]?["temp_f"]?.ToString() + "\u00b0F";
        }


        forecastPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10)
        };

        scrollViewer = new ScrollViewer
        {
            Content = forecastPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };


        resultsSection.Children.Remove(backButton);

        // add in new sections for each day retrieved
        foreach (var dayNode in root["forecast"]?["forecastday"]?.AsArray())
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(5),
                Padding = new Thickness(10)
            };

            StackPanel dayStack = new StackPanel();

            dayStack.Children.Add(new TextBlock
            {
                FontSize = 25,
                Text = dayNode?["date"]?.ToString(),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = Brushes.DodgerBlue

            });

            dayStack.Children.Add(new TextBlock
            {
                FontSize = 25,
                Text = dayNode?["day"]?["avgtemp_f"]?.ToString() + "\u00b0F",
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = Brushes.DodgerBlue
            });

            border.Child = dayStack;
            forecastPanel.Children.Add(border);

        }

        resultsSection.Children.Add(scrollViewer);
        resultsSection.Children.Add(backButton);



    }

    private async Task SubmitFormAsyncInternational(string town, string country, string days)
    {
        // Hide search section and show loading state
        searchSection.Visibility = Visibility.Collapsed;
        resultsSection.Visibility = Visibility.Visible;
        resultsBlockTemp.Text = "Loading...";


        // Attempt to retrieve data
        if (_cache.TryGet<object>("current_" + town + country, out var weather))
        {
            string jsonResponse = _cache.Get("current_" + town + country).ToString();
            Console.WriteLine("Using cached data...");
            JsonNode? root = JsonNode.Parse(jsonResponse);
            resultsBlockTown.Text = $"{makeTitleCase(town)}";
            resultsBlockState.Text = $"{makeTitleCase(country)}";
            resultsBlockTemp.Text = root?["current"]?["temp_f"]?.ToString() + "\u00b0F";
            Console.WriteLine(root?["current"]?["temp_f"]?.ToString());
        }

        // date not in cache, so add it
        else
        {
            Console.WriteLine("No cached data available... adding now....");
            JsonNode jsonNode = await _apiService.makeRequest("current", town, country);
            _cache.Set("current_" + town + country, jsonNode, TimeSpan.FromMinutes(5));
            resultsBlockTown.Text = $"{makeTitleCase(town)}";
            resultsBlockState.Text = $"{makeTitleCase(country)}";
            resultsBlockTemp.Text = jsonNode["current"]?["temp_f"]?.ToString() + "\u00b0F";
            Console.WriteLine(jsonNode["current"]?["temp_f"]?.ToString());
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        townTextbox.Clear();
        stateDropdown.Text = String.Empty;
        countryTextBox.Clear();
    }

    private void ShowSearchSection()
    {
        // clear forms before going back
        stateDropdown.Text = String.Empty;
        townTextbox.Clear();
        countryTextBox.Clear();
        daysDropdown.Text = String.Empty;

        resultsBlockState.Text = String.Empty;
        resultsBlockTown.Text = String.Empty;
        resultsBlockTemp.Text = String.Empty;

        forecastPanel.Children.Clear();



        // swap visibility
        resultsSection.Visibility = Visibility.Collapsed;
        searchSection.Visibility = Visibility.Visible;
    }

    private List<string> GetUsStateAbbreviations()
    {
        return new List<string>
            {
                "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA",
                "HI","ID","IL","IN","IA","KS","KY","LA","ME","MD",
                "MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ",
                "NM","NY","NC","ND","OH","OK","OR","PA","RI","SC",
                "SD","TN","TX","UT","VT","VA","WA","WV","WI","WY", "International"
            };
    }

    private void StateDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string? selectedState = stateDropdown.SelectedItem as string;

        if (selectedState == "International")
        {
            countryLabel.Visibility = Visibility.Visible;
            countryTextBox.Visibility = Visibility.Visible;
        }
        else
        {
            countryLabel.Visibility = Visibility.Collapsed;
            countryTextBox.Visibility = Visibility.Collapsed;
            countryTextBox.Clear();
        }
    }

    // Helper method to take a string and return a title cased string (making the first letter of each word capital)
    private string makeTitleCase(string name)
    {
        string[] words = name.Split(" ");
        StringBuilder sb = new StringBuilder();
        foreach (string s in words)
        {
            sb.Append(char.ToUpper(s[0]) + s.Substring(1) + " ");
        }
        return sb.ToString().Trim();
    }


}