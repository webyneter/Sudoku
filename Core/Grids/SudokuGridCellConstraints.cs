using System;


namespace Webyneter.Sudoku.Core.Grids
{
    /// <summary>
    /// Cell number constraints.
    /// </summary>
    [Flags]
    public enum SudokuGridCellConstraints : byte
    {
        /// <summary>
        /// 
        /// </summary>
        Even = 0x01,
        /// <summary>
        /// 
        /// </summary>
        Odd = 0x02,
        /// <summary>
        /// 
        /// </summary>
        Any = (Even | Odd)
    }
}