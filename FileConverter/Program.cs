using System;
using System.IO;

using Webyneter.Sudoku.Core.Conversion;
using Webyneter.Sudoku.Core.Miscs;
using Webyneter.Sudoku.Miscellaneous.CUI;


namespace Webyneter.Sudoku.FileConverter
{
    internal static class Program
    {
        private static string EXT_SOURCE = "txt";
        private static string EXT_CONVERTED = SudokuGridFile.Extension;

        private static string ABS_WORK_DIR = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        private static string SOURCES_ABS_DIR;
        private static string OUTPUT_ABS_DIR;


        private static void Main()
        {
            Console.WriteLine(ConsoleTextBlocks.ShowWelcome(
                "Welcome to Sudoku Initials Converter!") + "\n");


            Console.CursorVisible = true;


            string dir = ConsoleInteractions.RequireChild(ABS_WORK_DIR,
                "Sources directory",
                ConsoleTextMessages.DirectoryNotFound,
                null,
                Console.In,
                Console.Out);
            SOURCES_ABS_DIR = ABS_WORK_DIR + dir + Path.DirectorySeparatorChar;
            OUTPUT_ABS_DIR = SOURCES_ABS_DIR;


            Console.WriteLine("\n" + ConsoleTextBlocks.ShowCharsToLineEnd(Console.CursorLeft));


            for (; ; )
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
                            EXT_SOURCE,
                            Console.In,
                            Console.Out);

                        outputFilePath = OUTPUT_ABS_DIR +
                            Path.GetFileNameWithoutExtension(OUTPUT_ABS_DIR + selectedFile.Name) + "." + EXT_CONVERTED;


                        using (SudokuGridFile.CreateBinaryFromText(selectedFile,
                            outputFilePath,
                            SudokuConvertionAlgorithm.NonUniform))
                            selectedFile.Dispose();

                        break;


                    case SudokuConvertionMode.Decoding:

                        selectedFile = ConsoleInteractions.ShowListAndSelectItem(SOURCES_ABS_DIR,
                            EXT_CONVERTED,
                            Console.In,
                            Console.Out);

                        outputFilePath = OUTPUT_ABS_DIR +
                            Path.GetFileNameWithoutExtension(OUTPUT_ABS_DIR + selectedFile.Name) + "." + EXT_SOURCE;


                        using (SudokuGridFile.CreateTextFromBinary(selectedFile,
                            outputFilePath))
                            selectedFile.Dispose();

                        break;
                }


                Console.WriteLine(ConsoleTextBlocks.ShowCharsToLineEnd(Console.CursorLeft));


                Console.WriteLine("\n" + "Finished!");
                if (ConsoleInteractions.ShowEscapeQuestion())
                    break;
            }
        }
    }
}
