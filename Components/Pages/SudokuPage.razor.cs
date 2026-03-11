using Microsoft.AspNetCore.Components.Web;
using SudokuBlazor.Enums;
using SudokuBlazor.Models;
using System.Diagnostics.CodeAnalysis;

namespace SudokuBlazor.Components.Pages;

public partial class SudokuPage
{
  private readonly int[] _empty = [];
  private readonly int[] _steppy =
  [
    0,2,0, 0,0,5, 0,9,8,
    0,8,0, 0,0,0, 0,0,7,
    6,7,0, 0,1,8, 0,0,3,

    8,0,0, 3,9,0, 0,5,0,
    0,6,0, 0,0,1, 7,0,0,
    3,1,0, 0,4,6, 0,8,0,

    0,5,0, 4,7,0, 3,6,1,
    0,0,1, 0,6,0, 0,7,5,
    0,0,6, 1,0,0, 0,0,2,
  ];

  private Sudoku Sudoku { get; set; }

  public Cell? SelectedCell { get; set; }

  public SudokuPage()
  {
    Load(_steppy);
  }

  [MemberNotNull(nameof(Sudoku))]
  private void Load(int[] cells)
    => Sudoku = new(cells);

  private void Random()
  {
    int[] cells = new int[Sudoku.CellCount];
    for (int i = 0; i < cells.Length; i++)
    {
      Random rng = new();
      double chance = rng.NextDouble();
      cells[i] = chance > 0.25 ? 0 : rng.Next(Sudoku.BoardWidth) + 1;
    }
    Load(cells);
  }

  private void SelectCell(Cell? cell)
    => SelectedCell = cell;
  private bool TrySelectCellAt(int2 coords)
  {
    if (!Sudoku.TryGetCell(coords, out Cell? cell))
      return false;
    SelectedCell = cell;
    return true;
  }
  private void SelectCellAt(int2 coords)
  {
    Sudoku.TryGetCell(coords, out Cell? cell);
    SelectedCell = cell;
  }

  private static int2 GetOffset(Direction direction)
    => direction switch
    {
      Direction.Up => (-1, 0),
      Direction.Down => (1, 0),
      Direction.Left => (0, -1),
      Direction.Right => (0, 1),
      _ => (0, 0)
    };
  private static Direction GetWrapDirection(Direction direction)
    => direction switch
    {
      Direction.Up => Direction.Left,
      Direction.Down => Direction.Right,
      Direction.Left => Direction.Up,
      Direction.Right => Direction.Down,
      _ => direction
    };
  private void TryMoveSelection(Direction direction)
  {
    if (SelectedCell is null)
      return;

    int2 offset = GetOffset(direction);
    int2 coords = SelectedCell.Coords + offset;
    if (Sudoku.TryGetCell(coords, out Cell? cell))
    {
      SelectedCell = cell;
      return;
    }

    if (!Session.Settings.WrapMovement)
      return;

    direction = GetWrapDirection(direction);
    offset = GetOffset(direction);
    coords += offset;
    coords = (Extensions.Math.Mod(coords.Row, Sudoku.BoardWidth), Extensions.Math.Mod(coords.Col, Sudoku.BoardWidth));
    SelectedCell = Sudoku.GetCellAt(coords);
  }

  private void SetCellValue(int? value)
  {
    if (SelectedCell is null)
      return;
    if (Session.Settings.InputMode == InputMode.Value)
    {
      if (value is null)
        SelectedCell.RemoveValue();
      else
      {
        SelectedCell.Value = value.Value;
        foreach (CellGroup group in SelectedCell.Groups)
          group.SetValueTaken(value.Value);
      }
    }
    else if (Session.Settings.InputMode == InputMode.Option)
    {
      if (value is null)
        return;
      if (SelectedCell.Options.Contains(value.Value))
        SelectedCell.Options.Remove(value.Value);
      else
        SelectedCell.Options.Add(value.Value);
    }
  }

  private void CalculateState()
  {
    Sudoku.CalculateCellOptions();
    Sudoku.CalculateCellProblems();
  }
  private void CleanState()
  {
    Sudoku.ResetCellOptions();
    Sudoku.ResetCellProblems();
  }

  private void CellClicked(Cell? cell)
    => SelectCell(cell);

  private void ValueClicked(int? value)
    => SetCellValue(value);

  private void Step()
  {
    Solver solver = new(Sudoku);
    solver.Step();
    CalculateState();
  }

  private void SolveStepwise()
  {
    Solver solver = new(Sudoku);
    solver.SolveStepwise();
  }

  private void OnKeyDown(KeyboardEventArgs e)
  {
    (e.Key switch
    {
      "ArrowUp" => () => TryMoveSelection(Direction.Up),
      "ArrowDown" => () => TryMoveSelection(Direction.Down),
      "ArrowLeft" => () => TryMoveSelection(Direction.Left),
      "ArrowRight" => () => TryMoveSelection(Direction.Right),
      "Tab" => () => TryMoveSelection(e.ShiftKey ? Direction.Left : Direction.Right),
      "Enter" => () => TryMoveSelection(e.ShiftKey ? Direction.Up : Direction.Down),
      "Escape" => () => SelectCell(null),

      "0" or "Delete" => () => SetCellValue(null),
      "1" => () => SetCellValue(1),
      "2" => () => SetCellValue(2),
      "3" => () => SetCellValue(3),
      "4" => () => SetCellValue(4),
      "5" => () => SetCellValue(5),
      "6" => () => SetCellValue(6),
      "7" => () => SetCellValue(7),
      "8" => () => SetCellValue(8),
      "9" => () => SetCellValue(9),
      _ => (Action)(() => { })
    })();
  }
}