using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Webyneter.Sudoku.Core.Grids;
using Webyneter.Sudoku.Core.Miscs;
using Webyneter.Sudoku.Core.Solving;
using Webyneter.Sudoku.Miscellaneous.CUI;

namespace Webyneter.Sudoku.Solver
{
    internal static class Program
    {
        private const string DEFAULT_DIR_LOGS = "logs";
        private const string DEFAULT_DIR_CONSOLE_LOGS = "console";
        private const string DEFAULT_DIR_SOLUTION_LOGS = "solution";

        private static readonly string GRID_EXT = SudokuGridFile.Extension; 
        
        private static readonly string ABS_WORK_DIR = Directory.GetCurrentDirectory();
        private static readonly string ABS_WORK_DIR_SEPARATED = ABS_WORK_DIR + Path.DirectorySeparatorChar;
        
        private static string USER_DIR_INITIALS;
        private static string USER_ABS_DIR_INITIALS;

        private static string USER_DIR_LOGS;
        private static string USER_ABS_DIR_LOGS;

        private static string USER_DIR_CONSOLE_LOGS;
        private static string USER_ABS_DIR_CONSOLE_LOGS;

        private static string USER_DIR_SOLUTION_LOGS;
        private static string USER_ABS_DIR_SOLUTION_LOGS;

