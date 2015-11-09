using System.IO;

using Webyneter.Sudoku.Core.Conversion;
using Webyneter.Sudoku.Core.Grids;


namespace Webyneter.Sudoku.Core.Miscs
{
    /// <summary>
    /// Provides static methods for sudoku files I/O.
    /// </summary>
    public static class SudokuGridFile
    {
        // WI: move the definition to outer scope (db-like storage, etc.)
        /// <summary>
        /// Extensions defined for each constraints of sudoku desks.
        /// </summary>
        public static readonly string Extension = "tradsud";

        /// <summary>
        /// Creates binary file with compressed data.
        /// </summary>
        /// <param name="textFile">File with text representation of a grid.</param>
        /// <param name="outputFilePath">Fully qualified name of output file.</param>
        /// <param name="alg">Preferable compression algorithm.</param>
        /// <returns>Seeked to the beginning binary file with read-write acessibilty 
        /// (existing file will be overwritten).</returns>
        public static FileStream CreateBinaryFromText(FileStream textFile, string outputFilePath, 
            SudokuConvertionAlgorithm alg)
        {
            // TODO: ? duplicate functionality -- use ToBinary(FileStream,  ...) instead
            textFile.Position = 0;
            string content = new StreamReader(textFile).ReadToEnd();
            textFile.Position = 0;

            var restrictions = SudokuGridConstraints.Traditional;
            var metrics = new SudokuGridMetrics(9, 3, 3);
            byte[] binary = SudokuConverter.ToBinary(content, alg, restrictions, metrics);

            var outputFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite);
            outputFile.Write(binary, 0, binary.Length);
            outputFile.Position = 0;
            
            return outputFile;
        }

        /// <summary>
        /// Creates text file from binary file containing compressed data.
        /// </summary>
        /// <param name="binaryFile">File with compressed binary data.</param>
        /// <param name="outputFilePath">Fully qualified name of output file.</param>
        /// <returns>Seeked to the beginning binary file with read-write acessibilty 
        /// (existing file will be overwritten).</returns>
        public static FileStream CreateTextFromBinary(FileStream binaryFile, string outputFilePath)
        {
            // TODO: duplicate functionality -- use ToText(FileStream,  ...) instead
            //var binary = new byte[binaryFile.Length];
            //binaryFile.Position = 0;
            //binaryFile.Read(binary, 0, (int)binaryFile.Length);
            //binaryFile.Position = 0;

            SudokuGridConstraints constraints;
            SudokuGridMetrics metrics;
            byte[] numbersKinds;
            string content = SudokuConverter.ToText(binaryFile, out constraints, out numbersKinds, out metrics);
            //string content = SudokuConverter.ToText(binary, out constraints, out metrics);

            var outputFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite);
            var sw = new StreamWriter(outputFile);
            sw.Write(content);
            sw.Flush();
            outputFile.Position = 0;

            return outputFile;
        }
    }
}