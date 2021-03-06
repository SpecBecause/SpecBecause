using Shouldly;
using SpecBecause.XUnit;
using System;
using System.Linq;
using Xunit;

namespace SpecBecause.Tests
{
    public class EngineTests : SpecBecauseBase
    {
        [Fact]
        public void When_the_type_Engine_is_loaded()
        {
            var engineType = Because(() => typeof(Engine));

            var becauseMethods = engineType.GetMethods().Where(x => x.Name == "Because").ToList();

            It($"should implement {nameof(IDisposable)}", () =>
            {
                engineType.GetInterface(nameof(IDisposable)).ShouldNotBeNull();
            });

            It("should have methods named Because", () =>
            {
                becauseMethods.Count.ShouldBe(2);
            });

            It("should have a void Because method", () =>
            {
                var voidBecauseMethod = becauseMethods.Single(x => !x.IsGenericMethod);

                voidBecauseMethod.GetParameters()
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Name.ShouldBe("act");
                        x.ParameterType.Name.ShouldBe("Action");
                    });

                voidBecauseMethod.ReturnType.Name.ShouldBe("Void");
            });

            It("should have a generic Because method", () =>
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

            It("should have a BecauseThrows method", () =>
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

            It("should have an It method", () =>
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

                itMethod.ReturnType.Name.ShouldBe("Void");
            });
        }

        [Fact]
        public void When_calling_void_Because()
        {
            var engineUnderTest = new Engine();
            int callCount = 0;

            Because(() =>
            {
                engineUnderTest.Because(() =>
                {
                    var _ = callCount++;
                });
            });

            It("should execute the act Action", () =>
            {
                callCount.ShouldBe(1);
            });
        }

        [Fact]
        public void When_calling_generic_Because()
        {
            var engineUnderTest = new Engine();
            var expectedResult = new object();

            var result = Because(() =>
            {
                return engineUnderTest.Because(() => expectedResult);
            });

            It("should execute the act Func", () =>
            {
                result.ShouldBeSameAs(expectedResult);
            });
        }

        [Fact]
        public void When_calling_BecauseThrows_and_an_exception_is_thrown()
        {
            var engineUnderTest = new Engine();
            var expectedException = new InvalidOperationException(Guid.NewGuid().ToString());

            var exception = Because(() => engineUnderTest.BecauseThrows<InvalidOperationException>(() => throw expectedException));

            It("should catch and return the thrown exception", () =>
            {
                exception.ShouldBeSameAs(expectedException);
            });
        }

