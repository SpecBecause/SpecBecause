using AutoMoqCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Reflection;

namespace SpecBecause.NUnit.Tests
{
    public class SpecBecauseBaseTests : SpecBecauseBase
    {
        private AutoMoqer Mocker { get; set; }
        private SpecBecauseBase ClassUnderTest { get; set; }


        [SetUp]
        public void Setup()
        {
            Mocker = new AutoMoqer(new Config());
            ClassUnderTest = Mocker.Create<SpecBecauseBase>();
            ClassUnderTest.SetUp();
        }

        [Test]
        public void When_the_type_SpecBecauseBase_is_loaded()
        {
            var specBecauseBaseType = Because(() => typeof(SpecBecauseBase));

            It($"has an {nameof(Engine)} property", () =>
                specBecauseBaseType.GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.PropertyType.Name.ShouldBe(nameof(IEngine));
                    })
            );

            It($"has an {nameof(Engine)}Provider property", () =>
                specBecauseBaseType.GetProperty("EngineProvider", BindingFlags.NonPublic | BindingFlags.Instance)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.PropertyType.Name.ShouldBe("Func`1");
                        x.PropertyType.GenericTypeArguments
                            .ShouldHaveSingleItem()
                            .Name.ShouldBe(nameof(IEngine));
                    })
            );

            It($"has a constructor that accepts an {nameof(IEngine)} providing function", () =>
                    specBecauseBaseType.GetConstructor(new[] { typeof(Func<IEngine>) })
                        .ShouldSatisfyAllConditions(x =>
                        {
                            x.ShouldNotBeNull();
                            x.GetParameters().ShouldHaveSingleItem()
                                .ShouldSatisfyAllConditions(y =>
                                {
                                    y.Name.ShouldBe("engineProvider");
                                    y.DefaultValue.ShouldBeNull();
                                });
                        })
            );

            It($"implements {nameof(IEngine)}", () =>
                specBecauseBaseType.GetInterface(nameof(IEngine)).ShouldNotBeNull());

            It("should have an NUnit tear down method", () =>
                specBecauseBaseType.GetMethod("TearDown")
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.GetParameters().ShouldBeEmpty();
                        x.ReturnType.Name.ShouldBe("Void");
                        x.GetCustomAttribute<TearDownAttribute>().ShouldNotBeNull();
                    })
             );
        }

        [Test]
        public void When_constructing_with_defaults()
        {
            var classUnderTest = Because(() => new SpecBecauseBase());

            It("does not set the Engine property to the default value", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldBeNull()
            );

            It("sets the EngineProvider property to the default EngineProvider", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("EngineProvider", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldNotBeNull()
                    .ShouldBeOfType<Func<IEngine>>()
            );
        }

        [Test]
        public void When_constructing_with_arguments()
        {
            Func<IEngine> expectedEngineProvider = () => null;

            var classUnderTest = Because(() => new SpecBecauseBase(expectedEngineProvider));

            It("does not set the Engine property to the passed argument", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldBeNull()
            );

            It("sets the EngineProvider property to the passed argument", () =>
            {
                typeof(SpecBecauseBase)
                    .GetProperty("EngineProvider", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldNotBeNull()
                    .ShouldBeSameAs(expectedEngineProvider);
            });
        }

        [Test]
        public void When_calling_default_EngineProvider()
        {
            var classUnderTest = new SpecBecauseBase();

            var defaultEngineProvider = (Func<IEngine>)typeof(SpecBecauseBase)
                .GetProperty("EngineProvider", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(classUnderTest);

            var result = Because(() => defaultEngineProvider());

            It("returns an Engine", () =>
                result
                    .ShouldNotBeNull()
                    .ShouldBeOfType<Engine>()
            );
        }

        [Test]
        public void When_calling_default_EngineProvider_multiple_times()
        {
            var classUnderTest = new SpecBecauseBase();

            var defaultEngineProvider = (Func<IEngine>)typeof(SpecBecauseBase)
                .GetProperty("EngineProvider", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(classUnderTest);

            var (r1, r2) = Because(() => {
                var result1 = defaultEngineProvider();
                var result2 = defaultEngineProvider();

                return (result1, result2);
            });

            It("returns a unique Engine instance for each invocation", () =>
                r1.ShouldNotBeSameAs(r2)
            );
        }

        [Test]
        public void When_calling_SetUp()
        {
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            bool engineProviderWasCalled = false;
            var expectedEngine = new Engine();
            Func<IEngine> engineProviderMock = () =>
            {
                engineProviderWasCalled = true;

                return expectedEngine;
            };

            typeof(SpecBecauseBase)
                .GetProperty("EngineProvider", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(classUnderTest, engineProviderMock);

            Because(() => classUnderTest.SetUp());

            It("sets Engine using EngineProvider", () => {
                engineProviderWasCalled.ShouldBeTrue();

                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldBeSameAs(expectedEngine);
            });
        }

        [Test]
        public void When_calling_void_Because()
        {
            Action expectedAct = () => { };

            Because(() => ClassUnderTest.Because(expectedAct));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));
        }

        [Test]
        public void When_calling_generic_Because()
        {
            int expectedResult = 1;
            Func<int> expectedAct = () => expectedResult;
            Mocker.GetMock<IEngine>().Setup(x => x.Because(expectedAct)).Returns(expectedResult);

            var result = Because(() => ClassUnderTest.Because(expectedAct));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));

            It($"returns the result from the forwarded call", () =>
                result.ShouldBe(expectedResult));
        }

        [Test]
        public void When_calling_BecauseThrows()
        {
            Action expectedAct = () => { };
            var expectedException = new Exception();
            Mocker.GetMock<IEngine>().Setup(x => x.BecauseThrows<Exception>(expectedAct)).Returns(expectedException);

            var exception = Because(() => ClassUnderTest.BecauseThrows<Exception>(expectedAct));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.BecauseThrows<Exception>(expectedAct), Times.Once));

            It($"returns the result from the forwarded call", () =>
                exception.ShouldBe(expectedException));
        }

        [Test]
        public void When_calling_It()
        {
            var expectedAssertionMessage = Guid.NewGuid().ToString();
            Action expectedAssertion = () => { };

            Because(() => ClassUnderTest.It(expectedAssertionMessage, expectedAssertion));

            It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.It(expectedAssertionMessage, expectedAssertion), Times.Once));
        }

        [Test]
        public void When_calling_Dispose()
        {
            Because(() => ClassUnderTest.Dispose());

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

        [Test]
        public void When_calling_TearDown()
        {
            Because(() => ClassUnderTest.TearDown());

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