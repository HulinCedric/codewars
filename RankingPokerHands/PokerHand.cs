using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.RankingPokerHands;

public class SolutionTest
{
    [Theory]
    [InlineData("Highest straight flush wins", Result.Loss, "2H 3H 4H 5H 6H", "KS AS TS QS JS")]
    [InlineData("Straight flush wins of 4 of a kind", Result.Win, "2H 3H 4H 5H 6H", "AS AD AC AH JD")]
    [InlineData("Highest 4 of a kind wins", Result.Win, "AS AH 2H AD AC", "JS JD JC JH 3D")]
    [InlineData("4 Of a kind wins of full house", Result.Loss, "2S AH 2H AS AC", "JS JD JC JH AD")]
    [InlineData("Full house wins of flush", Result.Win, "2S AH 2H AS AC", "2H 3H 5H 6H 7H")]
    [InlineData("Highest flush wins", Result.Win, "AS 3S 4S 8S 2S", "2H 3H 5H 6H 7H")]
    [InlineData("Flush wins of straight", Result.Win, "2H 3H 5H 6H 7H", "2S 3H 4H 5S 6C")]
    [InlineData("Equal straight is tie", Result.Tie, "2S 3H 4H 5S 6C", "3D 4C 5H 6H 2S")]
    [InlineData("Straight wins of three of a kind", Result.Win, "2S 3H 4H 5S 6C", "AH AC 5H 6H AS")]
    [InlineData("3 Of a kind wins of two pair", Result.Loss, "2S 2H 4H 5S 4C", "AH AC 5H 6H AS")]
    [InlineData("2 Pair wins of pair", Result.Win, "2S 2H 4H 5S 4C", "AH AC 5H 6H 7S")]
    [InlineData("Highest pair wins", Result.Loss, "6S AD 7H 4S AS", "AH AC 5H 6H 7S")]
    [InlineData("Pair wins of nothing", Result.Loss, "2S 2H AH KS JC", "3H 3C 2H 4H 5S")]
    [InlineData("Highest card loses", Result.Loss, "2S 3H 6H 7S 9C", "7H 3C TH 6H 9S")]
    [InlineData("Highest card loses", Result.Loss, "AD QD JS TD 9H", "AH KH 4H 2S 3H")]
    [InlineData("Highest card wins", Result.Win, "4S 5H 6H TS AC", "3S 5H 6H TS AC")]
    [InlineData("Equal cards is tie", Result.Tie, "2S AH 4H 5S 6C", "AD 4C 5H 6H 2C")]
    public void PokerHandTest(string description, Result expected, string hand, string opponentHand)
        => new PokerHand(hand).CompareWith(new PokerHand(opponentHand)).Should().Be(expected, description);
}

public enum Result
{
    Win,
    Loss,
    Tie
}

/// <summary>
///     Inspired by <see href="https://monstercoder.github.io/2016/project-euler-54/" /> solution from @MonsterCoder.
/// </summary>
public class PokerHand
{
    private readonly ImmutableList<Card> cards;

    private readonly IEnumerable<IRank> ranks;

    public PokerHand(string hand)
    {
        cards = hand.Split(' ').Select(c => new Card(c)).ToImmutableList();

        ranks = new List<IRank>
        {
            new HighCard(cards),
            new OnePair(cards),
            new TwoPairs(cards),
            new ThreeOfKinds(cards),
            new Straight(cards),
            new Flush(cards),
            new FullHouse(cards),
            new FourOfKinds(cards),
            new StraightFlush(cards),
            new RoyalFlush(cards)
        };
    }

    private HandValue Value
        => ranks
            .Select((rank, order) => rank.GetHandValue(order))
            .MaxBy(handValue => handValue.Value);

    public Result CompareWith(PokerHand opponent)
        => Value.CompareWith(opponent.Value);

    public override string ToString()
        => $"{string.Join(" ", cards.OrderBy(c => c.Value).ToArray())} : {Value}";
}

