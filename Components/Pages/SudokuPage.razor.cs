using Microsoft.AspNetCore.Components.Web;
using SudokuBlazor.Models;

namespace SudokuBlazor.Components.Pages;

public partial class SudokuPage
{
  private Sudoku Sudoku { get; set; } = null!;

  public Cell? SelectedCell { get; set; }

  protected override void OnInitialized()
  {
    Sudoku = new();
  }

  private void New() => Sudoku = new();

  private void SelectCell(Cell? cell)
    => SelectedCell = cell;
  private void CellClicked(Cell? cell)
    => SelectCell(cell);

  private void SetCellValue(int? value)
  {
    SelectedCell?.Value = value;
    Sudoku.Update();
  }
  private void ValueClicked(int? value)
    => SetCellValue(value);

  private void TryMove(int2 delta)
  {
    if (SelectedCell is null)
      return;

    int2 coords = SelectedCell.Coords + delta;
    if (Sudoku.GetCell(coords, out Cell? cell))
      SelectedCell = cell;
  }

  private void OnKeyDown(KeyboardEventArgs e)
  {
    (e.Key switch
    {
      "ArrowUp" => () => TryMove(int2.Up),
      "ArrowDown" => () => TryMove(int2.Down),
      "ArrowLeft" => () => TryMove(int2.Left),
      "ArrowRight" => () => TryMove(int2.Right),
      "Tab" => () => TryMove(e.ShiftKey ? int2.Left : int2.Right),
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
  private void OnKeyUp(KeyboardEventArgs e)
  {

  }
  private void OnKeyPress(KeyboardEventArgs e)
  {

  }
}