using System;
using System.IO;
using System.Text;
using System.Threading;


namespace Webyneter.Sudoku.Miscellaneous.CUI
{
    /// <summary>
    /// Provides text block messages displayed to user.
    /// </summary>
    public static class ConsoleTextBlocks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="topSeparator"></param>
        /// <param name="bottomSeparator"></param>
        /// <returns></returns>
        public static string ShowWelcome(string message, char topSeparator = '\\', char bottomSeparator = '/')
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", ConsoleTextMessages.NullArgument);
            }
            message = message.Trim();
            string topHorDelim;
            string bottomHorDelim;
            int offset = (80 - message.Length) / 2;
            string offsetStr;

            var sb = new StringBuilder();

            for (byte i = 0; i < 80; ++i)
            {
                sb.Append(topSeparator);
            }
            topHorDelim = sb.ToString();

            sb.Clear();
            for (byte i = 0; i < 80; ++i)
            {
                sb.Append(bottomSeparator);
            }
            bottomHorDelim = sb.ToString();

            sb.Clear();
            for (byte i = 0; i < offset; ++i)
            {
                sb.Append(" ");
            }
            offsetStr = sb.ToString();

            sb.Clear();
            sb.AppendLine(topHorDelim);
            sb.AppendLine(offsetStr + message + offsetStr + "\n");
            sb.AppendLine(bottomHorDelim);

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cursorLeft"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string ShowCharsToLineEnd(int cursorLeft, char symbol = '*')
        {
            var sb = new StringBuilder();
            for (byte i = 0; i < 80 - cursorLeft; ++i)
            {
                sb.Append(symbol);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="consoleOut"></param>
        /// <param name="count"></param>
        /// <param name="msTimeout"></param>
        public static void ShowBlinkingDots(CancellationToken ct, TextWriter consoleOut, byte count = 9, 
            int msTimeout = 10)
        {
            if (consoleOut == null)
            {
                throw new ArgumentNullException("consoleOut", ConsoleTextMessages.NullArgument);
            }
			byte i;
            var sb = new StringBuilder(count);
            string empty;
            for (i = 0; i < count; ++i)
            {
                sb.Append(" ");
            }
            empty = sb.ToString();
            while (!ct.IsCancellationRequested)
            {
                for (i = 0; i < count; ++i)
                {
                    consoleOut.Write(".");
                    Thread.Sleep(msTimeout);
                }
                consoleOut.Write("\r" + empty + "\r");
            }
            consoleOut.WriteLine("\n");
        }
    }
}