public readonly record struct HandValue : IComparable<HandValue>
{
    public static readonly HandValue NoValue = new(0, 0, 0);

    private readonly int kickersValue;
    private readonly int rankOrder;
    private readonly int rankValue;

    /// <summary>
    ///     <para>
    ///         Hand value is a number composed of rank order, rank value and kickers value. Each component value in the hand
    ///         value have weight in order to compare by priority. Here is a repartition schema after weight have been apply:
    ///     </para>
    ///     <para>rank order:   x________,________</para>
    ///     <para>rank value:   _xxxxxxxx,________</para>
    ///     <para>kickersValue: _________,xxxxxxxx</para>
    /// </summary>
    /// <param name="rankOrder">The rank order, for hand rank comparison.</param>
    /// <param name="rankValue">The rank value, to compare hand with same rank order.</param>
    /// <param name="kickersValue">The kickers value, to break ties between hands of the same rank.</param>
    /// <example>
    ///     <code>HandValue(5, 1234567, 9876543)</code>
    ///     Resulting in: 5_1234567.9876543
    /// </example>
    public HandValue(int rankOrder, int rankValue, int kickersValue)
    {
        this.rankOrder = rankOrder;
        this.rankValue = rankValue;
        this.kickersValue = kickersValue;

        Value = rankOrder * 10_000_000 + rankValue % 10_000_000 + kickersValue / 10_000_000m;
    }

    public decimal Value { get; }

    public int CompareTo(HandValue opponent)
        => Value.CompareTo(opponent.Value);

    public override string ToString()
        => $"{rankOrder} : {rankValue} : {kickersValue} = {Value}";

    public Result CompareWith(HandValue opponentValue)
        => this == opponentValue ? Result.Tie :
           this > opponentValue ? Result.Win :
           Result.Loss;

    public static bool operator <(HandValue left, HandValue right)
        => left.CompareTo(right) < 0;

    public static bool operator >(HandValue left, HandValue right)
        => left.CompareTo(right) > 0;

    public static bool operator <=(HandValue left, HandValue right)
        => left.CompareTo(right) <= 0;

    public static bool operator >=(HandValue left, HandValue right)
        => left.CompareTo(right) >= 0;
}

public interface IRank
{
    HandValue GetHandValue(int rankOrder);
}

public readonly record struct Card : IComparable<Card>
{
    private static readonly IDictionary<char, int> NameValueCorrespondences = new Dictionary<char, int>
    {
        { '1', 1 },
        { '2', 2 },
        { '3', 3 },
        { '4', 4 },
        { '5', 5 },
        { '6', 6 },
        { '7', 7 },
        { '8', 8 },
        { '9', 9 },
        { 'T', 10 },
        { 'J', 11 },
        { 'Q', 12 },
        { 'K', 13 },
        { 'A', 14 }
    };

    public Card(string representation) :
        this(representation[0], representation[1])
    {
    }

    private Card(char name, char suit)
    {
        Name = name;
        Suit = suit;
        Value = NameValueCorrespondences[name];
    }

    public char Name { get; }
    public char Suit { get; }
    public int Value { get; }

    public int CompareTo(Card other)
        => Value.CompareTo(other.Value);

    public override string ToString()
        => $"{Name}{Suit}";

    public static bool operator <(Card left, Card right)
        => left.CompareTo(right) < 0;

    public static bool operator >(Card left, Card right)
        => left.CompareTo(right) > 0;

    public static bool operator <=(Card left, Card right)
        => left.CompareTo(right) <= 0;

    public static bool operator >=(Card left, Card right)
        => left.CompareTo(right) >= 0;
}

public abstract class Rank : IRank
{
    protected Rank(IEnumerable<Card> cards)
        => Cards = cards;

    protected IEnumerable<Card> Cards { get; }

    public HandValue GetHandValue(int rankOrder)
    {
        if (IsMatch() == false)
            return HandValue.NoValue;

        return new HandValue(
            rankOrder,
            GetValue(),
            GetKickersValue());
    }

    protected abstract int GetValue();

    private int GetKickersValue()
        => Kickers.GetValue(Cards);

    protected abstract bool IsMatch();
}

public class RoyalFlush : StraightFlush
{
    public RoyalFlush(IEnumerable<Card> cards) : base(cards)
    {
    }

    protected override int GetValue()
        => Cards.Sum(c => c.Value);

    protected override bool IsMatch()
        => base.IsMatch() &&
           Cards.Max().Value == new Card("AS").Value;
}

public class StraightFlush : Straight
{
    private const int NumberOfCardsInHand = 5;

    public StraightFlush(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<IGrouping<char, Card>> FlushGroup
        => Cards.ThatHaveSameSuit().AndNumberOfCardsIs(NumberOfCardsInHand);

    protected override int GetValue()
        => Cards.Sum(c => c.Value);

    protected override bool IsMatch()
        => base.IsMatch() && FlushGroup.Any();
}

public class FourOfKinds : Rank
{
    private const int NumberOfCardsQuadruplet = 4;
    private const int NumberOfQuadrupletInRank = 1;

    public FourOfKinds(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<Card> QuadrupletCards
        => QuadrupletGroup.SelectMany(card => card);

    private IEnumerable<IGrouping<int, Card>> QuadrupletGroup
        => Cards
            .ThatHaveSameValue()
            .AndNumberOfCardsIs(NumberOfCardsQuadruplet);

    protected override int GetValue()
        => QuadrupletCards.Sum(card => card.Value);

    protected override bool IsMatch()
        => QuadrupletGroup.Count() == NumberOfQuadrupletInRank;
}

public class FullHouse : Rank
{
    private const int NumberOfCardsInOnePair = 2;
    private const int NumberOfPairInRank = 1;

