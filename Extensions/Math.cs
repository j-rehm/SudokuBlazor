namespace SudokuBlazor.Extensions;

public class Math
{
  public static int Mod(int a, int n)
  {
    int r = a % n;
    return r < 0 ? r + n : r;
  }
}
