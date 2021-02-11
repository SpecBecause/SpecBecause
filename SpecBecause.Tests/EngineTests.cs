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

            engineType.ShouldNotBeNull();

            var becauseMethods = engineType.GetMethods().Where(x => x.Name == "Because").ToList();
            becauseMethods.Count.ShouldBe(2);

            var genericBecauseMethod = becauseMethods.Single(x => x.IsGenericMethod);

            genericBecauseMethod.GetGenericArguments()
                .ShouldHaveSingleItem()
                .ShouldSatisfyAllConditions(x =>
                {
                    x.Name.ShouldBe("TResult");
                    x.BaseType.ShouldBe(typeof(object));
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
                            y.Name.ShouldBe("TResult");
                            y.BaseType.ShouldBe(typeof(object));
                        });
                });

            genericBecauseMethod.ReturnType
                .ShouldSatisfyAllConditions(x =>
                {
                    x.Name.ShouldBe("TResult");
                    x.BaseType.ShouldBe(typeof(object));
                });

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

            callCount.ShouldBe(1);
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

            result.ShouldBeSameAs(expectedResult);
        }
    }
}
