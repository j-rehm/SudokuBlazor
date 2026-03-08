using SudokuBlazor.Extensions;
using System.Collections;

namespace SudokuBlazor.Models;

public class Cell(int2 coords)
{
  public int2 Coords { get; } = coords;

  private int? _value = null;
  public int Value
  {
    get => _value!.Value;
    set => _value = value;
  }
  public bool HasValue => _value.HasValue;
  public void RemoveValue() => _value = null;

  public List<CellGroup> Groups { get; } = [];
  public void JoinGroup(CellGroup group)
    => Groups.Add(group);
  public void LeaveGroup(CellGroup group)
    => Groups.Remove(group);
  public bool IsInGroup(CellGroup group)
    => Groups.Contains(group);
  public bool SharesGroupWith(Cell cell)
    => Groups.Any(cell.IsInGroup);

  public HashSet<int> Options { get; } = [];
  public bool HasNoOptions => HasValue == false && Options.Count == 0;
  public bool HasOptions => HasValue == false && Options.Count > 0;
  public bool HasSingleOption => HasValue == false && Options.Count == 1;
  public bool CouldBe(int value) => Options.Contains(value);

  public HashSet<CellInvalidContext> Problems { get; } = [];
  public bool HasProblem<T>(out List<T> problems)
    where T : CellInvalidContext
  {
    problems = [];
    foreach (CellInvalidContext problem in Problems)
      if (problem is T tProblem)
        problems.Add(tProblem);
    return problems.Count > 0;
  }
  public bool IsDuplicate => HasProblem<CellDuplicateGroup>(out _);
}

public class CellGroup : IEnumerable<Cell>, IDisposable
{
  private List<Cell> Cells { get; } = [];
  public HashSet<CellInvalidContext> Problems { get; } = [];

  public void Add(Cell cell)
  {
    Cells.Add(cell);
    cell.JoinGroup(this);
  }

  public void Dispose()
  {
    foreach (Cell cell in Cells)
      cell.LeaveGroup(this);
    Cells.Clear();
    GC.SuppressFinalize(this);
  }

  public HashSet<int> GetTakenValues()
  {
    HashSet<int> takenValues = [];
    foreach (Cell cell in Cells)
      if (cell.HasValue)
        takenValues.Add(cell.Value);
    return takenValues;
  }

  public void UpdateCellOptions()
  {
    HashSet<int> takenValues = GetTakenValues();
    foreach (Cell cell in Cells)
      if (cell.HasValue)
        cell.Options.Clear();
      else
        cell.Options.ExceptWith(takenValues);
  }

  public void SetValueTaken(int value)
    => SetValuesTaken(value);
  public void SetValuesTaken(params IEnumerable<int> values)
  {
    foreach (Cell cell in Cells)
      cell.Options.ExceptWith(values);
  }

  public void UpdateCellProblems()
  {
    Dictionary<int, List<Cell>> duplicates = [];
    for (int i = 0; i < Cells.Count; i++)
    {
      Cell cell = Cells[i];
      if (!cell.HasValue)
        continue;

      for (int j = i + 1; j < Cells.Count; j++)
      {
        Cell other = Cells[j];
        if (!other.HasValue)
          continue;

        if (cell.Value == other.Value)
          duplicates.AddItems(cell.Value, cell, other);
      }
    }
    
    foreach (var problem in Problems)
      if (problem is IDisposable disposable)
        disposable.Dispose();

    Problems.Clear();
    foreach (List<Cell> cells in duplicates.Values)
      Problems.Add(new CellDuplicateGroup(cells));
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
  private List<Cell> Cells { get; }

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
    Cells.Clear();
    GC.SuppressFinalize(this);
  }

  public IEnumerator<Cell> GetEnumerator()
  {
    foreach (Cell cell in Cells)
      yield return cell;
  }
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class CellNoOptions : CellInvalidContext, IDisposable
{
  public Cell? Cell { get; private set; }

  public CellNoOptions(Cell cell)
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