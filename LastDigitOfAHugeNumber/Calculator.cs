using System;
using System.Linq;

namespace Codewars.LastDigitOfAHugeNumber;

public static class Calculator
{
    public static int LastDigit(int[] array)
    {
        if (array.IsEmpty())
            return 1;

        var exponent = array.Last();

        foreach (var number in array.Reverse().Skip(1))
            exponent = number.Power(exponent);

        return exponent % 10;
    }

    private static int Power(this int number, int exponent)
        => exponent switch
        {
            0 => 1,
            1 => number,
            2 => number * number,
            _ => number.ModularPower(exponent)
        };

    private static int ModularPower(this int number, int exponent)
        => (int)Math.Pow(
            number.ReducedBase(),
            exponent.ReducedExponent());

    /// <summary>
    ///     Calculates a reduced representation of the base number for the modular power operation.
    /// </summary>
    /// <remarks>
    ///     This takes advantage of the property that the pattern of the last digits of numbers raised to the power repeats
    ///     every 20 numbers.
    ///     The selection of the reduction range 20 for the base is related to the periodicity of the last digits of powers in
    ///     base 10. More specifically, the sequence of the last digits of any number raised to powers is periodic, and for any
    ///     number, this period will be a divisor of 20. Thus, the base is reduced modulo 20.
    /// </remarks>
    private static int ReducedBase(this int number)
        => number < 3 ?
               number :
               (number - 3) % 20 + 3;

    /// <summary>
    ///     Calculates a reduced representation of the exponent for the modular power operation.
    /// </summary>
    /// <remarks>
    ///     This takes advantage of the property that the pattern of the last digits of numbers raised to the power repeats
    ///     every 4 numbers.
    ///     As for the exponent, the reduction range of 4 is selected due to another periodic property, this time related to
    ///     the exponents. More specifically, for any number, the sequence of the last digits of its powers is periodic with a
    ///     period that divides 4. This is why the exponent is reduced modulo 4.
    /// </remarks>
    private static int ReducedExponent(this int exponent)
        => (exponent - 3) % 4 + 3;
}