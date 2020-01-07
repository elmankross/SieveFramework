using SieveFramework.AspNetCore.Parsers;
using SieveFramework.Models;
using SieveFrameworkTests.Models;
using Xunit;

namespace SieveFrameworkTests
{
    public class QueryProviderTests
    {
        private readonly IParser _provider;

        public QueryProviderTests()
        {
            _provider = new NativeQueryParser();
        }


        [Fact]
        public void UnknownQueryParameter__ShouldBePassed()
        {
            var query = "unknown=";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.True(result.Successful);
        }


        [Fact]
        public void SimpleFilterString__ShouldBeParsed()
        {
            var query = "filter=Number~eq~1";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.True(result.Successful);
            Assert.NotNull(result.Result.Filter);
            Assert.IsType<SimpleFilterPipeline<SimpleTestModel>>(result.Result.Filter);

            var filter = result.Result.Filter as SimpleFilterPipeline<SimpleTestModel>;
            Assert.Equal("1", filter.Value);
            Assert.Equal("Number", filter.Property);
            Assert.Equal(SimpleFilterOperation.Equal, filter.Operation);
        }


        [Fact]
        public void SimpleFilterString__WithAnd__ShouldBeParsed()
        {
            var query = "filter=Number~eq~1~and~Boolean~eq~true";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.True(result.Successful);
            Assert.NotNull(result.Result.Filter);
            Assert.IsType<ComplexFilterPipeline<SimpleTestModel>>(result.Result.Filter);

            var complex = result.Result.Filter as ComplexFilterPipeline<SimpleTestModel>;
            Assert.Equal(ComplexFilterOperation.And, complex.Operation);
            Assert.NotEmpty(complex.Filters);

            var first = complex.Filters[0] as SimpleFilterPipeline<SimpleTestModel>;
            Assert.Equal("1", first.Value);
            Assert.Equal("Number", first.Property);
            Assert.Equal(SimpleFilterOperation.Equal, first.Operation);

            var second = complex.Filters[1] as SimpleFilterPipeline<SimpleTestModel>;
            Assert.Equal("true", second.Value);
            Assert.Equal("Boolean", second.Property);
            Assert.Equal(SimpleFilterOperation.Equal, second.Operation);
        }


        [Fact]
        public void SimpleFilterString__WithOr__ShouldBeParsed()
        {
            var query = "filter=Number~eq~1~or~Boolean~eq~true";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.True(result.Successful);
            Assert.NotNull(result.Result.Filter);
            Assert.IsType<ComplexFilterPipeline<SimpleTestModel>>(result.Result.Filter);

            var complex = result.Result.Filter as ComplexFilterPipeline<SimpleTestModel>;
            Assert.Equal(ComplexFilterOperation.Or, complex.Operation);
            Assert.NotEmpty(complex.Filters);

            var first = complex.Filters[0] as SimpleFilterPipeline<SimpleTestModel>;
            Assert.Equal("1", first.Value);
            Assert.Equal("Number", first.Property);
            Assert.Equal(SimpleFilterOperation.Equal, first.Operation);

            var second = complex.Filters[1] as SimpleFilterPipeline<SimpleTestModel>;
            Assert.Equal("true", second.Value);
            Assert.Equal("Boolean", second.Property);
            Assert.Equal(SimpleFilterOperation.Equal, second.Operation);
        }


        [Fact]
        public void SortString__ShouldBeParsed()
        {
            var query = "sort=Number~asc~and~Boolean~desc";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.True(result.Successful);
            Assert.NotEmpty(result.Result.Sort);
            Assert.Collection(result.Result.Sort,
                s =>
                {
                    Assert.Equal("Number", s.Property);
                    Assert.Equal(SortDirection.Ascending, s.Direction);
                },
                s =>
                {
                    Assert.Equal("Boolean", s.Property);
                    Assert.Equal(SortDirection.Descending, s.Direction);
                });
        }


        [Fact]
        public void ValidTakeString__ShouldBeParsed()
        {
            var query = "take=999";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.Equal(999, result.Result.Take);
        }


        [Fact]
        public void InvalidTakeString__ShouldThrowException()
        {
            var query = "take=example";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.False(result.Successful);
        }


        [Fact]
        public void ValidSkipString__ShouldBeParsed()
        {
            var query = "skip=999";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.Equal(999, result.Result.Skip);
        }


        [Fact]
        public void InvalidSkipString__ShouldThrowException()
        {
            var query = "skip=example";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.False(result.Successful);
        }


        [Fact]
        public void FilterWithOperations__ShouldBeParsed()
        {
            var query = "filter=&sort=&skip=&take=";
            var result = _provider.TryParse<SimpleTestModel>(query);

            Assert.True(result.Successful);
            Assert.Null(result.Result.Filter);
            Assert.Empty(result.Result.Sort);
            Assert.Equal(0, result.Result.Skip);
            Assert.Equal(100, result.Result.Take);
        }
    }
}