    private const int NumberOfCardsTriplet = 3;
    private const int NumberOfTripletInRank = 1;

    public FullHouse(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<IGrouping<int, Card>> PairGroup
        => Cards
            .ThatHaveSameValue()
            .AndNumberOfCardsIs(NumberOfCardsInOnePair);

    private IEnumerable<IGrouping<int, Card>> TripletGroup
        => Cards
            .ThatHaveSameValue()
            .AndNumberOfCardsIs(NumberOfCardsTriplet);

    protected override int GetValue()
        => Cards.Sum(c => c.Value);

    protected override bool IsMatch()
        => PairGroup.Count() == NumberOfPairInRank &&
           TripletGroup.Count() == NumberOfTripletInRank;
}

public class Flush : Rank
{
    public Flush(IEnumerable<Card> cards) : base(cards)
    {
    }

    private int NumberOfCardsInHand
        => Cards.Count();

    protected override int GetValue()
        => Cards.Sum(card => card.Value);

    protected override bool IsMatch()
        => Cards.ThatHaveSameSuit().AndNumberOfCardsIs(NumberOfCardsInHand).Any();
}

public class Straight : Rank
{
    public Straight(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<int> ExpectedSequenceValues
        => Enumerable.Range(SmallestCard.Value, NumberOfCardsInHand);

    private IEnumerable<int> HandSequenceValues
        => Cards
            .OrderBy(card => card.Value)
            .Select(card => card.Value);

    private int NumberOfCardsInHand
        => Cards.Count();

    private Card SmallestCard
        => Cards.Min();

    protected override int GetValue()
        => Cards.Max(card => card.Value);

    protected override bool IsMatch()
        => HandSequenceValues.SequenceEqual(ExpectedSequenceValues);
}

public class ThreeOfKinds : Rank
{
    private const int NumberOfCardsTriplet = 3;
    private const int NumberOfTripletInRank = 1;

    public ThreeOfKinds(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<Card> TripletCards
        => TripletGroup.SelectMany(card => card);

    private IEnumerable<IGrouping<int, Card>> TripletGroup
        => Cards
            .ThatHaveSameValue()
            .AndNumberOfCardsIs(NumberOfCardsTriplet);

    protected override int GetValue()
        => TripletCards.Sum(card => card.Value);

    protected override bool IsMatch()
        => TripletGroup.Count() == NumberOfTripletInRank;
}

public class TwoPairs : OnePair
{
    private const int NumberOfCardsInOnePair = 2;
    private const int NumberOfPairInRank = 2;

    public TwoPairs(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<Card> PairCards
        => PairGroup.SelectMany(card => card);

    private IEnumerable<IGrouping<int, Card>> PairGroup
        => Cards
            .ThatHaveSameValue()
            .AndNumberOfCardsIs(NumberOfCardsInOnePair);

    protected override int GetValue()
        => PairCards.Sum(card => card.Value);

    protected override bool IsMatch()
        => PairGroup.Count() == NumberOfPairInRank;
}

public class OnePair : Rank
{
    private const int NumberOfCardsInOnePair = 2;
    private const int NumberOfPairInRank = 1;

    public OnePair(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<Card> PairCards
        => PairGroup.SelectMany(card => card);

    private IEnumerable<IGrouping<int, Card>> PairGroup
        => Cards
            .ThatHaveSameValue()
            .AndNumberOfCardsIs(NumberOfCardsInOnePair);

    protected override int GetValue()
        => PairCards.Sum(card => card.Value);

    protected override bool IsMatch()
        => PairGroup.Count() == NumberOfPairInRank;
}

public class HighCard : Rank
{
    public HighCard(IEnumerable<Card> cards) : base(cards)
    {
    }

    protected override int GetValue()
        => Kickers.GetValue(Cards);

    protected override bool IsMatch()
        => true;
}

/// <seealso href="https://en.wikipedia.org/wiki/Kicker_(poker)" />
public static class Kickers
{
    public static int GetValue(IEnumerable<Card> cards)
        => (int)cards.OrderBy(card => card.Value).Select((card, index) => card.Value * Math.Pow(10, index)).Sum();
}

public static class PokerHandExtensions
{
    public static IEnumerable<IGrouping<int, Card>> ThatHaveSameValue(this IEnumerable<Card> cards)
        => cards
            .GroupBy(c => c.Value);

    public static IEnumerable<IGrouping<char, Card>> ThatHaveSameSuit(this IEnumerable<Card> cards)
        => cards
            .GroupBy(c => c.Suit);

    public static IEnumerable<IGrouping<T, Card>> AndNumberOfCardsIs<T>(
        this IEnumerable<IGrouping<T, Card>> groupOfCards,
        int numberOfCards)
        => groupOfCards
            .Where(g => g.Count() == numberOfCards);
}