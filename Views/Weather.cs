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


public class WeatherTab : UserControl
{
    private readonly TextBox townTextbox;
    private readonly ComboBox stateDropdown;
    private readonly ComboBox daysDropdown;
    private readonly Label countryLabel;
    private readonly TextBox countryTextBox;
    private readonly Button buttonSubmit;
    private readonly Button backButton;

    private readonly Grid rootlayout;
    private readonly Grid searchSection;
    private readonly StackPanel resultsSection;
    private readonly TextBlock resultsBlockTemp;
    private readonly TextBlock resultsBlockTown;
    private readonly TextBlock resultsBlockState;
    private ScrollViewer? scrollViewer;
    private StackPanel? forecastPanel;

    private readonly CacheService _cache;

    private readonly APIService _apiService;
    public WeatherTab(CacheService cache, APIService apiService)
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

    private static List<string> GetDaysSelection()
    {
        return
        [
            "Current", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14"
        ];
    }

    private async Task Submit_ClickAsync(object sender, RoutedEventArgs e)
    {
        buttonSubmit.IsEnabled = false; // disable button until processing is done

        string town = townTextbox.Text.ToLower();
        string state = stateDropdown.Text.ToLower();
        string country = countryTextBox.Text.ToLower();
        string days = daysDropdown.Text.ToLower();

        await SubmitFormAsync(town.Trim(), state.Trim(), country.Trim(), days.Trim());



        buttonSubmit.IsEnabled = true; // enable button after processing
    }

    private async Task SubmitFormAsync(string town, string state, string country, string days)
    {
        // Hide search section and show loading state
        searchSection.Visibility = Visibility.Collapsed;
        resultsSection.Visibility = Visibility.Visible;
        resultsBlockTemp.Text = "Loading...";
        JsonNode root;

        // Attempt to retrieve data
        if (_cache.TryGet<object>(town + country + state, out var weather))
        {
            //string jsonResponse = _cache.Get(town + country + state).ToString();
            Console.WriteLine("Using cached data...");
            root = JsonNode.Parse(weather.ToString()!)!;
            resultsBlockTown.Text = $"{MakeTitleCase(town)}";
            if (!country.Equals(string.Empty))
            {
                resultsBlockState.Text = $"{MakeTitleCase(country)}";
            }
            else
            {
                resultsBlockState.Text = $"{state.ToUpper()}";
            }

            resultsBlockTemp.Text = root["current"]?["temp_f"]?.ToString() + "\u00b0F";
        }

        // date not in cache, so add it
        else
        {
            Console.WriteLine("No cached data available... adding now....");
            root = await _apiService.MakeRequest(town, state, country);
            _cache.Set(town + country + state, root, TimeSpan.FromMinutes(5));
            resultsBlockTown.Text = $"{MakeTitleCase(town)}";
            if (!country.Equals(string.Empty))
            {
                resultsBlockState.Text = $"{MakeTitleCase(country)}";
            }
            else
            {
                resultsBlockState.Text = $"{state.ToUpper()}";
            }
            resultsBlockTemp.Text = root["current"]?["temp_f"]?.ToString() + "\u00b0F";
        }

        // If we want a multi-day forecast, create a new panel for the forecast objects
        if (!days.Equals("current"))
        {

            // To be used for multi-day forecast display
            forecastPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10)
            };

            // a scroll viewer to allow user to see all days if they don't normally fit the screen
            scrollViewer = new ScrollViewer
            {
                Content = forecastPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };


            // To have the back button at the bottom of the page, we need to remove it before adding other elements
            resultsSection.Children.Remove(backButton);

            // get the entire 14-day forecast (that's how it's stored in the cache)
            var dayNodes = root?["forecast"]?["forecastday"]?.AsArray();

            // but only create elements for the desired day amount
            for (int i = 0; i < int.Parse(days); i++)
            {
                // the data for the ith day
                var dayNode = dayNodes?[i];

                // border to separate elements
                var border = new Border
                {
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
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

            // after we have created the scroll viewer with all the new elements, add it to the results section
            resultsSection.Children.Add(scrollViewer);
            // add the "back" button back
            resultsSection.Children.Add(backButton);
        }
    }


    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        townTextbox.Clear();
        stateDropdown.Text = string.Empty;
        countryTextBox.Clear();
    }

    // debug method to show all cache contents
    private void DisplayCacheContents()
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
        stateDropdown.Text = string.Empty;
        townTextbox.Clear();
        countryTextBox.Clear();
        daysDropdown.Text = string.Empty;

        resultsBlockState.Text = string.Empty;
        resultsBlockTown.Text = string.Empty;
        resultsBlockTemp.Text = string.Empty;

        forecastPanel?.Children.Clear();


        // swap visibility
        resultsSection.Visibility = Visibility.Collapsed;
        searchSection.Visibility = Visibility.Visible;
    }

    private static List<string> GetUsStateAbbreviations()
    {
        return
            [
                "AK","AL","AR","AZ","CA","CO","CT","DE","FL","GA",
                "HI","IA","ID","IL","IN","KS","KY","LA","MA","MD",
                "ME","MI","MN","MO","MS","MT","NC","ND","NE","NH",
                "NJ","NM","NV","NY","OH","OK","OR","PA","RI","SC",
                "SD","TN","TX","UT","VA","VT","WA","WI","WV","WY","International"

            ];
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
    private static string MakeTitleCase(string name)
    {
        string[] words = name.Split(" ");
        StringBuilder sb = new();
        foreach (string s in words)
        {
            sb.Append(char.ToUpper(s[0]) + s[1..] + " ");
        }
        return sb.ToString().Trim();
    }


}