#pragma warning disable IDE1006

using System.Diagnostics.CodeAnalysis;

namespace SudokuBlazor.Models;

public readonly struct int2(int x, int y)
{
  public int X { get; } = x;
  public int Y { get; } = y;

  public int Row => X;
  public int Col => Y;

  public static implicit operator (int X, int Y)(int2 i) => (i.X, i.Y);
  public static implicit operator int2((int X, int Y) int2) => new(int2.X, int2.Y);

  public static int2 operator +(int2 operand) => operand;
  public static int2 operator -(int2 operand) => (-operand.X, -operand.Y);

  public static int2 operator +(int2 left, int2 right) => (left.X + right.X, left.Y + right.Y);
  public static int2 operator -(int2 left, int2 right) => (left.X - right.X, left.Y - right.Y);
  public static int2 operator *(int2 left, int2 right) => (left.X * right.X, left.Y * right.Y);
  public static int2 operator /(int2 left, int2 right) => (left.X / right.X, left.Y / right.Y);

  public override int GetHashCode() => HashCode.Combine(X, Y);

  public override bool Equals([NotNullWhen(true)] object? obj)
    => obj is int2 pair && GetHashCode() == pair.GetHashCode();

  public static bool operator ==(int2 left, int2 right) => left.Equals(right);
  public static bool operator !=(int2 left, int2 right) => !(left == right);
}