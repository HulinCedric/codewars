using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.CategorizeNewMember;

public class KataOpenOrSeniorTests
{
    [Fact]
    public void SampleTest()
    {
        Kata.OpenOrSenior(new[] { new[] { 45, 12 }, new[] { 55, 21 }, new[] { 19, 2 }, new[] { 104, 20 } })
            .Should()
            .BeEquivalentTo("Open", "Senior", "Open", "Senior");

        Kata.OpenOrSenior(new[] { new[] { 3, 12 }, new[] { 55, 1 }, new[] { 91, -2 }, new[] { 54, 23 } })
            .Should()
            .BeEquivalentTo("Open", "Open", "Open", "Open");

        Kata.OpenOrSenior(new[] { new[] { 59, 12 }, new[] { 45, 21 }, new[] { -12, -2 }, new[] { 12, 12 } })
            .Should()
            .BeEquivalentTo("Senior", "Open", "Open", "Open");
    }
}

public static class Kata
{
    public static IEnumerable<string> OpenOrSenior(int[][] personsInformation)
    {
        var potentialMembers = personsInformation
            .Select(info => new PotentialMember(info[0], info[1]));

        return potentialMembers.Select(potentialMember => potentialMember.GetCategory());
    }

    private record PotentialMember(int Age, int Handicap)
    {
        public string GetCategory()
            => IsSenior() ? "Senior" : "Open";

        private bool IsSenior()
        {
            var atLeast55YearsOld = Age >= 55;
            var haveAHandicapGreaterThan7 = Handicap > 7;

            return atLeast55YearsOld && haveAHandicapGreaterThan7;
        }
    }
}