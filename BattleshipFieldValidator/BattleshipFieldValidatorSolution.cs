using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.BattleshipFieldValidator;

public class SolutionTest
{
    private static int[,] Field
    {
        get
        {
            var field = new[,]
            {
                { 1, 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 1, 0, 1, 0, 0, 0, 0, 0, 1, 0 },
                { 1, 0, 1, 0, 1, 1, 1, 0, 1, 0 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            return field;
        }
    }

    [Fact]
    public void TestCase()
    {
        var field = Field;
        BattleshipField.ValidateBattlefield(field).Should().BeTrue();
    }

    [Fact]
    public void FindBoats()
    {
        var field = Field;

        var boats = Boat.FindBoats(field);

        boats.Should().HaveCount(10);
    }
}

public static class BattleshipField
{
    private const int BattleshipCount = 1;
    private const int CruiserCount = 2;
    private const int DestroyerCount = 3;
    private const int SubmarineCount = 4;

    private const int BoatTotalCount = BattleshipCount + CruiserCount + DestroyerCount + SubmarineCount;

    public static bool ValidateBattlefield(int[,] field)
    {
        var boats = Boat.FindBoats(field);

        return
            boats.OfType<Battleship>().Count() == BattleshipCount &&
            boats.OfType<Cruiser>().Count() == CruiserCount &&
            boats.OfType<Destroyer>().Count() == DestroyerCount &&
            boats.OfType<Submarine>().Count() == SubmarineCount &&
            boats.Count() == BoatTotalCount &&
            Boat.NoBoatIsInContact(boats);
    }
}

public record Boat
{
    protected Boat(IEnumerable<Coordinate> coordinates)
        => Coordinates = coordinates.ToImmutableList();

    public ImmutableList<Coordinate> Coordinates { get; }

    public static implicit operator List<Coordinate>(Boat boat)
        => boat.Coordinates.ToList();

    public static Boat Create(List<Coordinate> coordinates)
        => coordinates.Count switch
        {
            Submarine.Size => new Submarine(coordinates),
            Destroyer.Size => new Destroyer(coordinates),
            Cruiser.Size => new Cruiser(coordinates),
            Battleship.Size => new Battleship(coordinates),
            _ => new Boat(coordinates)
        };

    public static List<Boat> FindBoats(int[,] field)
        => Grid.FindBoats(field);

    private bool IsInContact(Boat other)
    {
        var adjacentCoordinates = from firstCoordinate in Coordinates
                                  from secondCoordinate in other.Coordinates
                                  where firstCoordinate.IsAdjacent(secondCoordinate)
                                  select new { firstCoordinate, secondCoordinate };

        return adjacentCoordinates.Any();
    }

    public static bool NoBoatIsInContact(IReadOnlyCollection<Boat> boats)
    {
        var boatsInContact = from firstBoat in boats
                             from secondBoat in boats
                             where firstBoat != secondBoat
                             where firstBoat.IsInContact(secondBoat)
                             select new { firstBoat, secondBoat };

        var noBoatIsInContact = !boatsInContact.Any();

        return noBoatIsInContact;
    }
}

internal static class Grid
{
    private const int GridSize = 10;
    private const int BoatExistence = 1;

    internal static List<Boat> FindBoats(int[,] field)
    {
        var boats = new List<Boat>();

        var coordinatesGrid = GetBoatsCoordinatesGrid(field);

        var horizontalBoats = FindBoats(x => coordinatesGrid.GetRow(x));
        boats.AddRange(horizontalBoats.Except(horizontalBoats.OfType<Submarine>()));

        var verticalBoats = FindBoats(y => coordinatesGrid.GetColumn(y));
        boats.AddRange(verticalBoats.Except(verticalBoats.OfType<Submarine>()));

        RemoveBoatFromGrid(boats, coordinatesGrid);

        var submarines = FindBoats(x => coordinatesGrid.GetRow(x));
        boats.AddRange(submarines);

        return boats;
    }


    private static Coordinate?[,] GetBoatsCoordinatesGrid(int[,] field)
    {
        var grid = new Coordinate?[GridSize, GridSize];

        for (var x = 0; x < GridSize; x++)
        for (var y = 0; y < GridSize; y++)
            if (field[x, y] == BoatExistence)
                grid[x, y] = new Coordinate(x, y);

        return grid;
    }

    private static List<Boat> FindBoats(Func<int, Coordinate?[]> getCoordinateLine)
    {
        var foundBoats = new List<Boat>();
        for (var i = 0; i < GridSize; i++)
        {
            var coordinatesLine = getCoordinateLine(i);

            var groupedCoordinates = Coordinate.GroupAdjacentCoordinates(coordinatesLine);

            var boats = groupedCoordinates
                .Where(group => group.Count >= 1)
                .Select(Boat.Create);

            foundBoats.AddRange(boats);
        }

        return foundBoats;
    }

    private static void RemoveBoatFromGrid(List<Boat> boats, Coordinate?[,] boatCoordinates)
    {
        foreach (var coordinate in boats.SelectMany(boat => boat.Coordinates))
            boatCoordinates[coordinate.X, coordinate.Y] = null;
    }
}

internal record Battleship : Boat
{
    public const int Size = 4;

    internal Battleship(List<Coordinate> coordinates) : base(coordinates)
    {
    }
}

internal record Cruiser : Boat
{
    public const int Size = 3;

    internal Cruiser(List<Coordinate> coordinates) : base(coordinates)
    {
    }
}

internal record Destroyer : Boat
{
    public const int Size = 2;

    internal Destroyer(List<Coordinate> coordinates) : base(coordinates)
    {
    }
}

internal record Submarine : Boat
{
    public const int Size = 1;

    internal Submarine(List<Coordinate> coordinates) : base(coordinates)
    {
    }
}

public record Coordinate(int X, int Y)
{
    public bool IsAdjacent(Coordinate otherCoordinate)
        => IsNorthCoordinate(otherCoordinate) ||
           IsNorthEastCoordinate(otherCoordinate) ||
           IsEastCoordinate(otherCoordinate) ||
           IsSouthEastCoordinate(otherCoordinate) ||
           IsSouthCoordinate(otherCoordinate) ||
           IsSouthWestCoordinate(otherCoordinate) ||
           IsWestCoordinate(otherCoordinate) ||
           IsNorthWestCoordinate(otherCoordinate);

    private bool IsNorthEastCoordinate(Coordinate otherCoordinate)
        => X - 1 == otherCoordinate.X && Y + 1 == otherCoordinate.Y;

    private bool IsSouthWestCoordinate(Coordinate otherCoordinate)
        => X + 1 == otherCoordinate.X && Y - 1 == otherCoordinate.Y;

    private bool IsNorthWestCoordinate(Coordinate otherCoordinate)
        => X - 1 == otherCoordinate.X && Y - 1 == otherCoordinate.Y;

    private bool IsSouthEastCoordinate(Coordinate otherCoordinate)
        => X + 1 == otherCoordinate.X && Y + 1 == otherCoordinate.Y;

    private bool IsWestCoordinate(Coordinate otherCoordinate)
        => X == otherCoordinate.X && Y - 1 == otherCoordinate.Y;

    private bool IsEastCoordinate(Coordinate otherCoordinate)
        => X == otherCoordinate.X && Y + 1 == otherCoordinate.Y;

    private bool IsNorthCoordinate(Coordinate otherCoordinate)
        => X - 1 == otherCoordinate.X && Y == otherCoordinate.Y;

    private bool IsSouthCoordinate(Coordinate otherCoordinate)
        => X + 1 == otherCoordinate.X && Y == otherCoordinate.Y;

    public static List<List<Coordinate>> GroupAdjacentCoordinates(IEnumerable<Coordinate?> coordinates)
        => coordinates.Aggregate(
            new List<List<Coordinate>> { new() },
            (groupedCoordinates, coordinate) =>
            {
                if (coordinate is not null)
                    groupedCoordinates.Last().Add(coordinate);
                else
                    groupedCoordinates.Add(new List<Coordinate>());

                return groupedCoordinates;
            });
}

public static class ArrayExtensions
{
    public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
        => Enumerable.Range(0, matrix.GetLength(0))
            .Select(x => matrix[x, columnNumber])
            .ToArray();

    public static T[] GetRow<T>(this T[,] matrix, int rowNumber)
        => Enumerable.Range(0, matrix.GetLength(1))
            .Select(y => matrix[rowNumber, y])
            .ToArray();
}