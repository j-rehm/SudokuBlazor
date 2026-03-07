using System.Diagnostics.CodeAnalysis;

namespace SudokuBlazor.Models;

public class Sudoku : IDisposable
{
  public int BlockWidth { get; }
  public int Width { get; }
  public int CellCount { get; }

  private readonly List<Cell> _cells;
  private readonly List<CellGroup> _groups;

  private readonly IReadOnlyList<int> _fullOptions;

  public Sudoku(int width = 9)
  {
    double widthSquareRoot = Math.Sqrt(width);
    int blockWidth = (int)Math.Floor(widthSquareRoot);
    if (widthSquareRoot > blockWidth) // Sqrt has a remainder. No good.
      throw new Exception("Sudoku width must be a perfect square.");

    BlockWidth = blockWidth;
    Width = width;
    CellCount = width * width;

    int[] fullOptions = new int[Width];
    for (int i = 0; i < fullOptions.Length; i++)
      fullOptions[i] = i + 1;
    _fullOptions = [.. fullOptions];

    _cells = new List<Cell>(CellCount);
    for (int index = 0; index < CellCount; index++)
    {
      int2 coords = CoordsOf(index);
      _cells.Add(new(coords));
    }

    _groups = [];
    for (int r = 0; r < Width; r++)
    {
      CellGroup group = [];
      for (int c = 0; c < Width; c++)
        if (TryGetCell((r, c), out Cell? cell))
          group.Add(cell);
      _groups.Add(group);
    }
    for (int c = 0; c < Width; c++)
    {
      CellGroup group = [];
      for (int r = 0; r < Width; r++)
        if (TryGetCell((r, c), out Cell? cell))
          group.Add(cell);
      _groups.Add(group);
    }
    for (int ro = 0; ro < Width; ro += BlockWidth)
    {
      for (int co = 0; co < Width; co += BlockWidth)
      {
        CellGroup group = [];
        for (int r = 0; r < BlockWidth; r++)
          for (int c = 0; c < BlockWidth; c++)
          {
            int2 coords = (r + ro, c + co);
            if (TryGetCell(coords, out Cell? cell))
              group.Add(cell);
          }
        _groups.Add(group);
      }
    }
  }

  public void Dispose()
  {
    foreach (CellGroup group in _groups)
      group.Dispose();
    _groups.Clear();
    _cells.Clear();
    GC.SuppressFinalize(this);
  }

  private static bool IsWithin(int value, int bound)
    => value >= 0 && value < bound;
  private static bool IsWithin(int2 values, int bound)
    => IsWithin(values.X, bound) && IsWithin(values.Y, bound);

  public int IndexOf(int2 coords)
    => coords.Row * Width + coords.Col;
  public bool IsIndexValid(int index)
    => IsWithin(index, CellCount);

  public int2 CoordsOf(int index)
    => (index / Width, index % Width);
  public bool AreCoordsValid(int2 coords)
    => IsWithin(coords, Width);

  public bool GetIndex(int2 coords, out int index)
  {
    index = IndexOf(coords);
    return IsIndexValid(index);
  }
  public bool GetCoords(int index, out int2 coords)
  {
    coords = CoordsOf(index);
    return AreCoordsValid(coords);
  }

  public Cell? CellAt(int index)
  {
    if (!IsIndexValid(index))
      return null;
    return _cells[index];
  }
  public Cell? CellAt(int2 coords)
  {
    if (!GetIndex(coords, out int index))
      return null;
    return _cells[index];
  }

  public bool TryGetCell(int index, [NotNullWhen(true)] out Cell? cell)
  {
    cell = CellAt(index);
    return cell is not null;
  }
  public bool TryGetCell(int2 coords, [NotNullWhen(true)] out Cell? cell)
  {
    cell = CellAt(coords);
    return cell is not null;
  }

  public void CalculateCellOptions()
  {
    foreach (Cell cell in _cells)
      if (!cell.HasValue)
        cell.Options.UnionWith(_fullOptions);
    foreach (CellGroup group in _groups)
      group.UpdateCellOptions();
  }
  public void ResetCellOptions()
  {
    foreach (Cell cell in _cells)
      cell.Options.Clear();
  }

  public void CalculateCellProblems()
  {
    ResetCellProblems();
    foreach (CellGroup group in _groups)
      group.UpdateCellProblems();
  }
  public void ResetCellProblems()
  {
    foreach (Cell cell in _cells)
      cell.Problems.Clear();
  }

  public void SetRandomValues(double threshold = 0.2)
  {
    Random rng = new();
    foreach (Cell cell in _cells)
    {
      double chance = rng.NextDouble();
      if (chance > threshold)
      {
        cell.RemoveValue();
        continue;
      }

      int value = rng.Next(Width) + 1;
      cell.Value = value;
    }
  }

  public void Clear()
  {
    foreach (Cell cell in _cells)
    {
      cell.RemoveValue();
      cell.Options.Clear();
      foreach (var problem in cell.Problems)
        if (problem is IDisposable disposable)
          disposable.Dispose();
      cell.Problems.Clear();
    }
  }
}
