using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.RangeExtraction;

public class RangeExtractorTest
{
    [Theory]
    [InlineData(new[] { 1, 2 }, "1,2")]
    [InlineData(new[] { 1, 2, 3 }, "1-3")]
    [InlineData(
        new[] { -6, -3, -2, -1, 0, 1, 3, 4, 5, 7, 8, 9, 10, 11, 14, 15, 17, 18, 19, 20 },
        "-6,-3-1,3-5,7-11,14,15,17-20")]
    [InlineData(new[] { -3, -2, -1, 2, 10, 15, 16, 18, 19, 20 }, "-3--1,2,10,15,16,18-20")]
    public void SimpleTests(int[] orderedIntegers, string rangeRepresentation)
        => RangeExtraction.Extract(orderedIntegers).Should().Be(rangeRepresentation);
}

public static class RangeExtraction
{
    public static string Extract(int[] orderedIntegers)
    {
        var groupOfAdjacentIntegers = GroupAdjacentIntegers(orderedIntegers);

        var groupRepresentations = PrintGroupOfAdjacentIntegers(groupOfAdjacentIntegers);

        var rangeRepresentation = PrintRange(groupRepresentations);

        return rangeRepresentation;
    }

    private static string PrintRange(IEnumerable<string> groupRepresentations)
        => string.Join(",", groupRepresentations);

    private static List<List<int>> GroupAdjacentIntegers(int[] integers)
        => integers
            .Skip(1)
            .Aggregate(
                new List<List<int>> { new() { integers.First() } },
                (groupedIntegers, currentInteger) =>
                {
                    var lastGroup = groupedIntegers.Last();
                    if (lastGroup.Last() == currentInteger - 1)
                        lastGroup.Add(currentInteger);
                    else
                        groupedIntegers.Add(new List<int> { currentInteger });

                    return groupedIntegers;
                });

    private static IEnumerable<string> PrintGroupOfAdjacentIntegers(List<List<int>> groupOfAdjacentIntegers)
    {
        foreach (var adjacentIntegers in groupOfAdjacentIntegers)
            if (IntegerInterval.IsIntegerInterval(adjacentIntegers))
                yield return IntegerInterval.Print(adjacentIntegers);
            else
                yield return IndividualIntegers.Print(adjacentIntegers);
    }
}

/// <summary>
///     An interval is an unbroken range of numbers.
/// </summary>
internal static class IntegerInterval
{
    private const int IntervalLength = 3;

    internal static bool IsIntegerInterval(List<int> integers)
        => integers.Count >= IntervalLength;

    internal static string Print(List<int> integers)
        => $"{integers.First()}-{integers.Last()}";
}

internal static class IndividualIntegers
{
    internal static string Print(List<int> integers)
        => string.Join(",", integers);
}