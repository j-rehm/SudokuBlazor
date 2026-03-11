using SudokuBlazor.Extensions;
using System.Collections;

namespace SudokuBlazor.Models;

public class Cell(int2 coords, int value = 0)
{
  //public Cell(int2 coords, int index)
  //{
  //  Coords = coords;
  //  _cell = 0;
  //  _cell |= index << 17;
  //  _cell |= 0 << 13;
  //  _cell |= 0b100100101 << 4;

  //  Console.WriteLine($"9: {GetFlag(9, 4)}");
  //  Console.WriteLine($"8: {GetFlag(8, 4)}");
  //  Console.WriteLine($"7: {GetFlag(7, 4)}");
  //  Console.WriteLine($"6: {GetFlag(6, 4)}");
  //  Console.WriteLine($"5: {GetFlag(5, 4)}");
  //  Console.WriteLine($"4: {GetFlag(4, 4)}");
  //  Console.WriteLine($"3: {GetFlag(3, 4)}");
  //  Console.WriteLine($"2: {GetFlag(2, 4)}");
  //  Console.WriteLine($"1: {GetFlag(1, 4)}");
  //}

  //private int _cell;

  //private bool GetFlag(int bit, int offset)
  //{
  //  int mask = 1 << bit + offset - 1;
  //  return (_cell >> offset & mask) == mask;
  //}

  //private void EnableFlag(int bit, int offset)
  //{
  //  int mask = 1 << bit + offset - 1;
  //  _cell |= mask;
  //}



  public int2 Coords { get; } = coords;

  public bool IsGiven { get; } = value > 0;

  public int Value
  {
    get => field;
    set
    {
      if (IsGiven)
        return;
      field = value;
    }
  } = value;

  public bool HasValue => Value > 0;
  public void RemoveValue() => Value = 0;

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

  public int CorrectOption { get; set; } = 0;

  public bool GetCorrectOption(out int option)
  {
    if (CorrectOption > 0)
    {
      option = CorrectOption;
      return true;
    }
    if (HasSingleOption)
    {
      option = Options.Single();
      return true;
    }
    option = 0;
    return false;
  }

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