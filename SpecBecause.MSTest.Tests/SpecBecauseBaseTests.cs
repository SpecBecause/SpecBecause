using AutoMoqCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using System.Reflection;

namespace SpecBecause.MSTest.Tests
{
    [TestClass]
    public class SpecBecauseBaseTests
    {
        private AutoMoqer Mocker { get; set; }
        private Engine Engine { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Mocker = new AutoMoqer(new Config());
            Engine = new Engine();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Engine.Dispose();
        }

        [TestMethod]
        public void When_the_type_SpecBecauseBase_is_loaded()
        {
            var specBecauseBaseType = Engine.Because(() => typeof(SpecBecauseBase));

            Engine.It($"has an {nameof(Engine)} property", () =>
                specBecauseBaseType.GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.Name.ShouldBe(nameof(Engine));
                        x.PropertyType.Name.ShouldBe(nameof(IEngine));
                    })
            );

            Engine.It("has a constructor that accepts defaults or overrides", () =>
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

            Engine.It($"implements {nameof(IEngine)}", () =>
                specBecauseBaseType.GetInterface(nameof(IEngine)).ShouldNotBeNull());

            Engine.It("should have an MSTest test cleanup method", () =>
                specBecauseBaseType.GetMethod("TestCleanup")
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.GetParameters().ShouldBeEmpty();
                        x.ReturnType.Name.ShouldBe("Void");
                        x.GetCustomAttribute<TestCleanupAttribute>().ShouldNotBeNull();
                    })
             );
        }

        [TestMethod]
        public void When_constructing_with_defaults()
        {
            var classUnderTest = Engine.Because(() => new SpecBecauseBase());

            Engine.It("sets the Engine property to the default value", () =>
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

        [TestMethod]
        public void When_constructing_with_arguments()
        {
            var expectedEngine = new Engine();

            var classUnderTest = Engine.Because(() => new SpecBecauseBase(expectedEngine));

            Engine.It("sets the Engine property to the passed argument", () =>
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

        [TestMethod]
        public void When_calling_void_Because()
        {
            Action expectedAct = () => { };
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Engine.Because(() => classUnderTest.Because(expectedAct));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));
        }

        [TestMethod]
        public void When_calling_generic_Because()
        {
            int expectedResult = 1;
            Func<int> expectedAct = () => expectedResult;
            Mocker.GetMock<IEngine>().Setup(x => x.Because(expectedAct)).Returns(expectedResult);
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            var result = Engine.Because(() => classUnderTest.Because(expectedAct));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.Because(expectedAct), Times.Once));

            Engine.It($"returns the result from the forwarded call", () =>
                result.ShouldBe(expectedResult));
        }

        [TestMethod]
        public void When_calling_BecauseThrows()
        {
            Action expectedAct = () => { };
            var expectedException = new Exception();
            Mocker.GetMock<IEngine>().Setup(x => x.BecauseThrows<Exception>(expectedAct)).Returns(expectedException);
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            var exception = Engine.Because(() => classUnderTest.BecauseThrows<Exception>(expectedAct));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.BecauseThrows<Exception>(expectedAct), Times.Once));

            Engine.It($"returns the result from the forwarded call", () =>
                exception.ShouldBe(expectedException));
        }

        [TestMethod]
        public void When_calling_It()
        {
            var expectedAssertionMessage = Guid.NewGuid().ToString();
            Action expectedAssertion = () => { };
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Engine.Because(() => classUnderTest.It(expectedAssertionMessage, expectedAssertion));

            Engine.It($"forwards the call to {nameof(Engine)}", () =>
                Mocker.GetMock<IEngine>().Verify(x => x.It(expectedAssertionMessage, expectedAssertion), Times.Once));
        }

        [TestMethod]
        public void When_calling_Dispose()
        {
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Engine.Because(() => classUnderTest.Dispose());

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

        [TestMethod]
        public void When_calling_TestCleanup()
        {
            var classUnderTest = Mocker.Create<SpecBecauseBase>();

            Engine.Because(() => classUnderTest.TestCleanup());

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
