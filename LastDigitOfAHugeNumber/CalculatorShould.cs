using FluentAssertions;
using Xunit;

namespace Codewars.LastDigitOfAHugeNumber;

public class CalculatorShould
{
    [Theory]
    [ClassData(typeof(LastDigitTestDataGenerator))]
    public void Return_the_last_digit_of_a_huge_number(LastDigitCase testCase)
        => Calculator.LastDigit(testCase.Test).Should().Be(testCase.Expect);
}