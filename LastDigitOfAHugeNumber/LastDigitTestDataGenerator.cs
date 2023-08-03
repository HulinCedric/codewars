using System;
using Xunit;

namespace Codewars.LastDigitOfAHugeNumber;

public class LastDigitTestDataGenerator : TheoryData<LastDigitCase>
{
    public LastDigitTestDataGenerator()
    {
        var rnd = new Random();
        var rand1 = rnd.Next(0, 100);
        var rand2 = rnd.Next(0, 10);

        Add(new LastDigitCase(new int[0], 1));
        Add(new LastDigitCase(new[] { 0, 0 }, 1));
        Add(new LastDigitCase(new[] { 0, 0, 0 }, 0));
        Add(new LastDigitCase(new[] { 1, 2 }, 1));
        Add(new LastDigitCase(new[] { 3, 4, 5 }, 1));
        Add(new LastDigitCase(new[] { 4, 3, 6 }, 4));
        Add(new LastDigitCase(new[] { 7, 6, 21 }, 1));
        Add(new LastDigitCase(new[] { 12, 30, 21 }, 6));
        Add(new LastDigitCase(new[] { 2, 2, 2, 0 }, 4));
        Add(new LastDigitCase(new[] { 937640, 767456, 981242 }, 0));
        Add(new LastDigitCase(new[] { 123232, 694022, 140249 }, 6));
        Add(new LastDigitCase(new[] { 499942, 898102, 846073 }, 6));
        Add(new LastDigitCase(new[] { rand1 }, rand1 % 10));
        Add(new LastDigitCase(new[] { rand1, rand2 }, (int)Math.Pow(rand1 % 10, rand2) % 10));
    }
}