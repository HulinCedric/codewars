using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Interval = System.ValueTuple<int, int>;

namespace Codewars.SumOfIntervals;

public class IntervalTest
{
    [Fact]
    public void ShouldHandleEmptyIntervals()
    {
        Intervals.SumIntervals(new Interval[] { }).Should().Be(0);
        Intervals.SumIntervals(new[] { (4, 4) }).Should().Be(0);
        Intervals.SumIntervals(new[] { (4, 4), (6, 6), (8, 8) }).Should().Be(0);
    }

    [Fact]
    public void ShouldAddDisjoinedIntervals()
    {
        Intervals.SumIntervals(new[] { (1, 2), (6, 10), (11, 15) }).Should().Be(9);
        Intervals.SumIntervals(new[] { (4, 8), (9, 10), (15, 21) }).Should().Be(11);
        Intervals.SumIntervals(new[] { (-1, 4), (-5, -3) }).Should().Be(7);
        Intervals.SumIntervals(new[] { (-245, -218), (-194, -179), (-155, -119) }).Should().Be(78);
    }

    [Fact]
    public void ShouldAddAdjacentIntervals()
    {
        Intervals.SumIntervals(new[] { (1, 2), (2, 6), (6, 55) }).Should().Be(54);
        Intervals.SumIntervals(new[] { (-2, -1), (-1, 0), (0, 21) }).Should().Be(23);
    }

    [Fact]
    public void ShouldAddOverlappingIntervals()
    {
        Intervals.SumIntervals(new[] { (1, 4), (7, 10), (3, 5) }).Should().Be(7);
        Intervals.SumIntervals(new[] { (5, 8), (3, 6), (1, 2) }).Should().Be(6);
        Intervals.SumIntervals(new[] { (1, 5), (10, 20), (1, 6), (16, 19), (5, 11) }).Should().Be(19);
    }

    [Fact]
    public void ShouldHandleMixedIntervals()
    {
        Intervals.SumIntervals(new[] { (2, 5), (-1, 2), (-40, -35), (6, 8) }).Should().Be(13);
        Intervals.SumIntervals(new[] { (-7, 8), (-2, 10), (5, 15), (2000, 3150), (-5400, -5338) })
            .Should()
            .Be(1234);
        Intervals.SumIntervals(new[] { (-101, 24), (-35, 27), (27, 53), (-105, 20), (-36, 26) })
            .Should()
            .Be(158);
    }
}

public static class Intervals
{
    public static int SumIntervals(Interval[] intervals)
        => MergeOverlappingIntervals(intervals)
            .Sum(ComputeIntervalDistance);

    /// <summary>
    ///     Merges overlapping intervals.
    /// </summary>
    /// <param name="intervals">A set of intervals</param>
    /// <returns>Overlapping intervals merged.</returns>
    /// <seealso href="https://www.geeksforgeeks.org/merging-intervals/" />
    private static Interval[] MergeOverlappingIntervals(Interval[] intervals)
    {
        // Test if the given set has at least one interval 
        if (intervals.Length <= 0)
            return intervals;

        var orderedIntervals = intervals.OrderBy(x => x, new IntervalComparer()).ToArray();

        // Create an empty stack of intervals
        var stack = new Stack<Interval>();

        // Push the first interval to stack
        stack.Push(orderedIntervals[0]);

        // Start from the next interval and merge if necessary
        for (var i = 1; i < orderedIntervals.Length; i++)
        {
            // get interval from stack top
            var top = stack.Peek();

            // if current interval is not overlapping with stack top, Push it to the stack
            if (top.Item2 < orderedIntervals[i].Item1)
            {
                stack.Push(orderedIntervals[i]);
            }

            // Otherwise update the ending time of top if ending of current interval is more 
            else if (top.Item2 < orderedIntervals[i].Item2)
            {
                top.Item2 = orderedIntervals[i].Item2;
                stack.Pop();
                stack.Push(top);
            }
        }

        return stack.ToArray();
    }

    private static int ComputeIntervalDistance(Interval interval)
        => Math.Abs(interval.Item1 - interval.Item2);

    /// <summary>
    ///     Sort the intervals in increasing order of start time.
    /// </summary>
    private class IntervalComparer : IComparer<Interval>
    {
        public int Compare(Interval first, Interval second)
        {
            if (first.Item1 == second.Item2)
                return first.Item2 - second.Item2;

            return first.Item1 - second.Item1;
        }
    }
}