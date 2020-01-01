using System;
using System.Linq;
using SieveFramework.Models;
using SieveFramework.Predicates;
using SieveFramework.Providers;
using SieveFrameworkTests.Models;
using Xunit;

namespace SieveFrameworkTests
{
    public class AttributeBindingsProviderTests
    {
        [Fact]
        public void SieveProvider__FromAssembly__ShouldConstructCorrectly()
        {
            var attributeProvider = new AttributeBindingsProvider(typeof(AttributeBindingsProviderTests).Assembly);
            var sieveProvider = new SieveProvider();
            attributeProvider.FillSieveProvider(sieveProvider);

            var query = new[]
            {
                new AttributedTestModel {Number = 1, String = "a"},
                new AttributedTestModel {Number = 2, String = "b"},
                new AttributedTestModel {Number = 3, String = "c"},
            }.AsQueryable();
            var result = sieveProvider.Apply(query, new[]
            {
                new FilterPredicate<AttributedTestModel>(
                    new SimpleFilterPipeline<AttributedTestModel, string>(model => model.String,
                        SimpleFilterOperation.Equal, "a"))
            }).ToList();

            Assert.Collection(result, r => Assert.Equal("a", r.String));
        }


        [Fact]
        public void SieveProvider__FromAssembly__WithIncorrectProperty__ShouldThrowException()
        {
            var attributeProvider = new AttributeBindingsProvider(typeof(AttributeBindingsProviderTests).Assembly);
            var sieveProvider = new SieveProvider();
            attributeProvider.FillSieveProvider(sieveProvider);

            var query = new[]
            {
                new AttributedTestModel {Boolean = true},
                new AttributedTestModel {Boolean = false},
                new AttributedTestModel {Boolean = false},
            }.AsQueryable();

            Assert.Throws<ArgumentException>(() =>
            {
                sieveProvider.Apply(query, new[]
                {
                    new FilterPredicate<AttributedTestModel>(new SimpleFilterPipeline<AttributedTestModel, bool>(
                        model => model.Boolean,
                        SimpleFilterOperation.Equal, true))
                });
            });
        }
    }
}
