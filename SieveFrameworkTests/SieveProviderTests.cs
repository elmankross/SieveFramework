using System;
using System.Collections.Generic;
using System.Linq;
using SieveFramework.Models;
using SieveFramework.Predicates;
using SieveFramework.Providers;
using SieveFrameworkTests.Models;
using Xunit;

namespace SieveFrameworkTests
{
    public class SieveProviderTests
    {
        [Fact]
        public void SimpleFilter__WithAllowedProperty__ShouldBeOk()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(builder =>
                {
                    builder.CanFilter(model => model.Boolean);
                });
            var query = new[]
            {
                new SimpleTestModel {Boolean = false},
                new SimpleTestModel {Boolean = true}
            }.AsQueryable();
            var filterPredicate = new FilterPredicate<SimpleTestModel>(
                new SimpleFilterPipeline<SimpleTestModel, bool>(model => model.Boolean, SimpleFilterOperation.Equal, true)
            );

            var result = provider.Apply(query, new[] { filterPredicate }).ToList();

            Assert.All(result, r => Assert.True(r.Boolean));
        }


        [Fact]
        public void SimpleFilter__WithDisallowedProperty__ShouldThrowException()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(_ => { });
            var query = new[]
            {
                new SimpleTestModel {Boolean = false},
                new SimpleTestModel {Boolean = true}
            }.AsQueryable();
            var filterPredicate = new FilterPredicate<SimpleTestModel>(
                new SimpleFilterPipeline<SimpleTestModel, bool>(model => model.Boolean, SimpleFilterOperation.Equal, true)
            );

            Assert.Throws<ArgumentException>(() =>
            {
                provider.Apply(query, new[] { filterPredicate });
            });
        }


        [Fact]
        public void EmptyComplexFilter__ShouldBeOk()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(builder =>
                {
                    builder.CanFilter(model => model.Boolean);
                });
            var query = new[]
            {
                new SimpleTestModel {Boolean = false},
                new SimpleTestModel {Boolean = true}
            }.AsQueryable();
            var filterPredicate = new FilterPredicate<SimpleTestModel>(
                new ComplexFilterPipeline<SimpleTestModel>(ComplexFilterOperation.And, new IFilterPipeline<SimpleTestModel>[0])
                );

            var result = provider.Apply(query, new[] { filterPredicate }).ToList();

            Assert.Equal(2, result.Count);
        }


        [Fact]
        public void ComplexFilter__WithAllowedProperty__ShouldBeOk()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(builder =>
                {
                    builder.CanFilter(model => model.Boolean);
                    builder.CanFilter(model => model.Number);
                });
            var query = new[]
            {
                new SimpleTestModel {Boolean = false, Number = 1},
                new SimpleTestModel {Boolean = true, Number = 2},
                new SimpleTestModel {Boolean = true, Number = 3}
            }.AsQueryable();
            var filterPredicate = new FilterPredicate<SimpleTestModel>(new ComplexFilterPipeline<SimpleTestModel>(
                ComplexFilterOperation.Or, new IFilterPipeline<SimpleTestModel>[]
                {
                    new ComplexFilterPipeline<SimpleTestModel>(ComplexFilterOperation.And,
                        new IFilterPipeline<SimpleTestModel>[]
                        {
                            new SimpleFilterPipeline<SimpleTestModel, bool>(model => model.Boolean,
                                SimpleFilterOperation.Equal, true),
                            new SimpleFilterPipeline<SimpleTestModel, int>(model => model.Number,
                                SimpleFilterOperation.Equal, 3),
                        }),
                    new SimpleFilterPipeline<SimpleTestModel, bool>(model => model.Boolean, SimpleFilterOperation.Equal,
                        false)
                }));

            var result = provider.Apply(query, new[] { filterPredicate }).ToList();

            Assert.Equal(2, result.Count);
            Assert.Single(result, model => model.Boolean == true && model.Number == 3);
            Assert.Single(result, model => model.Boolean == false && model.Number == 1);
        }


        [Fact]
        public void Sort__WithAllowedProperty__ShouldBeOk()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(builder =>
                {
                    builder.CanSort(model => model.Number);
                });
            var query = new[]
            {
                new SimpleTestModel {Number = 2},
                new SimpleTestModel {Number = 3},
                new SimpleTestModel {Number = 1}
            }.AsQueryable();
            var sortPredicate = new SortPredicate<SimpleTestModel>(
                new SortPipeline<SimpleTestModel, int>(model => model.Number, SortDirection.Descending)
            );

            var result = provider.Apply(query, new[] { sortPredicate }).ToList();

            Assert.Collection(result,
                r => Assert.Equal(3, r.Number),
                r => Assert.Equal(2, r.Number),
                r => Assert.Equal(1, r.Number));
        }


        [Fact]
        public void Sort__WithDisallowedProperty__ShouldThrowException()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(_ => { });
            var query = new[]
            {
                new SimpleTestModel {Number = 2},
                new SimpleTestModel {Number = 3},
                new SimpleTestModel {Number = 1}
            }.AsQueryable();
            var sortPredicate = new SortPredicate<SimpleTestModel>(
                new SortPipeline<SimpleTestModel, int>(model => model.Number, SortDirection.Descending)
            );

            Assert.Throws<ArgumentException>(() =>
            {
                provider.Apply(query, new[] { sortPredicate });
            });
        }


        [Fact]
        public void Sieve__WithAllowedProperties__ShouldBeOk()
        {
            var provider = new SieveProvider()
               .AddModel<SimpleTestModel>(builder =>
                {
                    builder.CanFilter(model => model.Boolean);
                    builder.CanSort(model => model.Number);
                });
            var query = new[]
            {
                new SimpleTestModel {Number = 2, Boolean = true},
                new SimpleTestModel {Number = 3, Boolean = false},
                new SimpleTestModel {Number = 1, Boolean = true}
            }.AsQueryable();
            var sieve = new Sieve<SimpleTestModel>
            {
                Filter = new SimpleFilterPipeline<SimpleTestModel, bool>(model => model.Boolean, SimpleFilterOperation.Equal, true)
                ,
                Sort = new List<ISortPipeline<SimpleTestModel>>
                {
                    new SortPipeline<SimpleTestModel, int>(model => model.Number, SortDirection.Ascending)
                }
            };

            var result = provider.Apply(query, sieve).ToList();

            Assert.Collection(result,
                r => Assert.Equal(1, r.Number),
                r => Assert.Equal(2, r.Number));
        }


        [Fact]
        public void Sieve__WithUnregisteredModel__ShouldThrowException()
        {
            var provider = new SieveProvider();
            var query = new[]
            {
                new SimpleTestModel {Number = 2, Boolean = true},
                new SimpleTestModel {Number = 3, Boolean = false},
                new SimpleTestModel {Number = 1, Boolean = true}
            }.AsQueryable();
            var sieve = new Sieve<SimpleTestModel>
            {
                Filter = new SimpleFilterPipeline<SimpleTestModel, bool>(model => model.Boolean, SimpleFilterOperation.Equal, true)
            };

            Assert.Throws<ArgumentException>(() => provider.Apply(query, sieve));
        }
    }
}
