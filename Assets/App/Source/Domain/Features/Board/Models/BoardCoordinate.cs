using System;

namespace App.Domain.Board
{
    public readonly struct BoardCoordinate : IEquatable<BoardCoordinate>
    {
        public int X { get; }
        public int Y { get; }

        public BoardCoordinate(int x, int y) => (X, Y) = (x, y);

        public bool Equals(BoardCoordinate other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is BoardCoordinate other && Equals(other);

        public override int GetHashCode() => unchecked((X * 397) ^ Y);

        public static bool operator ==(BoardCoordinate left, BoardCoordinate right) => left.Equals(right);

        public static bool operator !=(BoardCoordinate left, BoardCoordinate right) => !left.Equals(right);

        public static BoardCoordinate operator +(BoardCoordinate a, BoardCoordinate b) => new(a.X + b.X, a.Y + b.Y);

        public override string ToString() => $"({X}, {Y})";
    }
}