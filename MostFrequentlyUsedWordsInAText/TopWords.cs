using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Codewars.MostFrequentlyUsedWordsInAText;

public class SolutionTest
{
    [Theory]
    [InlineData("a a a  b  c c  d d d d  e e e e e", new[] { "e", "d", "a" })]
    [InlineData("e e e e DDD ddd DdD: ddd ddd aa aA Aa, bb cc cC e e e", new[] { "e", "ddd", "aa" })]
    [InlineData("  //wont won't won't ", new[] { "won't", "wont" })]
    [InlineData("  , e   .. ", new[] { "e" })]
    [InlineData("  ...  ", new string[] { })]
    [InlineData("  '  ", new string[] { })]
    [InlineData("  '''  ", new string[] { })]
    [InlineData(
        "In a village of La Mancha, the name of which I have no desire to call to\n" +
        "mind, there lived not long since one of those gentlemen that keep a lance\n" +
        "in the lance-rack, an old buckler, a lean hack, and a greyhound for\n" +
        "coursing. An olla of rather more beef than mutton, a salad on most\n" +
        "nights, scraps on Saturdays, lentils on Fridays, and a pigeon or so extra\n" +
        "on Sundays, made away with three-quarters of his income.",
        new[] { "a", "of", "on" })]
    public void SampleTests(string sentence, string[] result)
        => TopWords.Top3(sentence).Should().BeEquivalentTo(result);

    [Theory]
    [InlineData("a", "a")]
    [InlineData("B", "b")]
    [InlineData("'AbC'", "'abc'")]
    [InlineData("//wont", "wont")]
    [InlineData("//Won't#", "won't")]
    [InlineData("Don't.Want", "don't want")]
    [InlineData("Don't.Want#", "don't want")]
    public void BeEqual(string text, string sanitizeText)
        => Words.CreateFrom(text).ToString().Should().Be(sanitizeText);

    [Theory]
    [InlineData(@"#")]
    [InlineData(@"\")]
    [InlineData(@"/")]
    [InlineData(@".")]
    [InlineData(@"..")]
    [InlineData(@".#/")]
    public void BeEmptyWordWhenContainingNotPartWordCharacter(string text)
        => Words.CreateFrom(text).ToString().Should().Be(Word.Empty.ToString());

    [Theory]
    [InlineData(@".''.", "")]
    [InlineData(@".'a'.", "'a'")]
    [InlineData("  '''  ", "")]
    [InlineData("''' don't.want#", "don't want")]
    public void BeEmptyWordWhenContainingNoLetter(string text, string value)
        => Words.CreateFrom(text).ToString().Should().Be(value);
}

public static class TopWords
{
    private const int Top3Number = 3;

    public static List<string> Top3(string text)
    {
        var words = Words.CreateFrom(text);
        var orderedWords = words.OrderByDescendingOccurrence();
        var top3MostFrequentlyUsedWords = orderedWords.Take(Top3Number);

        return top3MostFrequentlyUsedWords.Select(word => word.ToString()).ToList();
    }
}

public class Words : IEnumerable<Word>
{
    private readonly IImmutableList<Word> words;

    private Words(IEnumerable<Word> words)
        => this.words = words.ToImmutableList();

    public IEnumerator<Word> GetEnumerator()
        => words.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static Words CreateFrom(string text)
        => new(Word.ExtractWords(text));

    public Words OrderByDescendingOccurrence()
        => new(
            words.GroupBy(word => word)
                .OrderByDescending(groupedWord => groupedWord.Count())
                .Select(groupedWord => groupedWord.Key));

    public override string ToString()
        => string.Join(' ', words);
}

public record Word
{
    public static readonly Word Empty = new(string.Empty);
    private readonly string value;

    private Word(string text)
        => value = text;

    public static IEnumerable<Word> ExtractWords(string sentence)
        => Sanitize(sentence)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => new Word(w));

    private static string Sanitize(string text)
    {
        text = ReplaceNotPartOfWordCharactersByWhitespace(text);
        text = ReplaceTextWithoutLetterByWhitespaces(text);
        return text.ToLowerInvariant();
    }

    private static string ReplaceNotPartOfWordCharactersByWhitespace(string text)
    {
        var stringBuilder = new StringBuilder();
        foreach (var character in text)
            stringBuilder.Append(IsWordCharacter(character) ? character : ' ');

        return stringBuilder.ToString();
    }

    private static bool IsWordCharacter(char character)
        => char.IsLetter(character) || character == '\'';

    private static string ReplaceTextWithoutLetterByWhitespaces(string text)
        => string.Join(' ', text.Split(' ').Select(GetEquivalentWhitespacesWhenTextNotContainsLetter));

    private static string GetEquivalentWhitespacesWhenTextNotContainsLetter(string text)
    {
        var containsLetter = text.Any(char.IsLetter);
        return containsLetter ? text : ReplaceAllCharacterByWhitespace(text);
    }

    private static string ReplaceAllCharacterByWhitespace(string text)
        => new(Enumerable.Repeat(' ', text.Length).ToArray());

    public override string ToString()
        => value;
}