using System.Diagnostics.CodeAnalysis;

namespace SudokuBlazor.Models;

public class Sudoku : IDisposable
{
  public int Root { get; }
  public int Magnitude { get; }
  public int Square { get; }

  private Cell[] Cells { get; set; }
  private List<CellGroup> Groups { get; set; }

  public Sudoku(int magnitude = 9)
  {
    double sqrt = Math.Sqrt(magnitude);
    int root = (int)Math.Floor(sqrt);
    if (sqrt > root)
      throw new Exception("Magnitude must be a perfect square.");

    Root = root;
    Magnitude = magnitude;
    Square = magnitude * magnitude;

    Cells = new Cell[Square];
    for (int index = 0; index < Square; index++)
    {
      GetCoords(index, out int2 coords);
      Cells[index] = new(coords);
    }

    Groups = [];
    for (int r = 0; r < Magnitude; r++)
    {
      CellGroup group = [];
      for (int c = 0; c < Magnitude; c++)
        if (GetCell(r, c, out Cell? cell))
          group.Add(cell);
      Groups.Add(group);
    }
    for (int c = 0; c < Magnitude; c++)
    {
      CellGroup group = [];
      for (int r = 0; r < Magnitude; r++)
        if (GetCell(r, c, out Cell? cell))
          group.Add(cell);
      Groups.Add(group);
    }
    for (int ro = 0; ro < Magnitude; ro += Root)
    {
      for (int co = 0; co < Magnitude; co += Root)
      {
        CellGroup group = [];
        for (int r = 0; r < Root; r++)
          for (int c = 0; c < Root; c++)
          {
            int2 coords = (r + ro, c + co);
            if (GetCell(coords, out Cell? cell))
              group.Add(cell);
          }
        Groups.Add(group);
      }
    }
  }

  public bool GetCoords(int index, out int2 coords)
  {
    coords = (index / Magnitude, index % Magnitude);
    return index >= 0 && index < Square;
  }
  public bool GetIndex(int2 coords, out int index)
  {
    index = coords.Row * Magnitude + coords.Col;
    return index >= 0 && index < Square;
  }

  public bool GetCell(int2 coords, [NotNullWhen(true)] out Cell? cell)
  {
    if (!GetIndex(coords, out int index))
    {
      cell = null;
      return false;
    }
    cell = Cells[index];
    return true;
  }
  public bool GetCell(int row, int col, [NotNullWhen(true)] out Cell? cell)
    => GetCell((row, col), out cell);

  public bool SetValue(int2 coord, int? value)
  {
    if (!GetCell(coord, out Cell? cell))
      return false;
    cell.Value = value;
    return true;
  }

  private HashSet<int> GetAllPossibleValues()
  {
    HashSet<int> possibleValues = [];
    for (int value = 1; value <= Magnitude; value++)
      possibleValues.Add(value);
    return possibleValues;
  }

  public void UpdatePossibleValues()
  {
    foreach (Cell cell in Cells)
      if (!cell.HasValue)
        cell.PossibleValues = GetAllPossibleValues();
    foreach (CellGroup group in Groups)
      group.UpdatePossibleValues();
  }

  public void UpdateProblems()
  {
    foreach (CellGroup group in Groups)
      group.UpdateProblems();
  }

  public void Update()
  {
    UpdatePossibleValues();
    UpdateProblems();
  }

  public void Random(double threshold = 0.25)
  {
    Random rng = new();
    foreach (Cell cell in Cells)
      cell.Value = rng.NextDouble() > threshold ? null : rng.Next(Magnitude) + 1;
    Update();
  }

  public void Clear()
  {
    foreach (Cell cell in Cells)
    {
      cell.Value = null;
      cell.PossibleValues = [];
      foreach (var problem in cell.Problems)
        if (problem is IDisposable disposable)
          disposable.Dispose();
      cell.Problems = [];
    }
  }

  public void Dispose()
  {
    foreach (CellGroup group in Groups)
      group.Dispose();
    Groups = [];
    Cells = [];
    GC.SuppressFinalize(this);
  }
}
