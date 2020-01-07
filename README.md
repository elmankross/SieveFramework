# ![SieveFramework](img/sieve.png) SieveFramework

**[Filtering](#filtering)** |
**[Sorting](#sorting)** |
**[Swagger](#swagger)**

# Goal & Description

SieveFramework is a hight customizable framework that helps to easier integrate Filters, Sorts and Pagination to your project with enough abstraction's level.
Under the hood is `System.Linq.Expressions` helps to achieve any operations with collections and get native support for querying. 

Project|                                       Description|
-------|                                       -----------|
`SieveFramework`|                            Core project with base functionality|
`SieveFramework.AspNetCore`|                Required dependencies to integrate sieve to ASP.Net Core projects|
`SieveFramework.AspNetCore.Swashbuckle`|   Required dependencies to integrate sieve with Swagger framework|

### Design

The main provider is `SieveProvider`. It just contains a collection of `ModelProvider` that keeps the information about a concrete model:
  * which properties can be sorted
  * which properties can be filtered
  * what a type of a model
  * any meta information

The main provider works with predicates - collections of actions under a queryable resource. Each predicate can do only one type of query's action under resource:
  * FilterPredicate - Where()
  * SortPredicate - OrderBy(), OrderByDesc()
  * TakePredicate - Take()
  * SkipPredicate - Skip()
  * [any]

This design helps to delegate the same type of operations to one executor and controls a direction of operations in the pipeline.

# ASP.Net Core

### Querying
> Filter's format presented by parsers and used to bind from query request using a custom model binder.  
> [!] Data from the request's body will bound with native ASP.Net model binders.

Configuration of default native parser:

Alias|          Description|
-----|          -----------|
`~`|           `NODE_DELIMITER` - used to split values inside node|
`&`|           `OPERATION_DELIMITER` - used to split different operations (default query splitter)|
`filter=`|    `FILTER` - query parameter contains filter|
`sort=`|      `SORT` - query parameter contains sorts|
`take=`|      `TAKE` - query parameter contains selection size|
`skip=`|      `SKIP` - query parameter contains selection size|

> Planned feature is [DeepObject](https://swagger.io/docs/specification/serialization/#query) swagger format to deserialize model to query.

#### Filtering

##### Simple
Structure: `Property` `NODE_DELIMITER` `Alias` `NODE_DELIMITER` `Value`

Alias (Native)|     Alias (DeepObject)|  Description|
---------------|    ------------------|  -----------|
`eq` |              TODO|                Equal to   |
`neq`|              TODO|                Not equal to|
`gt` |              TODO|                Greater than|
`gte`|              TODO|                Greater than or equal|
`lt` |              TODO|                Less than|
`lte`|              TODO|                Less than or equal|

##### Complex
Structure: `Filter` `NODE_DELIMITER` `Alias` `NODE_DELIMITER` `Filter`

Alias (Native)|     Alias (DeepObject)|     Description|
--------------|     ------------------|     ------------|
`and`         |     TODO|                   Concatenates filters by _and_ logic|
`or`          |     TODO|                   Concatenates filters by _or_ logic|

> Filter derived firstly by `or` condition so `node`\~`and`\~`node`\~`or`\~`node` will be (`node`\~`and`\~`node`)\~`or`\~`node`.  

#### Sorting
Structure: `Property` `NODE_DELIMITER` `Alias`

Alias (Native)|  Alias (DeepObject)|    Description|
--------------|  ------------------|    -----------|
`asc`|          TODO|                   Sort by ascending|
`desc`|         TODO|                   Sort by descending|

> Sort nodes may be concatenates only with `and` logic:
> * `node`\~`and`\~`node`

### Configuration

#### Register required services
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
#### Setup Controller
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

# Swagger
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
Sieve descriptions fill up swagger scheme with strong typed sieve's model and list of allowed to sorting and filtering properties for each API action.


# Roadmap
- [X] Basic implementations and abstractions
- [X] Attribute model's binding  
- [X] ASP.NET Core abstractions with request's query builder
- [ ] ~~Supports nested models~~
- [X] Intergation with Swagger API docs
- [ ] DeepObject serialization for swagger
- [ ] Complex filters groups
- [ ] Cache expressions 

# License
<div>Icons made by <a href="https://www.flaticon.com/authors/freepik" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>