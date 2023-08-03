using System.Linq;

namespace Codewars.LastDigitOfAHugeNumber;

public readonly record struct LastDigitCase(int[] Test, int Expect)
{
    public override string ToString()
        => $"{{ Test = [{PrintTestValues()}], Expect = {Expect} }}";

    private string PrintTestValues()
        => string.Join(", ", Test.Select(x => x.ToString()).ToArray());
}