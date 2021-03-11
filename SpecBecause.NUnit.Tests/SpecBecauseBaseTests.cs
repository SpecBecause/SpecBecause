using AutoMoqCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Reflection;

namespace SpecBecause.NUnit.Tests
{
    public class SpecBecauseBaseTests
    {
        private AutoMoqer Mocker { get; set; }
        private Engine Engine { get; set; }
        private SpecBecauseBase ClassUnderTest { get; set; }


        [SetUp]
        public void Setup()
        {
            Mocker = new AutoMoqer(new Config());
            Engine = new Engine();
            ClassUnderTest = Mocker.Create<SpecBecauseBase>();
            ClassUnderTest.SetUp();
        }

        [TearDown]
        public void TearDown()
        {
            Engine.Dispose();
        }

        [Test]
        public void When_the_type_SpecBecauseBase_is_loaded()
        {
            var specBecauseBaseType = Engine.Because(() => typeof(SpecBecauseBase));

            Engine.It($"has an {nameof(Engine)} property", () =>
                specBecauseBaseType.GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.PropertyType.Name.ShouldBe(nameof(IEngine));
                    })
            );

            Engine.It($"has an {nameof(Engine)}Provider property", () =>
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

            Engine.It($"has a constructor that accepts an {nameof(IEngine)} providing function", () =>
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

            Engine.It($"implements {nameof(IEngine)}", () =>
                specBecauseBaseType.GetInterface(nameof(IEngine)).ShouldNotBeNull());

            Engine.It("should have an NUnit tear down method", () =>
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
            var classUnderTest = Engine.Because(() => new SpecBecauseBase());

            Engine.It("does not set the Engine property to the default value", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldBeNull()
            );

            Engine.It("sets the EngineProvider property to the default EngineProvider", () =>
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

            var classUnderTest = Engine.Because(() => new SpecBecauseBase(expectedEngineProvider));

            Engine.It("does not set the Engine property to the passed argument", () =>
                typeof(SpecBecauseBase)
                    .GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(classUnderTest)
                    .ShouldBeNull()
            );

            Engine.It("sets the EngineProvider property to the passed argument", () =>
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

            var result = Engine.Because(() => defaultEngineProvider());

            Engine.It("returns an Engine", () =>
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

            var (r1, r2) = Engine.Because(() => {
                var result1 = defaultEngineProvider();
                var result2 = defaultEngineProvider();

                return (result1, result2);
            });

            Engine.It("returns a unique Engine instance for each invocation", () =>
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

            Engine.Because(() => classUnderTest.SetUp());

            Engine.It("sets Engine using EngineProvider", () => {
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

            Engine.Because(() => ClassUnderTest.Because(expectedAct));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));
        }

        [Test]
        public void When_calling_generic_Because()
        {
            int expectedResult = 1;
            Func<int> expectedAct = () => expectedResult;
            Mocker.GetMock<IEngine>().Setup(x => x.Because(expectedAct)).Returns(expectedResult);

            var result = Engine.Because(() => ClassUnderTest.Because(expectedAct));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));

            Engine.It($"returns the result from the forwarded call", () =>
                result.ShouldBe(expectedResult));
        }

        [Test]
        public void When_calling_BecauseThrows()
        {
            Action expectedAct = () => { };
            var expectedException = new Exception();
            Mocker.GetMock<IEngine>().Setup(x => x.BecauseThrows<Exception>(expectedAct)).Returns(expectedException);

            var exception = Engine.Because(() => ClassUnderTest.BecauseThrows<Exception>(expectedAct));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.BecauseThrows<Exception>(expectedAct), Times.Once));

            Engine.It($"returns the result from the forwarded call", () =>
                exception.ShouldBe(expectedException));
        }

        [Test]
        public void When_calling_It()
        {
            var expectedAssertionMessage = Guid.NewGuid().ToString();
            Action expectedAssertion = () => { };

            Engine.Because(() => ClassUnderTest.It(expectedAssertionMessage, expectedAssertion));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.It(expectedAssertionMessage, expectedAssertion), Times.Once));
        }

        [Test]
        public void When_calling_Dispose()
        {
            Engine.Because(() => ClassUnderTest.Dispose());

            var verifyFailed = false;
            Engine.It($"forwards the call to {nameof(Engine)}", () =>
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
            Engine.Because(() => ClassUnderTest.TearDown());

            var verifyFailed = false;
            Engine.It($"forwards the call to {nameof(Engine)}", () =>
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