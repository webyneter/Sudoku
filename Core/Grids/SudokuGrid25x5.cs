namespace Webyneter.Sudoku.Core.Grids
{
    /// <summary>
    /// 25x25 grid with 5x5 blocks.
    /// </summary>
    public sealed class SudokuGrid25x5 : SudokuGrid
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="constraints"></param>
        internal SudokuGrid25x5(SudokuGridConstraints constraints) : base(constraints, 25, 5, 5) { }
    }
}