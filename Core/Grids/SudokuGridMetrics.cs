namespace Webyneter.Sudoku.Core.Grids
{
    /// <summary>
    /// Represents metrics of any grid.
    /// </summary>
    public struct SudokuGridMetrics
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly SudokuGridMetrics Empty = new SudokuGridMetrics();

        /// <summary>
        /// 
        /// </summary>
        public readonly byte MaximumNumber;
        /// <summary>
        /// 
        /// </summary>
        public readonly ushort CellsTotal;
        /// <summary>
        /// 
        /// </summary>
        public readonly byte BlockWidth;
        /// <summary>
        /// 
        /// </summary>
        public readonly byte BlockHeight;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxNumber"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        internal SudokuGridMetrics(byte maxNumber, byte blockWidth, byte blockHeight)
        {
            MaximumNumber = maxNumber;
            CellsTotal = (ushort)(maxNumber * maxNumber);
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
        }
    }
}