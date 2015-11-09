using System;
using System.IO;

using Webyneter.Sudoku.Core.Conversion;
using Webyneter.Sudoku.Core.Miscs;
using Webyneter.Sudoku.Miscellaneous.CUI;


namespace Webyneter.Sudoku.FileConverter
{
    internal static class Program
    {
        private static readonly string SOURCE_EXT = "txt";
        private static readonly string CONVERTED_EXT = SudokuGridFile.Extension;

        private static readonly string WORK_ABS_DIR = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        private static string SOURCES_ABS_DIR;
        private static string OUTPUT_ABS_DIR;
        
        private static void Main()
        {
            Console.CursorVisible = true;

            Console.WriteLine(ConsoleTextBlocks.ShowWelcome("Welcome to Sudoku Grid Files Converter!"));
            Console.WriteLine();
            
            string dir = ConsoleInteractions.RequireChild(WORK_ABS_DIR,
                "Sources directory",
                ConsoleTextMessages.DirectoryNotFound,
                null,
                Console.In,
                Console.Out);
            SOURCES_ABS_DIR = WORK_ABS_DIR + dir + Path.DirectorySeparatorChar;
            OUTPUT_ABS_DIR = SOURCES_ABS_DIR;
            
            Console.WriteLine("\n" + ConsoleTextBlocks.ShowCharsToLineEnd(Console.CursorLeft));
            
            while(true)
            {
                string[] modes = Enum.GetNames(typeof(SudokuConvertionMode));
                FileStream selectedFile;
                string outputFilePath;

                var selectedMode = (SudokuConvertionMode)ConsoleInteractions.AskMultiple(modes, Console.In, Console.Out);
                Console.WriteLine("\n");

                switch (selectedMode)
                {
                    case SudokuConvertionMode.Encoding:
                        selectedFile = ConsoleInteractions.ShowListAndSelectItem(SOURCES_ABS_DIR,
                            SOURCE_EXT,
                            Console.In,
                            Console.Out);
                        outputFilePath = OUTPUT_ABS_DIR +
                            Path.GetFileNameWithoutExtension(OUTPUT_ABS_DIR + selectedFile.Name) + "." + CONVERTED_EXT;
                        using (SudokuGridFile.CreateBinaryFromText(selectedFile,
                                                                   outputFilePath,
                                                                   SudokuConvertionAlgorithm.NonUniform))
                        {
                            selectedFile.Dispose();
                        }
                        break;

                    case SudokuConvertionMode.Decoding:
                        selectedFile = ConsoleInteractions.ShowListAndSelectItem(SOURCES_ABS_DIR,
                            CONVERTED_EXT,
                            Console.In,
                            Console.Out);
                        outputFilePath = OUTPUT_ABS_DIR +
                            Path.GetFileNameWithoutExtension(OUTPUT_ABS_DIR + selectedFile.Name) + "." + SOURCE_EXT;
                        using (SudokuGridFile.CreateTextFromBinary(selectedFile,
                                                                   outputFilePath))
                        {
                            selectedFile.Dispose();
                        }
                        break;
                }
                
                Console.WriteLine(ConsoleTextBlocks.ShowCharsToLineEnd(Console.CursorLeft));
                Console.WriteLine();
                Console.WriteLine("Finished!");
                if (ConsoleInteractions.ShowEscapeQuestion())
                {
                    break;
                }
            }
        }
    }
}
