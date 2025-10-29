using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls;

public class APIService
{
    private string _API_KEY;
    private string _baseURL;
    private HttpClient _client;
    public APIService()
    {
        var variableName = "WEATHER_API_KEY";
        // Load the API KEY. The key CANNOT be null or else no request can be made
        _API_KEY = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User)!;
        if (string.IsNullOrEmpty(variableName))
        {
            Console.WriteLine($"Environment variable '{_API_KEY}' is not set.");
        }
        _baseURL = "http://api.weatherapi.com/v1";
        _client = new HttpClient();
    }

    public async Task<JsonNode> MakeRequest(string town, string state, string country)
    {
        return await GetWeatherForecast(town, state, country, "14");
    }

    private async Task<JsonNode> GetCurrentWeather(string town, string state)
    {
        var request = "/current.json?key=" + _API_KEY + "&q=" + town + ", " + state + "&aqi=no";
        try
        {
            Console.WriteLine("Attempting an API call....");
            HttpResponseMessage response = await _client.GetAsync(_baseURL + request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Successfully fetched API response....");
            return JsonNode.Parse(responseBody)!;

        }

        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            return JsonNode.Parse("{\"Status\": \"Failed to get the request\"}")!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
            return JsonNode.Parse("{\"Status\": \"An unexpected error occurred.\"}")!;
        }

    }

    private async Task<JsonNode> GetWeatherForecast(string town, string state, string country, string days)
    {
        string request;
        if (!country.Equals(String.Empty))
        {
            request = "/forecast.json?key=" + _API_KEY + "&q=" + town + ", " + country + "&days=" + days + "&aqi=no&alerts=no";
        }
        else
        {
            request = "/forecast.json?key=" + _API_KEY + "&q=" + town + ", " + state + "&days=" + days + "&aqi=no&alerts=no";
        }
        try
        {
            Console.WriteLine("Attempting an API call....");
            HttpResponseMessage response = await _client.GetAsync(_baseURL + request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Successfully fetched API response....");
            return JsonNode.Parse(responseBody)!;

        }

        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            return JsonNode.Parse("{\"Status\": \"Failed to get the request\"}")!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
            return JsonNode.Parse("{\"Status\": \"An unexpected error occurred.\"}")!;
        }

    }
}