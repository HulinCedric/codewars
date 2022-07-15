using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Codewars.MakeASpiral;

public class SolutionTest
{
    [Fact]
    public void Test05()
    {
        const int input = 5;
        int[,] expected =
        {
            { 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 1 },
            { 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1 }
        };

        var actual = Spiralizor.Spiralize(input);
        actual.Should().BeEquivalentTo(expected);
    }


    [Fact]
    public void Test10()
    {
        const int input = 10;
        int[,] expected =
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 0, 0, 0, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        var actual = Spiralizor.Spiralize(input);
        actual.Should().BeEquivalentTo(expected);
    }
}

public static class Spiralizor
{
    public static int[,] Spiralize(int size)
    {
        var grid = new Grid(size);
        var snake = new Snake(grid);

        while (snake.RotationCount < 2)
            if (snake.CanMove())
                snake.Move();
            else
                snake.Rotate();

        return snake.GetRepresentation();
    }
}

internal class Snake
{
    private readonly List<Coordinate> body;
    private readonly Grid grid;
    private SnakeHead head;

    internal Snake(Grid grid)
    {
        this.grid = grid;

        head = new SnakeHead();

        body = new List<Coordinate> { head.Coordinate };
    }

    internal int RotationCount { get; private set; }

    internal void Move()
    {
        head = head.Move();

        body.Add(head.Coordinate);

        RotationCount = 0;
    }

    internal bool CanMove()
    {
        var nextHeadPosition = head.Move();

        if (!grid.Contains(nextHeadPosition.Coordinate))
            return false;

        if (body.Contains(nextHeadPosition.Coordinate))
            return false;

        if (HaveAdjacentBody(nextHeadPosition))
            return false;

        return true;
    }

    private bool HaveAdjacentBody(SnakeHead headPosition)
    {
        var face = headPosition.Move().Coordinate;
        var right = headPosition.RotateRight().Move().Coordinate;
        var left = headPosition.RotateLeft().Move().Coordinate;

        return body.Contains(face) ||
               body.Contains(right) ||
               body.Contains(left);
    }

    internal void Rotate()
    {
        head = head.Rotate();

        RotationCount++;
    }

    internal int[,] GetRepresentation()
    {
        var gridRepresentation = grid.GetRepresentation();

        foreach (var bodyPart in body)
            gridRepresentation[bodyPart.X, bodyPart.Y] = 1;

        return gridRepresentation;
    }

    private record SnakeHead(Coordinate Coordinate, Direction Direction)
    {
        internal SnakeHead() : this(new Coordinate(0, 0), new Direction(0, 1))
        {
        }

        internal SnakeHead Move()
            => this with { Coordinate = Coordinate.NextIn(Direction) };

        internal SnakeHead Rotate()
            => RotateRight();

        internal SnakeHead RotateRight()
            => this with { Direction = Direction.RotateRight() };

        internal SnakeHead RotateLeft()
            => this with { Direction = Direction.RotateLeft() };
    }
}

internal record Direction(int X, int Y)
{
    internal Direction RotateRight()
        => new(Y, -X);

    internal Direction RotateLeft()
        => new(-Y, X);
}

internal record Coordinate(int X, int Y)
{
    internal Coordinate NextIn(Direction direction)
        => new(X + direction.X, Y + direction.Y);
}

internal class Grid
{
    private readonly int size;

    internal Grid(int size)
        => this.size = size;

    internal bool Contains(Coordinate coordinate)
        => coordinate.X >= 0 && coordinate.X < size && coordinate.Y >= 0 && coordinate.Y < size;

    internal int[,] GetRepresentation()
        => new int[size, size];
}