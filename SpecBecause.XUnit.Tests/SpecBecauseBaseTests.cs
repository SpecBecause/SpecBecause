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
        private AutoMoqer Mocker { get; set; }

        public SpecBecauseBaseTests()
        {
            Mocker = new AutoMoqer(new Config());
        }

        [Fact]
        public void When_the_type_SpecBecauseBase_is_loaded()
        {
            var specBecauseBaseType = Because(() => typeof(SpecBecauseBase));

            It($"has an {nameof(Engine)} property", () =>
                specBecauseBaseType.GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.Name.ShouldBe(nameof(Engine));
                        x.PropertyType.Name.ShouldBe(nameof(IEngine));
                    })
            );

            It("has a constructor that accepts defaults or overrides", () =>
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
                    })
            );

            It($"implements {nameof(IEngine)}", () =>
                specBecauseBaseType.GetInterface(nameof(IEngine)).ShouldNotBeNull());
        }

        [Fact]
        public void When_constructing_with_defaults()
        {
            var classUnderTest = Because(() => new SpecBecauseBase());

            It("sets the Engine property to the default value", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldSatisfyAllConditions(x =>
                        {
                            x.ShouldNotBeNull();
                            x.ShouldBeOfType<Engine>();
                        })
            );
        }

        [Fact]
        public void When_constructing_with_arguments()
        {
            var expectedEngine = new Engine();

            var classUnderTest = Because(() => new SpecBecauseBase(expectedEngine));

            It("sets the Engine property to the passed argument", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.ShouldBeOfType<Engine>();
                        x.ShouldBeSameAs(expectedEngine);
                    })
            );
        }

        [Fact]
        public void When_calling_void_Because()
        {
            Action expectedAct = () => { };
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Because(() => classUnderTest.Because(expectedAct));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));
        }

        [Fact]
        public void When_calling_generic_Because()
        {
            int expectedResult = 1;
            Func<int> expectedAct = () => expectedResult;
            Mocker.GetMock<IEngine>().Setup(x => x.Because(expectedAct)).Returns(expectedResult);
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            var result = Because(() => classUnderTest.Because(expectedAct));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));

            It($"returns the result from the forwarded call", () =>
                result.ShouldBe(expectedResult));
        }

        [Fact]
        public void When_calling_BecauseThrows()
        {
            Action expectedAct = () => { };
            var expectedException = new Exception();
            Mocker.GetMock<IEngine>().Setup(x => x.BecauseThrows<Exception>(expectedAct)).Returns(expectedException);
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            var exception = Because(() => classUnderTest.BecauseThrows<Exception>(expectedAct));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.BecauseThrows<Exception>(expectedAct), Times.Once));

            It($"returns the result from the forwarded call", () =>
                exception.ShouldBe(expectedException));
        }

        [Fact]
        public void When_calling_It()
        {
            var expectedAssertionMessage = Guid.NewGuid().ToString();
            Action expectedAssertion = () => { };
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Because(() => classUnderTest.It(expectedAssertionMessage, expectedAssertion));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.It(expectedAssertionMessage, expectedAssertion), Times.Once));
        }

        [Fact]
        public void When_calling_Dispose()
        {
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Because(() => classUnderTest.Dispose());

            var verifyFailed = false;
            It($"forwards the call to {nameof(Engine)}", () =>
            {
                try
                {
                    Mocker.GetMock<IEngine>().Verify(x => x.Dispose(), Times.Once);
                }
                catch
                {
                    verifyFailed = true;
                    throw;
                }
            });

            // IMPORTANT: Do not place this if statement in an It call
            if (verifyFailed)
            {
                throw new Exception($"{nameof(SpecBecauseBase)}.{nameof(SpecBecauseBase.Dispose)} never called {nameof(Engine)}.{nameof(Engine.Dispose)}.");
            }
        }
    }
}
