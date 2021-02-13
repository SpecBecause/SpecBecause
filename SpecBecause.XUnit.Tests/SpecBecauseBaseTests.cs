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
            Action expectedAct = () => { };
            var classUnderTest = mocker.Create<SpecBecauseBase>();

            Because(() => classUnderTest.Because(expectedAct));

            mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once);
        }

        [Fact]
        public void When_calling_generic_Because()
        {
            var mocker = new AutoMoqer(new Config());
            int expectedResult = 1;
            Func<int> expectedAct = () => expectedResult;
            mocker.GetMock<IEngine>().Setup(x => x.Because(expectedAct)).Returns(expectedResult);
            var classUnderTest = mocker.Create<SpecBecauseBase>();
            
            var result = Because(() => classUnderTest.Because(expectedAct));

            result.ShouldBe(expectedResult);
        }

        [Fact]
        public void When_calling_BecauseThrows()
        {
            var mocker = new AutoMoqer(new Config());
            Action expectedAct = () => { };
            var expectedException = new Exception();
            mocker.GetMock<IEngine>().Setup(x => x.BecauseThrows<Exception>(expectedAct)).Returns(expectedException);
            var classUnderTest = mocker.Create<SpecBecauseBase>();

            var exception = Because(() => classUnderTest.BecauseThrows<Exception>(expectedAct));

            exception.ShouldBe(expectedException);
        }

        [Fact]
        public void When_calling_It()
        {
            var mocker = new AutoMoqer(new Config());
            var expectedAssertionMessage = Guid.NewGuid().ToString();
            Action expectedAssertion = () => { };
            var classUnderTest = mocker.Create<SpecBecauseBase>();

            Because(() => classUnderTest.It(expectedAssertionMessage, expectedAssertion));

            mocker.GetMock<IEngine>().Verify(x => x.It(expectedAssertionMessage, expectedAssertion), Times.Once);
        }
    }
}
