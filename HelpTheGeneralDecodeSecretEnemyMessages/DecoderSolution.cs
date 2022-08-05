using System.Text;
using FluentAssertions;
using Xunit;

namespace Codewars.HelpTheGeneralDecodeSecretEnemyMessages;

public class SolutionTest
{
    [Theory]
    [InlineData("Hello World!")]
    public void ShouldBeCompatibleWithEncoder(string message)
        => Decoder.Decode(Encoder.Encode(message)).Should().Be(message);

    [Theory]
    [InlineData(
        "The quick brown fox jumped over the lazy developer.",
        "yFNYhdmEdViBbxc40,ROYNxwfwvjg5CHUYUhiIkp2CMIvZ.1qPz")]
    [InlineData(
        "The secrecy system has systematically denied",
        "yFNYeZWZn4l5gCB GY1gZG7?D6CV9y9C,?sLbFlDHoMW")]
    [InlineData(
        "American historians access to the records of American",
        "1ZNtt58GxrgKydwwtxTOZRuc.6v5tP9.9b9As31xAQXNiXaPuun1L")]
    [InlineData(
        "history. Of late we find ourselves relying on archives",
        "pJrZkmYVxPA5oJGXUW?OR?E3MJglc5,39Cd41rh1HHsGhrERKOPWdq")]
    [InlineData(
        "of the former Soviet Union in Moscow to resolve",
        "Dx6Z2Zf9PgyS3ExpLy?Yo,E6rUvfOPgzV8gqpDS8o0OGzfu")]
    [InlineData(
        "questions of what was going on in Washington at",
        "HqNJKNRGmVH1WxNj?AEzXDWiYUURtoqC0be6ElAqqkXdiWq")]
    [InlineData(
        "mid-century. The Venona intercepts contained",
        "zJF-CZXBFglEWVNXUR?CQWg0YUCVxhW6UCdQBu7fOaMW")]
    [InlineData(
        "overwhelming proof of the activities of Soviet spy",
        "DuNt QK4wK..WNwpFM12RDfSfkSbWxBCUWUWp3r8dyY7vPJMOJ")]
    [InlineData(
        "networks in America, complete with names,",
        "BtzGkmaNxK.5q yTT0m3oRLVTbx5CPkCUYdvY0oUE")]
    [InlineData(
        "dates, places, and deeds.",
        "hdzmeifiUsUSgz9jlz1K6YB?v")]
    public void ShouldDecodeTheMessages(string input, string encoded)
        => Decoder.Decode(encoded).Should().Be(input);
}

/// <summary>
///     Decode a message.
/// </summary>
public static class Decoder
{
    private const string Key = "bdhpF,82QsLirJejtNmzZKgnB3SwTyXG ?.6YIcflxVC5WE94UA1OoD70MkvRuPqHa";

    public static string Decode(string encodedText)
    {
        var decodedText = new StringBuilder(encodedText.Length);

        for (var characterIndex = 0; characterIndex < encodedText.Length; characterIndex++)
        {
            var encodedCharacter = encodedText[characterIndex];

            var decodedCharacter = DecodedCharacter(encodedCharacter, characterIndex);

            decodedText.Append(decodedCharacter);
        }

        return decodedText.ToString();
    }

    private static char DecodedCharacter(char encodedCharacter, int characterIndex)
    {
        if (!Key.Contains(encodedCharacter))
            return encodedCharacter;

        var decodedCharPosition = (Key.IndexOf(encodedCharacter) - (characterIndex + 1)) % Key.Length;

        var decodedCharIndex = decodedCharPosition >= 0 ? decodedCharPosition : ^-decodedCharPosition;

        return Key[decodedCharIndex];
    }
}

/// <summary>
///     Encode a message.
/// </summary>
/// <remarks>Not given at the beginning of the exercise.</remarks>
public static class Encoder
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,? *";

    private static char EncryptOne(char c, int n)
    {
        for (var i = 0; i < n; i++)
        {
            var ind = Alphabet.IndexOf(c);
            c = Alphabet[((ind + 1) * 2 - 1) % Alphabet.Length];
        }

        return c;
    }

    public static string Encode(string str)
    {
        var result = new StringBuilder(str.Length);

        for (var i = 0; i < str.Length; i++)
            if (Alphabet.IndexOf(str[i]) < 0)
                result.Append(str[i]);
            else
                result.Append(EncryptOne(str[i], i + 1));

        return result.ToString();
    }
}