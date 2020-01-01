using SieveFramework.Exceptions;
using SieveFramework.Models;
using SieveFramework.Providers;
using SieveFrameworkTests.Models;
using Xunit;

namespace SieveFrameworkTests
{
    public class QueryProviderTests
    {
        private readonly QueryProvider _provider;

        public QueryProviderTests()
        {
            _provider = new QueryProvider();
        }


        [Fact]
        public void InvalidFilterOperation__ShouldThrowException()
        {
            var query = "invalid=";

            Assert.Throws<InvalidFilterFormatException>(() =>
            {
                _provider.Parse<SimpleTestModel>(query);
            });
        }


        [Fact]
        public void SimpleFilterString__ShouldBeParsed()
        {
            var query = "filter=Number~eq~1";
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.NotNull(sieve.Filter);
            Assert.IsType<SimpleFilterPipeline<SimpleTestModel>>(sieve.Filter);

            var filter = sieve.Filter as SimpleFilterPipeline<SimpleTestModel>;
            Assert.Equal("1", filter.Value);
            Assert.Equal("Number", filter.Property);
            Assert.Equal(SimpleFilterOperation.Equal, filter.Operation);
        }


        [Fact]
        public void SimpleFilterString__WithAnd__ShouldBeParsed()
        {
            var query = "filter=Number~eq~1~and~Boolean~eq~true";
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.NotNull(sieve.Filter);
            Assert.IsType<ComplexFilterPipeline<SimpleTestModel>>(sieve.Filter);

            var complex = sieve.Filter as ComplexFilterPipeline<SimpleTestModel>;
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
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.NotNull(sieve.Filter);
            Assert.IsType<ComplexFilterPipeline<SimpleTestModel>>(sieve.Filter);

            var complex = sieve.Filter as ComplexFilterPipeline<SimpleTestModel>;
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
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.NotEmpty(sieve.Sort);
            Assert.Collection(sieve.Sort,
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
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.Equal(999, sieve.Take);
        }


        [Fact]
        public void InvalidTakeString__ShouldThrowException()
        {
            var query = "take=example";

            Assert.Throws<InvalidFilterFormatException>(() =>
            {
                _provider.Parse<SimpleTestModel>(query);
            });
        }


        [Fact]
        public void ValidSkipString__ShouldBeParsed()
        {
            var query = "skip=999";
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.Equal(999, sieve.Skip);
        }


        [Fact]
        public void InvalidSkipString__ShouldThrowException()
        {
            var query = "skip=example";

            Assert.Throws<InvalidFilterFormatException>(() =>
            {
                _provider.Parse<SimpleTestModel>(query);
            });
        }


        [Fact]
        public void FilterWithOperations__ShouldBeParsed()
        {
            var query = "filter=&sort=&skip=&take=";
            var sieve = _provider.Parse<SimpleTestModel>(query);

            Assert.Null(sieve.Filter);
            Assert.Empty(sieve.Sort);
            Assert.Equal(0, sieve.Skip);
            Assert.Equal(100, sieve.Take);
        }
    }
}
