using Shouldly;
using System.Reflection;
using Xunit;
using AutoMoqCore;
using System;
using Moq;

namespace SpecBecause.XUnit.Tests
{
    public class SpecBecauseBaseTests : SpecBecauseBase
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

            specBecauseBaseType.GetConstructor(new[] { typeof(IEngine) })
                .ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.GetParameters().ShouldHaveSingleItem()
                        .ShouldSatisfyAllConditions(y =>
                        {
                            y.Name.ShouldBe("engine");
                            y.ParameterType.Name.ShouldBe(nameof(IEngine));
                            y.DefaultValue.ShouldBeNull();
                        });
                });

            specBecauseBaseType.GetInterface(nameof(IEngine)).ShouldNotBeNull();
        }

        [Fact]
        public void When_constructing_with_defaults()
        {
            var classUnderTest = new SpecBecauseBase();

            var engineProperty = typeof(SpecBecauseBase).GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance);

            engineProperty!.GetValue(classUnderTest).ShouldSatisfyAllConditions(x =>
            {
                x.ShouldNotBeNull();
                x.ShouldBeOfType<Engine>();
            });
        }

        [Fact]
        public void When_constructing_with_arguments()
        {
            var expectedEngine = new Engine();
            var classUnderTest = new SpecBecauseBase(expectedEngine);

            var engineProperty = typeof(SpecBecauseBase).GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance);

            engineProperty!.GetValue(classUnderTest).ShouldSatisfyAllConditions(x =>
            {
                x.ShouldNotBeNull();
                x.ShouldBeOfType<Engine>();
                x.ShouldBeSameAs(expectedEngine);
            });
        }

        [Fact]
        public void When_calling_void_Because()
        {
            var mocker = new AutoMoqer(new Config());

            var classUnderTest = mocker.Create<SpecBecauseBase>();

            Action act = () => { };
            classUnderTest.Because(act);

            mocker.GetMock<IEngine>().Verify(x => x.Because(act), Times.Once);
        }
    }
}
