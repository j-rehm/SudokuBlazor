using System.Collections;

namespace SudokuBlazor.Models;

public class Cell(int2 coords)
{
  public int2 Coords { get; } = coords;

  public int? Value { get; set; }
  public bool HasValue => Value.HasValue;

  private List<CellGroup> Groups { get; set; } = [];
  public void JoinGroup(CellGroup group)
    => Groups.Add(group);
  public void LeaveGroup(CellGroup group)
    => Groups.Remove(group);
  public bool IsInGroup(CellGroup group)
    => Groups.Contains(group);
  public bool SharesGroupWith(Cell cell)
    => Groups.Any(cell.IsInGroup);

  public HashSet<int> PossibleValues { get; set; } = [];

  public HashSet<CellInvalidContext> Problems { get; set; } = [];
  public bool HasProblem<T>(out List<T> problems)
    where T : CellInvalidContext
  {
    problems = [];
    foreach (CellInvalidContext problem in Problems)
      if (problem is T tProblem)
        problems.Add(tProblem);
    return problems.Count > 0;
  }
}

public class CellGroup : IEnumerable<Cell>, IDisposable
{
  private List<Cell> Cells { get; set; } = [];
  public HashSet<CellInvalidContext> Problems { get; private set; } = [];

  public void Add(Cell cell)
  {
    Cells.Add(cell);
    cell.JoinGroup(this);
  }

  public void Dispose()
  {
    foreach (Cell cell in Cells)
      cell.LeaveGroup(this);
    Cells = [];
    GC.SuppressFinalize(this);
  }

  public HashSet<int> GetTakenValues()
  {
    HashSet<int> takenValues = [];
    foreach (Cell cell in Cells)
      if (cell.Value is int value)
        takenValues.Add(value);
    return takenValues;
  }

  public void UpdatePossibleValues()
  {
    HashSet<int> takenValues = GetTakenValues();
    foreach (Cell cell in Cells)
      cell.PossibleValues.ExceptWith(takenValues);
  }

  public void UpdateProblems()
  {
    Dictionary<int, List<Cell>> duplicates = [];
    for (int i = 0; i < Cells.Count; i++)
    {
      Cell cell = Cells[i];
      for (int j = i + 1; j < Cells.Count; j++)
      {
        Cell other = Cells[j];
        if (cell.HasValue && cell.Value == other.Value)
          duplicates.AddItems(cell.Value!.Value, cell, other);
      }
    }
    
    foreach (var problem in Problems)
      if (problem is IDisposable disposable)
        disposable.Dispose();

    Problems = [];
    foreach (List<Cell> cells in duplicates.Values)
      Problems.Add(new CellDuplicateGroup(cells));
  }

  public bool AreCellsValid(out HashSet<CellInvalidContext> problems)
  {
    UpdateProblems();
    problems = Problems;
    return Problems.Count == 0;
  }

  public IEnumerator<Cell> GetEnumerator()
  {
    foreach (Cell cell in Cells)
      yield return cell;
  }
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class CellInvalidContext { }

public class CellDuplicateGroup : CellInvalidContext, IEnumerable<Cell>, IDisposable
{
  private List<Cell> Cells { get; set; }

  public CellDuplicateGroup(List<Cell> cells)
  {
    Cells = cells;
    foreach (Cell cell in Cells)
      cell.Problems.Add(this);
  }

  public void Dispose()
  {
    foreach (Cell cell in Cells)
      cell.Problems.Remove(this);
    Cells = [];
    GC.SuppressFinalize(this);
  }

  public IEnumerator<Cell> GetEnumerator()
  {
    foreach (Cell cell in Cells)
      yield return cell;
  }
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class CellNoPossibilities : CellInvalidContext, IDisposable
{
  public Cell? Cell { get; private set; }

  public CellNoPossibilities(Cell cell)
  {
    Cell = cell;
    Cell.Problems.Add(this);
  }

  public void Dispose()
  {
    Cell?.Problems.Remove(this);
    Cell = null;
    GC.SuppressFinalize(this);
  }
}