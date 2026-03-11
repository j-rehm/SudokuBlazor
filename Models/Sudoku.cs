using SudokuBlazor.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace SudokuBlazor.Models;

public class Sudoku : IDisposable
{
  public const int BlockWidth = 3;
  public const int BoardWidth = 9;
  public const int CellCount = 81;

  private readonly List<Cell> _cells;
  public ReadOnlySet<Cell> Cells { get; }
  
  private readonly List<CellGroup> _groups;
  public ReadOnlySet<CellGroup> Groups { get; }

  private readonly HashSet<CellInvalidContext> _problems;
  public ReadOnlySet<CellInvalidContext> Problems { get; }

  private readonly IReadOnlyList<int> _fullOptions;

  public Sudoku() : this([]) { }
  public Sudoku(int[] cells)
  {
    int[] fullOptions = new int[BoardWidth];
    for (int i = 0; i < fullOptions.Length; i++)
      fullOptions[i] = i + 1;
    _fullOptions = [.. fullOptions];

    _cells = new List<Cell>(CellCount);
    Cells = new(() => _cells);
    for (int index = 0; index < CellCount; index++)
    {
      int value = cells.Length > index ? cells[index] : 0;
      int2 coords = CoordsOf(index);
      _cells.Add(new(coords, value));
    }

    _groups = [];
    Groups = new(() => _groups);
    for (int r = 0; r < BoardWidth; r++)
    {
      CellGroup group = [];
      for (int c = 0; c < BoardWidth; c++)
        if (TryGetCell((r, c), out Cell? cell))
          group.Add(cell);
      _groups.Add(group);
    }
    for (int c = 0; c < BoardWidth; c++)
    {
      CellGroup group = [];
      for (int r = 0; r < BoardWidth; r++)
        if (TryGetCell((r, c), out Cell? cell))
          group.Add(cell);
      _groups.Add(group);
    }
    for (int ro = 0; ro < BoardWidth; ro += BlockWidth)
    {
      for (int co = 0; co < BoardWidth; co += BlockWidth)
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

    _problems = [];
    Problems = new(() => _problems);
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
    => coords.Row * BoardWidth + coords.Col;
  public bool IsIndexValid(int index)
    => IsWithin(index, CellCount);

  public int2 CoordsOf(int index)
    => (index / BoardWidth, index % BoardWidth);
  public bool AreCoordsValid(int2 coords)
    => IsWithin(coords, BoardWidth);

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

  public Cell? GetCellAt(int index)
  {
    if (!IsIndexValid(index))
      return null;
    return _cells[index];
  }
  public Cell? GetCellAt(int2 coords)
  {
    if (!AreCoordsValid(coords) || !GetIndex(coords, out int index))
      return null;
    return _cells[index];
  }

  public bool TryGetCell(int index, [NotNullWhen(true)] out Cell? cell)
  {
    cell = GetCellAt(index);
    return cell is not null;
  }
  public bool TryGetCell(int2 coords, [NotNullWhen(true)] out Cell? cell)
  {
    cell = GetCellAt(coords);
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
    {
      group.UpdateCellProblems();
      _problems.UnionWith(group.Problems);
    }
  }
  public void ResetCellProblems()
  {
    _problems.Clear();
    foreach (Cell cell in _cells)
      cell.Problems.Clear();
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
