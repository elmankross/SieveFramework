# SieveFramework
Framework to filter and sort collections

# Usage  
**SieveProvider** is a singletone to keep information about all registered models that can be filtered and sorted.  
**ModelProvider** contains information about concrete model. What can be filtered and sorted there.  
So, to use it needs to setup:

```csharp
// We will construct filter to this resource
public class TestModel
{
    public int Number { get; set; }
    public string String { get; set; }
    public bool Boolean { get; set; }
}

// -----------------

// create main provider
var provider = SieveProvider();

// register new model we need to process with sieve
provider.AddModel<TestModel>(builder => {
  // what property can be filtered
  builder.CanFilter(model => model.Boolean);
  // what property can be sorted
  builder.CanSort(model => model.Number);
});

// model contains information what to do with model: filter, sort, take, skip
var sieve = new Sieve<TestModel>
{
   Filters = new List<IFilter<TestModel>>
   {
      new SimpleFilter<TestModel, bool>(model => model.Boolean, SimpleFilterOperation.Equal, true)
   },
   Sorts = new List<Sort>
   {
      new Sort<TestModel, int>(model => model.Number, SortDirection.Ascending)
   }
};

// what resource needs to process with sieve
var query = new []
{
   new TestModel {Number = 2, Boolean = true},
   new TestModel {Number = 3, Boolean = false},
   new TestModel {Number = 1, Boolean = true}
}.AsQueryable();

// result of sieve processing
var result = provider.Apply(query, sieve);

```

# Roadmap
- [X] Basic implementations and abstractions
- [ ] Attribute model's binding  
- [ ] ASP.NET Core abstractions with request's query builder
- [ ] Intergation with Swagger API docs
