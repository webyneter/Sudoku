using System;


namespace Webyneter.Sudoku.Core.Exceptions
{
    /// <summary>
    /// Grid with specified parameters does not exist.
    /// </summary> 
    public class SudokuGridNotSupportedException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public SudokuGridNotSupportedException()
            : base("That grid class is not supported!") { }
    }
}