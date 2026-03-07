using Microsoft.AspNetCore.Components.Web;
using SudokuBlazor.Enums;
using SudokuBlazor.Models;

namespace SudokuBlazor.Components.Pages;

public partial class SudokuPage
{
  private Sudoku Sudoku { get; } = new();

  public Cell? SelectedCell { get; set; }

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
    coords = (Extensions.Mod(coords.Row, Sudoku.Width), Extensions.Mod(coords.Col, Sudoku.Width));
    SelectedCell = Sudoku.GetCellAt(coords);
  }

  private void SetCellValue(int? value)
  {
    if (SelectedCell is null)
      return;
    if (value is null)
      SelectedCell.RemoveValue();
    else
      SelectedCell.Value = value.Value;
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