using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.StripComments;

public class StripCommentsTest
{
    [Theory]
    [InlineData(
        "apples, pears # and bananas\ngrapes\nbananas !apples",
        new[] { "#", "!" },
        "apples, pears\ngrapes\nbananas")]
    [InlineData(
        "a #b\nc\nd $e f g",
        new[] { "#", "$" },
        "a\nc\nd")]
    [InlineData(
        "a \n b \nc ",
        new[] { "#", "$" },
        "a\n b\nc")]
    public void SampleTests(string text, string[] commentSymbols, string expected)
        => StripCommentsSolution.StripComments(text, commentSymbols)
            .Should()
            .Be(expected);
}

public static class StripCommentsSolution
{
    private const char NewLineCharacter = '\n';

    public static string StripComments(string text, string[] commentSymbols)
    {
        var lines = text.Split(NewLineCharacter);

        var strippedLines = lines.StripComments(commentSymbols);

        return string.Join(NewLineCharacter, strippedLines);
    }

    private static IEnumerable<string> StripComments(this IEnumerable<string> lines, string[] commentSymbols)
        => lines.Select(line => line.StripComment(commentSymbols));

    private static string StripComment(this string line, string[] commentSymbols)
        => line.Split(commentSymbols, StringSplitOptions.None).First().TrimEnd();
}