namespace SudokuBlazor.Models;

public readonly struct int2(int x, int y)
{
  public int X { get; } = x;
  public int Y { get; } = y;

  public int Row => X;
  public int Col => Y;

  public static readonly int2 Up    = (-1,  0);
  public static readonly int2 Down  = ( 1,  0);
  public static readonly int2 Left  = ( 0, -1);
  public static readonly int2 Right = ( 0,  1);
  
  //public Int2 Up    => (X,Y) + Int2.Up;
  //public Int2 Down  => (X,Y) + Int2.Down;
  //public Int2 Left  => (X,Y) + Int2.Left;
  //public Int2 Right => (X,Y) + Int2.Right;

  public static implicit operator (int X, int Y)(int2 i) => (i.X, i.Y);
  public static implicit operator int2((int X, int Y) int2) => new(int2.X, int2.Y);

  public static int2 operator +(int2 operand) => operand;
  public static int2 operator -(int2 operand) => (-operand.X, -operand.Y);

  public static int2 operator +(int2 left, int2 right) => (left.X + right.X, left.Y + right.Y);
  public static int2 operator -(int2 left, int2 right) => (left.X - right.X, left.Y - right.Y);
  public static int2 operator *(int2 left, int2 right) => (left.X * right.X, left.Y * right.Y);
  public static int2 operator /(int2 left, int2 right) => (left.X / right.X, left.Y / right.Y);

  public static bool operator ==(int2 left, int2 right) => left.X == right.X && left.Y == right.Y;
  public static bool operator !=(int2 left, int2 right) => !(left == right);
}