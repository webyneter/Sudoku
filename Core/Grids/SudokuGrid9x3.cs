namespace Webyneter.Sudoku.Core.Grids
{
    /// <summary>
    /// 9x9 grid with 3x3 blocks.
    /// </summary>
    public sealed class SudokuGrid9x3 : SudokuGrid
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="constraints"></param>
        internal SudokuGrid9x3(SudokuGridConstraints constraints) : base(constraints, 9, 3, 3) { }
    }
}