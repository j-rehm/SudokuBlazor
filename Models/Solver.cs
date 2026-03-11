namespace SudokuBlazor.Models;

public class Solver(Sudoku sudoku)
{
  private readonly Sudoku _sudoku = sudoku;

  public bool Step()
  {
    Dictionary<Cell, int> cellCorrectOptions = [];
    foreach (Cell cell in _sudoku.Cells)
    {
      if (cell.HasValue)
        continue;
      if (cell.GetCorrectOption(out int option))
        cellCorrectOptions.Add(cell, option);
    }
    if (cellCorrectOptions.Count == 0)
      return false;
    foreach ((Cell cell, int correctOption) in cellCorrectOptions)
      cell.Value = correctOption;
    return true;
  }

  public bool SolveStepwise()
  {
    do
    {
      _sudoku.CalculateCellProblems();
      if (_sudoku.Problems.Any())
        return false;
      _sudoku.CalculateCellOptions();
    } while (Step());
    foreach (Cell cell in _sudoku.Cells)
      if (!cell.HasValue)
        return false;
    return true;
  }
}
