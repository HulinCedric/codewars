using System.Linq;
using FluentAssertions;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace Codewars.DetectPangram;

public class Tests
{
    [Fact]
    public void SampleTests()
        => Kata.IsPangram("The quick brown fox jumps over the lazy dog.").Should().BeTrue();

    [Theory]
    [InlineData("This isn't a pangram!", false)]
    [InlineData("abcdefghijklmopqrstuvwxyz ", false)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaa", false)]
    [InlineData("Detect Pangram", false)]
    [InlineData("A pangram is a sentence that contains every single letter of the alphabet at least once.", false)]
    [InlineData("Cwm fjord bank glyphs vext quiz", true)]
    [InlineData("Pack my box with five dozen liquor jugs.", true)]
    [InlineData("How quickly daft jumping zebras vex.", true)]
    [InlineData("ABCD45EFGH,IJK,LMNOPQR56STUVW3XYZ", true)]
    [InlineData("AbCdEfGhIjKlM zYxWvUtSrQpOn", true)]
    [InlineData(
        "Raw Danger! (Zettai Zetsumei Toshi 2) for the PlayStation 2 is a bit queer, but an alright game I guess, uh... CJ kicks and vexes Tenpenny precariously? This should be a pangram now, probably.",
        true)]
    public void FixedTests(string sentence, bool expected)
        => Kata.IsPangram(sentence).Should().Be(expected);
}

public static class Kata
{
    private static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static bool IsPangram(string sentence)
    {
        var uppercaseSentence = sentence.ToUpperInvariant();

        return Alphabet.All(letter => uppercaseSentence.Contains(letter));
    }
}