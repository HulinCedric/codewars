using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.SortablePokerHands;

public class SolutionTest
{
    [Fact]
    public void PokerHandSortTest()
    {
        // Arrange
        var expected = new List<PokerHand>
        {
            new("KS AS TS QS JS"),
            new("2H 3H 4H 5H 6H"),
            new("AS AD AC AH JD"),
            new("JS JD JC JH 3D"),
            new("2S AH 2H AS AC"),
            new("AS 3S 4S 8S 2S"),
            new("2H 3H 5H 6H 7H"),
            new("2S 3H 4H 5S 6C"),
            new("2D AC 3H 4H 5S"),
            new("AH AC 5H 6H AS"),
            new("2S 2H 4H 5S 4C"),
            new("AH AC 5H 6H 7S"),
            new("AH AC 4H 6H 7S"),
            new("2S AH 4H 5S KC"),
            new("2S 3H 6H 7S 9C")
        };
        var random = new Random((int)DateTime.Now.Ticks);
        var actual = expected.OrderBy(_ => random.Next()).ToList();

        // Act
        actual.Sort();

        // Assert
        actual.Should().ContainInOrder(expected);
    }
}

public class PokerHand : IComparable<PokerHand>
{
    private readonly List<Card> cards;

    public PokerHand(string hand)
    {
        var highAceRankCards = Cards.CreateHighAceRankVersion(hand);
        var lowAceRankCards = Cards.CreateLowAceRankVersion(hand);

        var ranks = BuildRanks(highAceRankCards, lowAceRankCards);

        cards = highAceRankCards;
        Value = ComputeHandValue(ranks);
    }

    private HandValue Value { get; }

    public int CompareTo(PokerHand? opponent)
    {
        if (opponent is null)
            return 1;
        return -Value.CompareTo(opponent.Value);
    }

    private static List<IRank> BuildRanks(
        List<Card> highAceRankCards,
        List<Card> lowAceRankCards)
        => new()
        {
            new HighCard(highAceRankCards),
            new OnePair(highAceRankCards),
            new TwoPairs(highAceRankCards),
            new ThreeOfKinds(highAceRankCards),
            new Straight(highAceRankCards),
            new Straight(lowAceRankCards),
            new Flush(highAceRankCards),
            new FullHouse(highAceRankCards),
            new FourOfKinds(highAceRankCards),
            new StraightFlush(highAceRankCards),
            new StraightFlush(lowAceRankCards),
            new RoyalFlush(highAceRankCards)
        };

    private static HandValue ComputeHandValue(IEnumerable<IRank> ranks)
        => ranks
            .Select(rank => rank.GetHandValue())
            .MaxBy(handValue => handValue.Value);

    public override string ToString()
        => $"{string.Join(" ", cards.OrderBy(card => card.Value).ToArray())} : {Value}";
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
    HandValue GetHandValue();
}

internal static class Cards
{
    private static IEnumerable<Card> Create(string hand, ICardFactory cardFactory)
        => hand.Split(' ')
            .Select(cardRepresentation => cardFactory.CreateCard(cardRepresentation[0], cardRepresentation[1]));

    public static List<Card> CreateHighAceRankVersion(string hand)
        => Create(hand, new Card.HighAceRankCardFactory()).ToList();

    public static List<Card> CreateLowAceRankVersion(string hand)
        => Create(hand, new Card.LowAceRankCardFactory()).ToList();
}

internal interface ICardFactory
{
    Card CreateCard(char cardName, char cardSuit);
}

