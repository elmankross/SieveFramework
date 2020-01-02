using System;
using SieveFramework.Attributes;

namespace WebTest
{
    public class WeatherForecast
    {
        [CanSort]
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [CanFilter]
        public string Summary { get; set; }
    }
}
