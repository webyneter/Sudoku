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
        
        private static readonly string WORK_ABS_DIR = Directory.GetCurrentDirectory();
        private static readonly string WORK_ABS_DIR_SEPARATED = WORK_ABS_DIR + Path.DirectorySeparatorChar;
        
        private static string USER_DIR_AVAILABLE_GRIDS;
        private static string USER_ABS_DIR_AVAILABLE_GRIDS;

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
                ConsoleTextBlocks.ShowWelcome("Welcome to Sudoku Solver CUI!") 
                + "\n");

            GetAvailableSudokuGrids();

            Console.WriteLine();
             
            while(true)
            {
                SudokuGrid grid;
                using (var selectedFile = ConsoleInteractions.ShowListAndSelectItem(USER_ABS_DIR_AVAILABLE_GRIDS,
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
            while(true)
            {
                Console.Write("Available grids' directory: {0}", WORK_ABS_DIR_SEPARATED);
                
                string dir = Console.ReadLine();
                if (dir == "")
                {
                    USER_ABS_DIR_AVAILABLE_GRIDS = WORK_ABS_DIR;
                }
                else if (Directory.Exists(dir))
                {
                    USER_ABS_DIR_AVAILABLE_GRIDS = WORK_ABS_DIR_SEPARATED + dir;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(ConsoleTextMessages.DirectoryNotFound);
                    continue;
                }

                USER_DIR_AVAILABLE_GRIDS = dir;

                Console.WriteLine();
                if (new DirectoryInfo(dir).GetFiles("*." + GRID_EXT).Length == 0)
                {
                    Console.WriteLine("Directory \"{0}\" contains no .{1} grid files!", dir, GRID_EXT);
                }
                else
                {
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
                        dirChecking, WORK_ABS_DIR), Console.In, Console.Out))
                {
                    mutingVar = dirChecking;
                }

                if (mutingVar == null)
                {
                    mutingVar = ConsoleInteractions.RequireChild(WORK_ABS_DIR,
                        "Logs directory",
                        ConsoleTextMessages.DirectoryNotFound,
                        dirChecking,
                        Console.In,
                        Console.Out);

                    Directory.CreateDirectory(WORK_ABS_DIR_SEPARATED + mutingVar);
                }

                return mutingVar;
            };
            
            if (ConsoleInteractions.Ask("Enable any kinds of logging", Console.In, Console.Out))
            {
                Console.WriteLine();
                if (USER_DIR_LOGS == null 
                    && ConsoleInteractions.Ask("Store any kinds of log files in separate directory in " + WORK_ABS_DIR, 
                    Console.In, Console.Out))
                {
                    USER_DIR_LOGS = __decideLogging(DEFAULT_DIR_LOGS);
                    USER_ABS_DIR_LOGS = WORK_ABS_DIR_SEPARATED + USER_DIR_LOGS;
                    Console.WriteLine();
                }
                if (USER_DIR_CONSOLE_LOGS == null 
                    && ConsoleInteractions.Ask("Enable console logging", Console.In, Console.Out))
                {
                    USER_DIR_CONSOLE_LOGS = __decideLogging(Path.Combine(USER_DIR_LOGS, DEFAULT_DIR_CONSOLE_LOGS));
                    USER_ABS_DIR_CONSOLE_LOGS = WORK_ABS_DIR_SEPARATED + USER_DIR_CONSOLE_LOGS;
                    Console.WriteLine();
                }
                if (USER_DIR_SOLUTION_LOGS == null 
                    && ConsoleInteractions.Ask("Enable solution logging", Console.In, Console.Out)) 
                {
                    USER_DIR_SOLUTION_LOGS = __decideLogging(Path.Combine(USER_DIR_LOGS, DEFAULT_DIR_SOLUTION_LOGS));
                    USER_ABS_DIR_SOLUTION_LOGS = WORK_ABS_DIR_SEPARATED + USER_DIR_SOLUTION_LOGS;
                    Console.WriteLine();
                }
            }
        }
        
        private static void ShowSolution(SudokuGrid grid) 
        {
            Console.WriteLine("Found solution:");
            Console.WriteLine();
            
            var sb = new StringBuilder();

            char horDelim = '-';
            char vertDelim = '|';

            for (byte i = 0; i < 25; ++i)
            {
                sb.Append(horDelim);
            }
            string horDelims = sb.ToString();

            sb.Clear();
            for (int i = 0; i < 2; ++i)
            {
                sb.Append(" ");
            }
            string leftOffset = sb.ToString();

            var defaultForeColor = Console.ForegroundColor;

            Action<string> writeColoredDelim = (data) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(data);
                Console.ForegroundColor = defaultForeColor;
            };
            
            writeColoredDelim(leftOffset + horDelims + "\n");

            for (byte i = 0; i < grid.Metrics.MaximumNumber; ++i)
            {
                writeColoredDelim(leftOffset + vertDelim);
                for (byte j = 0; j < grid.Metrics.MaximumNumber; ++j)
                {
                    Console.Write(" " + grid[i, j]);
                    if ((j + 1) % 3 == 0)
                    {
                        writeColoredDelim(" " + vertDelim);
                    }
                }
                Console.WriteLine();
                if ((i + 1) % 3 == 0)
                {
                    writeColoredDelim(leftOffset + horDelims + "\n");
                }
            }

            Console.WriteLine();
        }
    }
}