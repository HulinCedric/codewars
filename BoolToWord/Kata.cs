using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Codewars.BoolToWord;

public class BoolToWordTest
{
    private readonly ITestOutputHelper output;

    public BoolToWordTest(ITestOutputHelper output)
        => this.output = output;

    [Fact]
    public void BoolToWordReturned1()
    {
        var result = Kata.boolToWord(true);
        result.Should().Be("Yes");
        output.WriteLine("Expected Yes");
    }

    [Fact]
    public void BoolToWordReturned2()
    {
        var result = Kata.boolToWord(false);
        result.Should().Be("No");
        output.WriteLine("Expected No");
    }

    [Fact]
    public void BoolToWordReturned3()
    {
        var result = Kata.boolToWord(true);
        result.Should().Be("Yes");
        output.WriteLine("Expected Yes");
    }
}

public static class Kata
{
    public static string boolToWord(bool word)
        => word ? "Yes" : "No";
}