using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.HumanReadableDurationFormat;

public class Tests
{
    [Theory]
    [InlineData(0, "now")]
    [InlineData(1, "1 second")]
    [InlineData(62, "1 minute and 2 seconds")]
    [InlineData(120, "2 minutes")]
    [InlineData(3_662, "1 hour, 1 minute and 2 seconds")]
    [InlineData(15_731_080, "182 days, 1 hour, 44 minutes and 40 seconds")]
    [InlineData(132_030_240, "4 years, 68 days, 3 hours and 4 minutes")]
    [InlineData(205_851_834, "6 years, 192 days, 13 hours, 3 minutes and 54 seconds")]
    [InlineData(253_374_061, "8 years, 12 days, 13 hours, 41 minutes and 1 second")]
    [InlineData(242_062_374, "7 years, 246 days, 15 hours, 32 minutes and 54 seconds")]
    [InlineData(101_956_166, "3 years, 85 days, 1 hour, 9 minutes and 26 seconds")]
    [InlineData(33_243_586, "1 year, 19 days, 18 hours, 19 minutes and 46 seconds")]
    public void BasicTests(int durationInSeconds, string expected)
        => HumanTimeFormat.formatDuration(durationInSeconds).Should().Be(expected);
}

public static class HumanTimeFormat
{
    public static string formatDuration(DurationInSeconds duration)
        => duration.ToString();
}

public readonly record struct DurationInSeconds(int Value)
{
    public static implicit operator DurationInSeconds(int value)
        => new(value);

    public static int operator /(DurationInSeconds a, int b)
        => a.Value / b;

    public override string ToString()
        => GetOrderedDurationParts().GetRepresentation();

    private DurationParts GetOrderedDurationParts()
        => new(
            new IPresentDurationPart[]
            {
                Year.In(this),
                Day.In(this),
                Hour.In(this),
                Minute.In(this),
                Second.In(this)
            });
}

public class DurationParts
{
    private const string Now = "now";

    private readonly IPresentDurationPart[] parts;

    public DurationParts(IPresentDurationPart[] parts)
        => this.parts = parts;

    public string GetRepresentation()
    {
        var representations = parts
            .Select(p => p.Present())
            .Where(r => !string.IsNullOrEmpty(r))
            .ToArray();

        if (representations.Length == 0)
            return Now;

        if (representations.Length == 1)
            return representations.Single();

        return JoinRepresentations(representations);
    }

    private static string JoinRepresentations(string[] representations)
    {
        var last = ^1;
        var allExceptLast = ..last;

        var firstParts = representations[allExceptLast];
        var lastPart = representations[last];

        var joinedFirstParts = string.Join(", ", firstParts);

        return $"{joinedFirstParts} and {lastPart}";
    }
}

public interface IPresentDurationPart
{
    string Present();
}

public readonly record struct Second(int Value) : IPresentDurationPart
{
    private const int NumberOfSecondsIn = 1;
    private const int NumberOfSecondsInOneMinute = 60;
    private const string DurationPartName = "second";

    public string Present()
        => Value.GetDurationPartRepresentation(DurationPartName);

    public static Second In(DurationInSeconds durationInSeconds)
        => new(durationInSeconds / NumberOfSecondsIn % NumberOfSecondsInOneMinute);
}

public readonly record struct Minute(int Value) : IPresentDurationPart
{
    private const int NumberOfSecondsIn = 60;
    private const int NumberOfMinutesInOneHour = 60;
    private const string DurationPartName = "minute";

    public string Present()
        => Value.GetDurationPartRepresentation(DurationPartName);

    public static Minute In(DurationInSeconds duration)
        => new(duration / NumberOfSecondsIn % NumberOfMinutesInOneHour);
}

public readonly record struct Hour(int Value) : IPresentDurationPart
{
    private const int NumberOfSecondsIn = 3_600;
    private const int NumberOfHoursInOneDay = 24;
    private const string DurationPartName = "hour";

    public string Present()
        => Value.GetDurationPartRepresentation(DurationPartName);

    public static Hour In(DurationInSeconds duration)
        => new(duration / NumberOfSecondsIn % NumberOfHoursInOneDay);
}

public readonly record struct Day(int Value) : IPresentDurationPart
{
    private const int NumberOfSecondsIn = 86_400;
    private const int NumberOfDaysInOneYear = 365;
    private const string DurationPartName = "day";

    public string Present()
        => Value.GetDurationPartRepresentation(DurationPartName);

    public static Day In(DurationInSeconds duration)
        => new(duration / NumberOfSecondsIn % NumberOfDaysInOneYear);
}

public readonly record struct Year(int Value) : IPresentDurationPart
{
    private const int NumberOfSecondsIn = 31_556_952;
    private const string DurationPartName = "year";

    public string Present()
        => Value.GetDurationPartRepresentation(DurationPartName);

    public static Year In(DurationInSeconds duration)
        => new(duration / NumberOfSecondsIn);
}

public static class DurationPartPresentationExtensions
{
    public static string GetDurationPartRepresentation(this int durationValue, string durationPartName)
        => durationValue switch
        {
            0 => string.Empty,
            1 => $"{durationValue} {durationPartName}",
            _ => $"{durationValue} {durationPartName}s"
        };
}