using System;
using System.IO;


namespace Webyneter.Sudoku.Miscellaneous.CUI
{
    /// <summary>
    /// Provides functionality to interact with user.
    /// </summary>
    public static class ConsoleInteractions
    {
        public static bool Ask(string question, TextReader consoleIn, TextWriter consoleOut,
            char yesChar = 'y', char noChar = 'n')
        {
            if (question == null)
            {
                throw new ArgumentNullException("question", ConsoleTextMessages.NullArgument);
            }
            if (consoleIn == null)
            {
                throw new ArgumentNullException("consoleIn", ConsoleTextMessages.NullArgument);
            }
            if (consoleOut == null)
            {
                throw new ArgumentNullException("consoleOut", ConsoleTextMessages.NullArgument);
            }
            yesChar = yesChar.ToString().ToLower()[0];
            noChar = noChar.ToString().ToLower()[0];
            string answer;
            while(true)
            {
                consoleOut.Write("{0} ({1} / {2}): ", question, yesChar, noChar);
                answer = consoleIn.ReadLine().ToLower();
                if (answer.Length != 1)
                {
                    consoleOut.WriteLine(ConsoleTextMessages.InvalidInputPleaseReenter + "\n");
                }
                else if (answer[0] == yesChar)
                {
                    return true;
                }
                else if (answer[0] == noChar)
                {
                    return false;
                }
                else
                {
                    consoleOut.WriteLine(ConsoleTextMessages.InvalidInputPleaseReenter + "\n");
                }
            }
        }
        
        public static int AskMultiple(string[] options, TextReader consoleIn, TextWriter consoleOut)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options", ConsoleTextMessages.NullArgument);
            }
            if (consoleIn == null)
            {
                throw new ArgumentNullException("consoleIn", ConsoleTextMessages.NullArgument);
            }
            if (consoleOut == null)
            {
                throw new ArgumentNullException("consoleOut", ConsoleTextMessages.NullArgument);    
            }
            consoleOut.WriteLine("Available options:\n");
            for (int i = 0; i < options.Length; ++i)
            {
                consoleOut.WriteLine((i + 1) + ". " + options[i].Trim());
            }
            int itemNum;
            while(true)
            {
                consoleOut.Write("\nChoose option by number: ");
                if (int.TryParse(consoleIn.ReadLine(), out itemNum) &&
                    (itemNum > 0) &&
                    (itemNum <= options.Length))
                {
                    break;
                }
                consoleOut.WriteLine(ConsoleTextMessages.InvalidInputPleaseReenter);
            }
            return itemNum - 1;
        }
        
        public static string RequireChild(string dirAbsPath, string requestMess, string errorMess,
            string forbiddenName, TextReader consoleIn, TextWriter consoleOut)
        {
            if (dirAbsPath == null)
            {
                throw new ArgumentNullException("dirAbsPath", ConsoleTextMessages.NullArgument);
            }
            if (requestMess == null)
            {
                throw new ArgumentNullException("requestMess", ConsoleTextMessages.NullArgument);
            }
            if (errorMess == null)
            {
                throw new ArgumentNullException("errorMess", ConsoleTextMessages.NullArgument);
            }
            if (consoleIn == null)
            {
                throw new ArgumentNullException("consoleIn", ConsoleTextMessages.NullArgument);
            }
            if (consoleOut == null)
            {
                throw new ArgumentNullException("consoleOut", ConsoleTextMessages.NullArgument);
            }
            if (forbiddenName != null)
            {
                forbiddenName = forbiddenName.Trim();
            }
            while (true)
            {
                consoleOut.Write("{0}: {1}", requestMess, dirAbsPath);
                string dir = consoleIn.ReadLine();
                if (forbiddenName != null 
                    && dir == forbiddenName)
                {
                    consoleOut.WriteLine("The dir name \"{0}\" is forbidden!", forbiddenName);
                    continue;
                }
                if (dir == "")
                {
                    return "";
                }
                if (Directory.Exists(dir))
                {
                    return dir;
                }
                consoleOut.WriteLine("\n" + errorMess + "\n");
            }
        }
        
        public static FileStream ShowListAndSelectItem(string dirAbsPath, string ext, TextReader consoleIn, 
            TextWriter consoleOut)
        {
            if (dirAbsPath == null)
            {
                throw new ArgumentNullException("dirAbsPath", ConsoleTextMessages.NullArgument);
            }
            if (ext == null)
            {
                throw new ArgumentNullException("ext", ConsoleTextMessages.NullArgument);
            }
            if (consoleIn == null)
            {
                throw new ArgumentNullException("consoleIn", ConsoleTextMessages.NullArgument);
            }
            if (consoleOut == null)
            {
                throw new ArgumentNullException("consoleOut", ConsoleTextMessages.NullArgument);
            }
            consoleOut.WriteLine("Available grids:\n");
            var dir = new DirectoryInfo(dirAbsPath);
            FileInfo[] files = dir.GetFiles("*." + ext);
            for (int i = 0; i < files.Length; ++i)
            {
                consoleOut.WriteLine(i + 1 + ". " + files[i].Name);
            }
            int itemNum;
            while(true)
            {
                consoleOut.Write("\n\nNumber of the file to operate with: ");
                if (int.TryParse(consoleIn.ReadLine(), out itemNum) &&
                    (itemNum > 0) && 
                    (itemNum <= files.Length))
                {
                    consoleOut.Write("({0})", files[itemNum - 1].Name);
                    consoleOut.WriteLine();
                    break;
                }
                consoleOut.WriteLine(ConsoleTextMessages.InvalidInputPleaseReenter);
            }
            return new FileStream(dirAbsPath + "\\" + files[itemNum - 1].Name, FileMode.Open, FileAccess.Read);
        }
        
        public static bool ShowEscapeQuestion()
        {
            Console.Write("<Enter> to repeat; <Esc> to exit: ");
            ConsoleKeyInfo keyPressed;
            while (true)
            {
                keyPressed = Console.ReadKey(false);
                Console.WriteLine();
                if (keyPressed.Key == ConsoleKey.Escape)
                {
                    return true;
                }
                if (keyPressed.Key != ConsoleKey.Enter)
                {
                    Console.WriteLine(ConsoleTextMessages.InvalidInputPleaseReenter + "\n");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(ConsoleTextBlocks.ShowCharsToLineEnd(Console.CursorLeft) + "\n");
                    return false;
                }
            }
        }
    }
}