public readonly record struct Card : IComparable<Card>
{
    private Card(char name, char suit, int value)
    {
        Name = name;
        Suit = suit;
        Value = value;
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

    internal class HighAceRankCardFactory : ICardFactory
    {
        private static readonly IDictionary<char, int> NameValueCorrespondences = new Dictionary<char, int>
        {
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

        public Card CreateCard(char cardName, char cardSuit)
            => new(cardName, cardSuit, NameValueCorrespondences[cardName]);
    }

    internal class LowAceRankCardFactory : ICardFactory
    {
        private static readonly IDictionary<char, int> NameValueCorrespondences = new Dictionary<char, int>
        {
            { 'A', 1 },
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
            { 'K', 13 }
        };

        public Card CreateCard(char cardName, char cardSuit)
            => new(cardName, cardSuit, NameValueCorrespondences[cardName]);
    }
}

public abstract class Rank : IRank
{
    protected Rank(IEnumerable<Card> cards)
        => Cards = cards;

    protected IEnumerable<Card> Cards { get; }

    public HandValue GetHandValue()
    {
        if (IsMatch() == false)
            return HandValue.NoValue;

        return new HandValue(
            GetOrder(),
            GetValue(),
            GetKickersValue());
    }

    protected abstract int GetOrder();

    protected abstract int GetValue();

    private int GetKickersValue()
        => Kickers.GetValue(Cards);

    protected abstract bool IsMatch();

    public HandValue GetHandValue(int rankOrder)
        => throw new NotImplementedException();
}

public class RoyalFlush : StraightFlush
{
    public RoyalFlush(IEnumerable<Card> cards) : base(cards)
    {
    }

    protected override int GetOrder()
        => 10;

    protected override int GetValue()
        => Cards.Sum(c => c.Value);

    protected override bool IsMatch()
        => base.IsMatch() &&
           Cards.Max().Value == 14;
}

public class StraightFlush : Straight
{
    private const int NumberOfCardsInHand = 5;

    public StraightFlush(IEnumerable<Card> cards) : base(cards)
    {
    }

    private IEnumerable<IGrouping<char, Card>> FlushGroup
        => Cards.ThatHaveSameSuit().AndNumberOfCardsIs(NumberOfCardsInHand);

    protected override int GetOrder()
        => 9;

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

    private IEnumerable<IGrouping<char, Card>> QuadrupletGroup
        => Cards
            .ThatHaveSameName()
            .AndNumberOfCardsIs(NumberOfCardsQuadruplet);

    protected override int GetOrder()
        => 8;

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

    private IEnumerable<IGrouping<char, Card>> PairGroup
        => Cards
            .ThatHaveSameName()
            .AndNumberOfCardsIs(NumberOfCardsInOnePair);

    private IEnumerable<IGrouping<char, Card>> TripletGroup
        => Cards
            .ThatHaveSameName()
            .AndNumberOfCardsIs(NumberOfCardsTriplet);

    protected override int GetOrder()
        => 7;

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

    protected override int GetOrder()
        => 6;

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

    protected override int GetOrder()
        => 5;

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

    private IEnumerable<IGrouping<char, Card>> TripletGroup
        => Cards
            .ThatHaveSameName()
            .AndNumberOfCardsIs(NumberOfCardsTriplet);

    protected override int GetOrder()
        => 4;

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

    private IEnumerable<IGrouping<char, Card>> PairGroup
        => Cards
            .ThatHaveSameName()
            .AndNumberOfCardsIs(NumberOfCardsInOnePair);

    protected override int GetOrder()
        => 3;

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

    private IEnumerable<IGrouping<char, Card>> PairGroup
        => Cards
            .ThatHaveSameName()
            .AndNumberOfCardsIs(NumberOfCardsInOnePair);

    protected override int GetOrder()
        => 2;

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

    protected override int GetOrder()
        => 1;

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
    public static IEnumerable<IGrouping<char, Card>> ThatHaveSameName(this IEnumerable<Card> cards)
        => cards
            .GroupBy(c => c.Name);

    public static IEnumerable<IGrouping<char, Card>> ThatHaveSameSuit(this IEnumerable<Card> cards)
        => cards
            .GroupBy(c => c.Suit);

    public static IEnumerable<IGrouping<T, Card>> AndNumberOfCardsIs<T>(
        this IEnumerable<IGrouping<T, Card>> groupOfCards,
        int numberOfCards)
        => groupOfCards
            .Where(g => g.Count() == numberOfCards);
}