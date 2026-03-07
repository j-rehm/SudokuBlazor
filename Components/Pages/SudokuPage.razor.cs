using Microsoft.AspNetCore.Components.Web;
using SudokuBlazor.Models;

namespace SudokuBlazor.Components.Pages;

public partial class SudokuPage
{
  private Sudoku Sudoku { get; } = new();

  public Cell? SelectedCell { get; set; }

  private static readonly int2 _up    = (-1,  0);
  private static readonly int2 _down  = ( 1,  0);
  private static readonly int2 _left  = ( 0, -1);
  private static readonly int2 _right = ( 0,  1);

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

  private void TryMoveSelection(int2 delta)
  {
    if (SelectedCell is null)
      return;

    int2 coords = SelectedCell.Coords + delta;
    if (Sudoku.TryGetCell(coords, out Cell? cell))
      SelectedCell = cell;
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

  private void CellClicked(Cell? cell)
    => SelectCell(cell);

  private void ValueClicked(int? value)
    => SetCellValue(value);

  private void OnKeyDown(KeyboardEventArgs e)
  {
    (e.Key switch
    {
      "ArrowUp" => () => TryMoveSelection(_up),
      "ArrowDown" => () => TryMoveSelection(_down),
      "ArrowLeft" => () => TryMoveSelection(_left),
      "ArrowRight" => () => TryMoveSelection(_right),
      "Tab" => () => TryMoveSelection(e.ShiftKey ? _left : _right),
      "Enter" => () => TryMoveSelection(e.ShiftKey ? _up : _down),
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