        [Fact]
        public void When_calling_BecauseThrows_and_an_unexpected_exception_is_thrown()
        {
            var engineUnderTest = new Engine();
            var unexpectedException = new Exception(Guid.NewGuid().ToString());

            var exception = BecauseThrows<Exception>(() => engineUnderTest.BecauseThrows<InvalidOperationException>(() => throw unexpectedException));

            It("should catch and return the thrown exception", () =>
            {
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeSameAs(unexpectedException);
                    x.ShouldBeOfType<EngineException>();
                    x!.Message.ShouldBe("Act threw an unexpected exception.");
                    x.InnerException.ShouldBeSameAs(unexpectedException);
                });
            });
        }

        [Fact]
        public void When_calling_BecauseThrows_and_no_exception_is_thrown()
        {
            var engineUnderTest = new Engine();

            var exception = Because(() => engineUnderTest.BecauseThrows<InvalidOperationException>(() => { }));

            It("should return null", () =>
            {
                exception.ShouldBeNull();
            });
        }

        [Fact]
        public void When_calling_It()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });
            var callCount = 0;

            Because(() =>
            {
                engineUnderTest.It(Guid.NewGuid().ToString(), () =>
                {
                    callCount++;
                });
            });

            var itExecuted = false;
            It("should execute the assertion action", () =>
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

        [Fact]
        public void When_calling_It_and_and_exception_is_thrown()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });
            var expectedException = new Exception(Guid.NewGuid().ToString());

            var exception = BecauseThrows<Exception>(() =>
                engineUnderTest.It(Guid.NewGuid().ToString(), () => throw expectedException));

            It("should not immediately throw the exception", () =>
            {
                exception.ShouldBeNull();
            });
        }

        [Fact]
        public void When_disposing_and_an_exception_has_been_captured()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });

            var expectedException = new Exception(Guid.NewGuid().ToString());
            var assertionMessage = Guid.NewGuid().ToString();
            engineUnderTest.It(assertionMessage, () => throw expectedException);

            var exception = BecauseThrows<Exception>(() => engineUnderTest.Dispose());

            var failedToThrow = false;
            It("should throw the exception", () =>
            {
                try
                {
                    exception.ShouldSatisfyAllConditions(x =>
                    {
                        x.ShouldNotBeNull();
                        x.ShouldBeOfType<EngineException>();
                        x.Message.ShouldBe($"It {assertionMessage}");
                        x.InnerException.ShouldBeSameAs(expectedException);
                    });
                }
                catch (Exception)
                {
                    failedToThrow = true;
                    throw;
                }
            });

            // IMPORTANT: Do not place this if statement in an It call
            if (failedToThrow)
            {
                throw new Exception("Failed to throw exception on Dispose.");
            }
        }

        [Fact]
        public void When_disposing_and_multiple_exceptions_have_been_captured()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });

            var expectedException1 = new Exception(Guid.NewGuid().ToString());
            var expectedException2 = new Exception(Guid.NewGuid().ToString());
            string assertionMessage1 = Guid.NewGuid().ToString();
            string assertionMessage2 = Guid.NewGuid().ToString();
            engineUnderTest.It(assertionMessage1, () => throw expectedException1);
            engineUnderTest.It(assertionMessage2, () => throw expectedException2);

            var exception = BecauseThrows<AggregateException>(() => engineUnderTest.Dispose());

            It("should aggregate and throw all captured exceptions", () =>
            {
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.InnerExceptions.Count.ShouldBe(2);
                    x.InnerExceptions.First().ShouldSatisfyAllConditions(y =>
                    {
                        y.ShouldBeOfType<EngineException>();
                        y.Message.ShouldBe($"It {assertionMessage1}");
                        y.InnerException.ShouldBe(expectedException1);
                    });
                    x.InnerExceptions.Last().ShouldSatisfyAllConditions(y =>
                    {
                        y.ShouldBeOfType<EngineException>();
                        y.Message.ShouldBe($"It {assertionMessage2}");
                        y.InnerException.ShouldBe(expectedException2);
                    });
                });
            });
        }

        [Fact]
        public void When_disposing_and_Because_was_not_called()
        {
            var engineUnderTest = new Engine();

            var exception = BecauseThrows<Exception>(() => engineUnderTest.Dispose());

            It($"should notify the developer to call {nameof(Engine.Because)}", () =>
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Message.ShouldBe($"Friendly reminder when using {nameof(Engine)} you must call {nameof(Engine.Because)} and {nameof(Engine.It)} methods before disposing.");
                })
            );
        }

        [Fact]
        public void When_disposing_and_It_was_not_called()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });

            var exception = BecauseThrows<Exception>(() => engineUnderTest.Dispose());

            It($"should notify the developer to {nameof(Engine.It)}", () =>
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Message.ShouldBe($"Friendly reminder when using {nameof(Engine)} you must call {nameof(Engine.Because)} and {nameof(Engine.It)} methods before disposing.");
                })
            );
        }

        [Fact]
        public void When_calling_It_before_Because()
        {
            var engineUnderTest = new Engine();

            var exception = BecauseThrows<Exception>(() => engineUnderTest.It(Guid.NewGuid().ToString(), () => { }));

            It($"should notify the developer that {nameof(Engine.Because)} must be called before {nameof(Engine.It)}", () =>
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Message.ShouldBe($"{nameof(Engine.Because)} must be called before {nameof(Engine.It)}.");
                })
            );
        }

        [Fact]
        public void When_calling_void_Because_after_It()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });
            engineUnderTest.It(Guid.NewGuid().ToString(), () => { });

            var exception = BecauseThrows<Exception>(() => engineUnderTest.Because(() => { }));

            It($"should notify the developer that {nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}", () =>
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Message.ShouldBe($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
                })
            );
        }

        [Fact]
        public void When_calling_generic_Because_after_It()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });
            engineUnderTest.It(Guid.NewGuid().ToString(), () => { });

            var exception = BecauseThrows<Exception>(() => engineUnderTest.Because(() => 0));

            It($"should notify the developer that {nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}", () =>
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Message.ShouldBe($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
                })
            );
        }

        [Fact]
        public void When_calling_BecauseThrows_after_It()
        {
            var engineUnderTest = new Engine();
            engineUnderTest.Because(() => { });
            engineUnderTest.It(Guid.NewGuid().ToString(), () => { });

            var exception = BecauseThrows<Exception>(() => engineUnderTest.BecauseThrows<Exception>(() => throw new Exception()));

            It($"should notify the developer that {nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}", () =>
                exception.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.Message.ShouldBe($"{nameof(Engine.Because)} cannot be called after {nameof(Engine.It)}.");
                })
            );
        }
    }
}
