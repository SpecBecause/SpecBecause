using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace SpecBecause.Tests
{
    public class EngineTests
    {
        private Engine Engine { get; }

        public EngineTests()
        {
            Engine = new Engine();
        }

        [Fact]
        public void When_the_type_Engine_is_loaded()
        {
            var engineType = Engine.Because(() => typeof(Engine));

            var becauseMethods = engineType.GetMethods().Where(x => x.Name == "Because").ToList();

            Engine.It("should have methods named Because", () =>
            {
                becauseMethods.Count.ShouldBe(2);
            });

            Engine.It("should have a void Because method", () =>
            {
                var voidBecauseMethod = becauseMethods.Single(x => !x.IsGenericMethod);

                voidBecauseMethod.GetParameters()
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("act");
                        x.ParameterType.Name.ShouldBe("Action");
                    });

                voidBecauseMethod.ReturnType
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("Void");
                    });
            });

            Engine.It("should have a generic Because method", () =>
            {
                const string becauseGenericTypeName = "TResult";
                var genericBecauseMethod = becauseMethods.Single(x => x.IsGenericMethod);

                genericBecauseMethod.GetGenericArguments()
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe(becauseGenericTypeName);
                        x.GetGenericParameterConstraints().ShouldBeEmpty();
                    });

                genericBecauseMethod.GetParameters()
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("act");
                        x.ParameterType.Name.ShouldBe("Func`1");
                        x.ParameterType.GetGenericArguments()
                            .ShouldHaveSingleItem()
                            .ShouldSatisfyAllConditions(y =>
                            {
                                y.Name.ShouldBe(becauseGenericTypeName);
                                y.GetGenericParameterConstraints().ShouldBeEmpty();
                            });
                    });

                genericBecauseMethod.ReturnType
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe(becauseGenericTypeName);
                        x.GetGenericParameterConstraints().ShouldBeEmpty();
                    });
            });

            Engine.It("should have a BecauseThrows method", () =>
            {
                const string becauseThrowsGenericTypeName = "TException";
                var becauseThrowsMethod = engineType.GetMethod("BecauseThrows");
                becauseThrowsMethod.ShouldNotBeNull();

                becauseThrowsMethod.GetGenericArguments()
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe(becauseThrowsGenericTypeName);
                        x.GetGenericParameterConstraints()
                            .ShouldHaveSingleItem()
                            .ShouldBe(typeof(Exception));
                    });

                becauseThrowsMethod.GetParameters()
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("act");
                        x.ParameterType.Name.ShouldBe("Action");
                    });

                becauseThrowsMethod.ReturnType
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe(becauseThrowsGenericTypeName);
                        x.GetGenericParameterConstraints()
                            .ShouldHaveSingleItem()
                            .ShouldBe(typeof(Exception));
                    });
            });

            Engine.It("should have an It method", () =>
            {
                var itMethod = engineType.GetMethod("It");
                itMethod.ShouldNotBeNull();

                var itParameters = itMethod.GetParameters();
                itParameters.Length.ShouldBe(2);
                itParameters.Single(x => x.Position == 0)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("assertionMessage");
                        x.ParameterType.ShouldBe(typeof(string));
                    });
                itParameters.Single(x => x.Position == 1)
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("assertion");
                        x.ParameterType.Name.ShouldBe("Action");
                    });

                itMethod.ReturnType
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("Void");
                    });
            });
        }

        [Fact]
        public void When_calling_void_Because()
        {
            var engineUnderTest = new Engine();
            int callCount = 0;

            Engine.Because(() =>
            {
                engineUnderTest.Because(() =>
                {
                    var _ = callCount++;
                });
            });

            Engine.It("should execute the act Action", () =>
            {
                callCount.ShouldBe(1);
            });
        }

        [Fact]
        public void When_calling_generic_Because()
        {
            var engineUnderTest = new Engine();
            var expectedResult = new object();

            var result = Engine.Because(() =>
            {
                return engineUnderTest.Because(() => expectedResult);
            });

            Engine.It("should execute the act Func", () =>
            {
                result.ShouldBeSameAs(expectedResult);
            });
        }

        [Fact]
        public void When_calling_BecauseThrows_and_an_exception_is_thrown()
        {
            var engineUnderTest = new Engine();
            var expectedException = new InvalidOperationException(Guid.NewGuid().ToString());

            var exception = Engine.Because(() => engineUnderTest.BecauseThrows<InvalidOperationException>(() => throw expectedException));

            Engine.It("should catch and return the thrown exception", () =>
            {
                exception.ShouldBeSameAs(expectedException);
            });
        }

        [Fact]
        public void When_calling_BecauseThrows_and_no_exception_is_thrown()
        {
            var engineUnderTest = new Engine();

            var exception = Engine.Because(() => engineUnderTest.BecauseThrows<InvalidOperationException>(() => { }));

            Engine.It("should return null", () =>
            {
                exception.ShouldBeNull();
            });
        }

        [Fact]
        public void When_calling_It()
        {
            var engineUnderTest = new Engine();
            var callCount = 0;

            Engine.Because(() =>
            {
                engineUnderTest.It(Guid.NewGuid().ToString(), () =>
                {
                    callCount++;
                });
            });

            var itExecuted = false;
            Engine.It("should execute the assertion action", () =>
            {
                callCount.ShouldBe(1);
                itExecuted = true;
            });

            // IMPORTANT: Do not place this if statement in an It call
            if (!itExecuted)
            {
                throw new Exception($"{nameof(Engine)}.{nameof(Engine.It)} never executed.");
            }
        }
    }
}
