# SpecBecause
Goal: Make the test grammar of NSpec and MSpec portable and with improved developer ergonomics.


# How to use
The `Engine` class exposes many kinds of `Because` methods and an `It` method. `Engine` is designed as a disposable so usage is best when paired with a `using` statement.

### Inline XUnit Fact Example
```
[Fact]
public void When_testing_something()
{
    using var engine = new Engine();

    var expected = "We expect this string";

    var actual = engine.Because(() => "We actually got this string");

    engine.It("should have the correct wording", () =>
    {
        Assert.Equal(expected, actual);
    });
}
```

## A Better Way: Test Framework Base Class
By using one of the `SpecBecauseBase` classes from your framework of choice or creating your own it simplifies test setup and makes tests simpler to read and maintain.

```
public class ExampleTests: SpecBecauseBase
{
    [Fact]
    public void When_testing_something()
    {
        var expected = "We expect this string";

        var actual = Because(() => "We actually got this string");

        It("should have the correct wording", () =>
        {
            Assert.Equal(expected, actual);
        });
    }
}
```
