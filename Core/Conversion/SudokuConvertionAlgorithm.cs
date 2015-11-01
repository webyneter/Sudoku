namespace Webyneter.Sudoku.Core.Conversion
{
    // TODO: make the conversion subsystem scalable much like the solving subsystem was recently re-designed
    /// <summary>
    /// Available sudoku files' compression algorithms.
    /// </summary>
    public enum SudokuConvertionAlgorithm : byte
    {
        /// <summary>
        /// All numbers' bits' sequences are compressed as much as possible. 
        /// </summary>
        Uniform,
        /// <summary>
        /// All numbers' (except zeros) bits' sequences are compressed using
        /// SudokuConvertionAlgorithms.Uniform algoritm while zeros are zipped.
        /// </summary>
        NonUniform
    }
}