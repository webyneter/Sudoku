using System;


namespace Webyneter.Sudoku.Core.Grids
{
    // WI: do we actually need it?
    /// <summary>
    /// Sudoku grid constraints.
    /// </summary>
    [Flags]
    public enum SudokuGridConstraints : byte
    {
        /// <summary>
        /// No constraints implied.
        /// </summary>
        Traditional = 0,
        /// <summary>
        /// Cells restricted to even numbers exist.
        /// </summary>
        Even = 0x01,
        /// <summary>
        /// Cells restricted to odd numbers exist.
        /// </summary>
        Odd = 0x02,
        /// <summary>
        /// Diagonal constraints implied.
        /// </summary>
        Diagonal = 0x04,
        // TODO: figure out how to implement and compress
        /// <summary>
        /// Bordered areas introduced.
        /// </summary>
        Areas = 0x08
    }
}