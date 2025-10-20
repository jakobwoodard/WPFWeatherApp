This is a project that I worked on over the course of a few days to dive deeper into C# and WPF. It uses all C# and no XAML to generate what is on the screen.

The app currently has 2 features: getting the current weather report for a given location (international included), and getting the weather forecast (up to 14 days past the current day) for a non-international location.

The API that I am using is from WeatherAPI.com. The actual API Key is, obviously, not included in this project for security concerns.

I also implemented a caching system. The system is fully functional for current weather reports and exists for forecast reports but I don't currently like how it works (caching based on days requested in the forecast). The caching is in place to not only speed up the 
application as a whole, but to also prevent duplicate API calls to save tokens.

Some future improvements that I would like to add to this project are the following:
<ul>
  <li>Better visuals for both current and forecast cards</li>
  <li>Create a more robust caching system for forecasts</li>
  <li>Possibly combine current day and forecasting back into a single tab to limit repeat code (I split them up mainly to try out having different tabs and to limit file size)</li>
  <li>Have the window resize dynamically for forecasts < 4 or 5 (the ScrollView is a good solution for now but it would be best if you could expand the window and the View expanded with you</li>
  <li>Better file structuring for easier navigation</li>
  <li>Other small code optimizations</li>
</ul>

To try this out all you would need is a free API key from WeatherAPI.com in your environment variables wherever you are running this program from and everything should work nice.

![Alt text](/images/Home.png?raw=true "Optional Title")
