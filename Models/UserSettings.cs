using SudokuBlazor.Constants;
using SudokuBlazor.Enums;

namespace SudokuBlazor.Models;

public class UserSettings
{
  public bool WrapMovement { get; set; } = true;

  public bool DebugShowCellCoords { get; set; } = false;

  public InputMode InputMode { get; set; } = InputMode.Value;

  public Theme Theme { get; set; } = Theme.System;
}
