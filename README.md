# SieveFramework
Framework to solve issue like this one:
> Omg, I need some filter in this API but I don't want to write abstractions because of time...
> Ok, just accept it through constant query parameters!

Framework solves cases with Filtering and Sorting (Take and Skip included). 

Under the hood are System.Linq.Expressions.

To understand how it works please check tests.

(!) _Extended documentation after intergation with Swagger: [TODO]_

# ASP.Net Core Setup
### Register required services
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // [1] Or register with autoscan by attributes
    services.AddSieveProvider();

    // [2] Or register required models by yourself
    services.AddSieveProvider(provider =>
    {
        provider.AddModel<WeatherForecast>(builder =>
        {
            builder.CanFilter(x => x.Summary);
            builder.CanSort(x => x.Date);
        });
    });
    services.AddControllers();
}
```
### Setup Controller
```csharp
public class WeatherForecastController : ControllerBase
{
    // [1] Accept processor through DI 
    private readonly ISieveProvider _sieve;

    public WeatherForecastController(ISieveProvider sieve)
    {
        _sieve = sieve;
    }

    // [2] Wrap processed model's resource with [Sieve] - It will be maped automaticly
    [HttpGet]
    public ActionResult GetCustom(Sieve<WeatherForecast> model)
    {
        var rng = new Random();
        var query = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = "Summary" + index
        }).AsQueryable();

        // [3] Apply filter to resource
        var result = _sieve.Apply(query, model).ToArray();

        return Ok(new
        {
            origin = query.ToArray(),
            result = result
        });
    }
}
```

# Roadmap
- [X] Basic implementations and abstractions
- [X] Attribute model's binding  
- [X] ASP.NET Core abstractions with request's query builder
- [ ] ~~Supports nested models~~
- [ ] Intergation with Swagger API docs
- [ ] Cache expressions 
