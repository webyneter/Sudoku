namespace Webyneter.Sudoku.Core.Grids
{
    /// <summary>
    /// 16x16 grid with 4x4 blocks.
    /// </summary>
    public sealed class SudokuGrid16x4 : SudokuGrid
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="constraints"></param>
        internal SudokuGrid16x4(SudokuGridConstraints constraints) : base(constraints, 16, 4, 4) { }
    }
}