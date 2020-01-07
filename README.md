# SieveFramework
Framework to solve issue like this one:
> Omg, I need some filter in this API but I don't want to write abstractions because of time...
> Ok, just accept it through constant query parameters!

Project has abstractions and implementations for Sorting, Filtering and Pagination. 

Under the hood are System.Linq.Expressions.

To understand how it works please check tests.

**[TODO]**

### Filtering
Base structure _(further - node)_: `Property`\~`Alias`\~`Value`

Alias|  Description|
-----|  -----------|
`eq` |  Equal to   |
`neq`|  Not equal to|
`gt` |  Greater than|
`gte`|  Greater than or equal|
`lt` |  Less than|
`lte`|  Less than or equal|

Filter nodes may be concatinates with `OR` or `AND` logic like so:
* `node`\~and\~`node`\~and\~`node`
* `node`\~or\~`node`\~or\~`node`
* `node`\~and\~`node`\~or\~`node`

> Filter derived firstly by `or` condition so `node`\~and\~`node`\~or\~`node` will be (`node`\~and\~`node`)\~or\~`node`.  
> [TODO] Supports groups for filter.

### Sorting
Base structure _(further - node)_: `Property`\~`Alias`

Alias|  Description|
-----|  -----------|
`asc`|  Sort by ascending|
`desc`| Sort by descending|

Sort nodes may be concatinates only with `AND` logic:
* `node`\~and\~`node`

# ASP.Net Core Setup
### Register required services
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSieveProvider(config =>
    {
        // WithParser - Add custom Query parser. [NativeQueryParser] is default, no need to register them it's just an example
        config.WithParser<NativeQueryParser>()
        // ForAssemblies - Add assemblies to scan models by attributes [CanSort / CanFilter]
              .ForAssemblies(...)
        // Fluent models registration
              .ConfigureProvider(provider =>
              {
                  provider.AddModel<TestModel>(builder =>
                  {
                      builder.CanSort(p => p.TestProperty);
                      builder.CanFilter(p => p.TestProperty);
                  });
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

# Swagger Setup
### Configure SwaggerGen
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // [1] Must be registered before swagger
    services.AddSieveProvider();
    services.AddControllers();

    services.AddSwaggerGen(builder =>
    {
        // [2] Add configuration for swagger 
        services.AddSieveDescription(builder);
        builder.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Test",
            Version = "v1"
        });
    });
}
```
Sieve description extends model types in swagger scheme and adds description with allowed properties for filtering and sorting.  
**[TODO]**

# Roadmap
- [X] Basic implementations and abstractions
- [X] Attribute model's binding  
- [X] ASP.NET Core abstractions with request's query builder
- [ ] ~~Supports nested models~~
- [X] Intergation with Swagger API docs
- [ ] Cache expressions 