        private static void Main()
        {
            Console.CursorVisible = true;
            Console.WriteLine(
                ConsoleTextBlocks.ShowWelcome("Welcome to Traditional Sudoku Solver console program!") 
                + "\n");

            GetAvailableSudokuGrids();
            Console.WriteLine();
             
            while(true)
            {
                SudokuGrid grid;
                using (var selectedFile = ConsoleInteractions.ShowListAndSelectItem(USER_ABS_DIR_INITIALS,
                    SudokuGridFile.Extension,
                    Console.In,
                    Console.Out))
                {
                    grid = SudokuGrid.FromBinary(selectedFile);
                }
                
                Console.WriteLine();
                
                var solvingTask = new Task<bool>(() =>
                {
                    var gridSolver = new SudokuSolvingIterationAssumptionTechnique(grid);
                    gridSolver.Solve();
                    gridSolver.ApplySolutionToGrid();
                    return gridSolver.SolutionExists;
                });
                var dottingTaskCTS = new CancellationTokenSource();
                var dottingTask = new Task(() => ConsoleTextBlocks.ShowBlinkingDots(dottingTaskCTS.Token, Console.Out));
                var outputTask = new Task(() => ShowSolution(grid));
                var escapeTask = new Task<bool>(ConsoleInteractions.ShowEscapeQuestion);
                var checkingCorrectnessTask = new Task<bool>(grid.CheckCorrectness);

                checkingCorrectnessTask.ContinueWith((fin) => 
                {
                    if (fin.Result)
                    {
                        solvingTask.Start();
                    }
                    else
                    {
                        Console.WriteLine(ConsoleTextMessages.IncorrectInitialConfiguration + "\n");
                        escapeTask.Start();
                    } 
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                solvingTask.ContinueWith((fin) => dottingTaskCTS.Cancel(), TaskContinuationOptions.OnlyOnRanToCompletion);
                dottingTask.ContinueWith((fin) => 
                {
                    if (solvingTask.Result)
                    {
                        outputTask.Start();
                    }
                    else
                    {
                        Console.WriteLine(ConsoleTextMessages.IncorrectInitialConfiguration + "\n");
                        escapeTask.Start();
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                outputTask.ContinueWith((fin) => escapeTask.Start(), TaskContinuationOptions.OnlyOnRanToCompletion);
                
                dottingTask.Start();
                checkingCorrectnessTask.Start();

                escapeTask.Wait();
                if (escapeTask.Result)
                {
                    break;
                }
            }
        }
        
        private static void GetAvailableSudokuGrids()
        {
            string dir;

            for (; ; )
            {
                Console.Write("Initials' directory: {0}", ABS_WORK_DIR_SEPARATED);
                
                dir = Console.ReadLine();

                if (dir == "")
                    USER_ABS_DIR_INITIALS = ABS_WORK_DIR;
                else if (Directory.Exists(dir))
                    USER_ABS_DIR_INITIALS = ABS_WORK_DIR_SEPARATED + dir;
                else
                {
                    Console.Write("\n{0}\n", ConsoleTextMessages.DirectoryNotFound);
                    continue;
                }

                USER_DIR_INITIALS = dir;


                if ((new DirectoryInfo(dir)).GetFiles("*." + GRID_EXT).Length == 0)
                    Console.Write("\nDirectory \"{0}\" contains no .{1} files!\n", dir, GRID_EXT);
                else
                {
                    Console.Write("\nInitial configurations found! ");
                    Console.WriteLine(ConsoleTextBlocks.ShowCharsToLineEnd(Console.CursorLeft));
                    break;
                }
            }
        }

        private static void DecideLogging()
        {
            Func<string, string> __decideLogging = (dirChecking) => 
            {
                string mutingVar = null;

                if (Directory.Exists(dirChecking) &&
                    ConsoleInteractions.Ask(string.Format("Directory \"{0}\" found in {1} - do you want to use it",
                        dirChecking, ABS_WORK_DIR), Console.In, Console.Out))
                {
                    mutingVar = dirChecking;
                }

                if (mutingVar == null)
                {
                    mutingVar = ConsoleInteractions.RequireChild(ABS_WORK_DIR,
                        "Logs directory",
                        ConsoleTextMessages.DirectoryNotFound,
                        dirChecking,
                        Console.In,
                        Console.Out);

                    Directory.CreateDirectory(ABS_WORK_DIR_SEPARATED + mutingVar);
                }

                return mutingVar;
            };



            if (ConsoleInteractions.Ask("Enable any kinds of logging", Console.In, Console.Out))
            {
                Console.WriteLine();

                // спросить, нужна ли DEFAULT_DIR_LOGS она для хранения в ней логов:
                //  нужна -- поиск на диске DEFAULT_DIR_LOGS
                //      есть на диске -- спросить, использовать ли под логи
                //          использовать -- USER_ABS_DIR_LOGS = ... + DEFAULT_DIR_LOGS
                //          не использовать -- потребовать указать пользовательское имя создаваемой папки (проверка на == "logs") и создать
                //      нет на диске -- потребовать указать пользовательское имя создаваемой папки и создать
                //  не нужна -- перейти к поиску подпапок


                if (USER_DIR_LOGS == null &&
                    ConsoleInteractions.Ask("Store any kinds of log files in separate directory in " + ABS_WORK_DIR, 
                    Console.In, Console.Out))
                {
                    USER_DIR_LOGS = __decideLogging(DEFAULT_DIR_LOGS);
                    USER_ABS_DIR_LOGS = ABS_WORK_DIR_SEPARATED + USER_DIR_LOGS;
                    Console.WriteLine();
                }

                if (USER_DIR_CONSOLE_LOGS == null &&
                    ConsoleInteractions.Ask("Enable console logging", Console.In, Console.Out))
                {
                    USER_DIR_CONSOLE_LOGS = __decideLogging(Path.Combine(USER_DIR_LOGS, DEFAULT_DIR_CONSOLE_LOGS));
                    USER_ABS_DIR_CONSOLE_LOGS = ABS_WORK_DIR_SEPARATED + USER_DIR_CONSOLE_LOGS;
                    Console.WriteLine();
                }


                if (USER_DIR_SOLUTION_LOGS == null &&
                    ConsoleInteractions.Ask("Enable solution logging", Console.In, Console.Out)) 
                {
                    USER_DIR_SOLUTION_LOGS = __decideLogging(Path.Combine(USER_DIR_LOGS, DEFAULT_DIR_SOLUTION_LOGS));
                    USER_ABS_DIR_SOLUTION_LOGS = ABS_WORK_DIR_SEPARATED + USER_DIR_SOLUTION_LOGS;
                    Console.WriteLine();
                }
            }
        }
        
        private static void ShowSolution(SudokuGrid grid) 
        {
            Console.WriteLine("Solved InitialGrid:\n");

            byte i, j;
            var sb = new StringBuilder();

            char horDelim = '-';
            char vertDelim = '|';
            string horDelims;
            string leftOffset;

            for (i = 0; i < 25; ++i)
                sb.Append(horDelim);
            horDelims = sb.ToString();

            sb.Clear();
            for (i = 0; i < 2; ++i)
                sb.Append(" ");
            leftOffset = sb.ToString();


            var defaultForeColor = Console.ForegroundColor;

            Action<string> writeColoredDelim = (data) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(data);
                Console.ForegroundColor = defaultForeColor;
            };


            writeColoredDelim(leftOffset + horDelims + "\n");

            for (i = 0; i < grid.Metrics.MaximumNumber; ++i)
            {
                writeColoredDelim(leftOffset + vertDelim);

                for (j = 0; j < grid.Metrics.MaximumNumber; ++j)
                {
                    Console.Write(" " + grid[i, j]);

                    if ((j + 1) % 3 == 0)
                        writeColoredDelim(" " + vertDelim);
                }
                Console.WriteLine();

                if ((i + 1) % 3 == 0)
                    writeColoredDelim(leftOffset + horDelims + "\n");
            }

            Console.WriteLine();
        }
    }
}