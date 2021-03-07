using NUnit.Framework;
using Shouldly;
using System.Linq;
using System.Reflection;

namespace SpecBecause.NUnit.Tests
{
    public class SpecBecauseBaseTests
    {
        private SpecBecauseBase ClassUnderTest { get; set; }
        private Engine Engine { get; set; }

        [SetUp]
        public void Setup()
        {
            Engine = new Engine();
            ClassUnderTest = new SpecBecauseBase();
        }

        [Test]
        public void When_the_type_SpecBecauseBase_is_loaded()
        {
            var specBecauseBaseType = Engine.Because(() => typeof(SpecBecauseBase));

            specBecauseBaseType.GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)
                .ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Name.ShouldBe(nameof(Engine));
                    x.PropertyType.Name.ShouldBe(nameof(IEngine));
                });

            specBecauseBaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IEngine) }, null)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.GetParameters().ShouldHaveSingleItem()
                            .ShouldSatisfyAllConditions(y =>
                            {
                                y.Name.ShouldBe("engine");
                                y.ParameterType.Name.ShouldBe(nameof(IEngine));
                                y.HasDefaultValue.ShouldBeFalse();
                            });
                    });

            specBecauseBaseType.GetInterface(nameof(IEngine)).ShouldNotBeNull();

            specBecauseBaseType.GetMethod("SetUp")
                .ShouldSatisfyAllConditions(x =>
                {
                    x.GetParameters().ShouldBeEmpty();
                    x.ReturnType.Name.ShouldBe("Void");
                    x.GetCustomAttribute<SetUpAttribute>().ShouldNotBeNull();
                });

            specBecauseBaseType.GetMethod("TearDown")
                .ShouldSatisfyAllConditions(x =>
                {
                    x.GetParameters().ShouldBeEmpty();
                    x.ReturnType.Name.ShouldBe("Void");
                    x.GetCustomAttribute<TearDownAttribute>().ShouldNotBeNull();
                });
        }

        [Test]
        public void When_constructing_with_defaults()
        {
            var defaultClassUnderTest = new SpecBecauseBase();

            typeof(SpecBecauseBase)
                .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(defaultClassUnderTest)
                .ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.ShouldBeOfType<Engine>();
                });
        }

        [Test]
        public void When_constructing_with_arguments()
        {
            var expectedEngine = new Engine();

            var classUnderTest = new SpecBecauseBase(expectedEngine);

            typeof(SpecBecauseBase)
                .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(classUnderTest)
                .ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.ShouldBeOfType<Engine>();
                    x.ShouldBeSameAs(expectedEngine);
                });
        }
    }
}