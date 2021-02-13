using Shouldly;
using System.Reflection;
using Xunit;

namespace SpecBecause.XUnit.Tests
{
    public class SpecBecauseBaseTests
    {
        [Fact]
        public void When_the_type_SpecBecauseBase_is_loaded()
        {
            var specBecauseBaseType = typeof(SpecBecauseBase);

            specBecauseBaseType.GetProperty("Engine", BindingFlags.NonPublic|BindingFlags.Instance)
                .ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Name.ShouldBe(nameof(Engine));
                    x.PropertyType.Name.ShouldBe(nameof(IEngine));
                });
        }
    }
}
