using System;
using System.Collections.Generic;
using System.IO;

using Webyneter.Sudoku.Core.Conversion;
using Webyneter.Sudoku.Core.Exceptions;


namespace Webyneter.Sudoku.Core.Grids
{
    // TODO: optimize the way the grid cells are currrently represented: use a sort of byte-sequences instead with a set of functionality aimed at finding the appropriate cell by its coords in virt coord space (x -> w * h) and vice-versa.
    /// <summary>
    /// Represents base class features derived by all desks.
    /// </summary>
    public abstract class SudokuGrid
    {
        internal class CellNumberChangedEventArgs : EventArgs
        {
            public SudokuGridCell Cell { get { return cell; } }

            private readonly SudokuGridCell cell;

            public CellNumberChangedEventArgs(SudokuGridCell cell) { this.cell = cell; }
        }

        // WI: ? make virtual calls
        /// <summary>
        /// Converts data in the specified binary file to the instance of grid.
        /// </summary>
        /// <param name="binaryFile">Binary file containing 
        /// compressed grid.</param>
        /// <returns>Instance of a particular grid.</returns>
        public static SudokuGrid FromBinary(FileStream binaryFile)
        {
            SudokuGridConstraints constraints;
            SudokuGridMetrics metrics;
            byte[] numbersKinds;
            string content = SudokuConverter.ToText(binaryFile, out constraints, out numbersKinds, out metrics);
            //string content = SudokuConverter.ToText(binaryFile, out constraints, out metrics);
            switch (metrics.MaximumNumber)
            {
                case 9:
                    var grid = new SudokuGrid9x3(constraints);
                    grid.IterateLinesXY((x, y) =>
                    {
                        var cell = new SudokuGridCell(grid, 
                            new SudokuGridPosition(x, y, false), 
                            byte.Parse(content[metrics.MaximumNumber * y + x].ToString()));
                        //cell.NumberChanged += (s, e) => grid.OnCellNumberChanged(new CellNumberChangedEventArgs((SudokuGridCell)s));
                        //grid.cells[y, x] = cell;
                        AssignNewCell(grid, cell);
                    });
                    return grid;
                default:
                    throw new SudokuGridNotSupportedException();
            }
        }

        // BUG: used to avoid buggy duplicate-code decision in solver classes -- get rid of as soon as possible
        internal static void AssignNewCell(SudokuGrid grid, SudokuGridCell cell)
        {
            cell.NumberChanged += (s, e) => grid.OnCellNumberChanged(new CellNumberChangedEventArgs((SudokuGridCell)s));
            grid.cells[cell.Position.Y, cell.Position.X] = cell;
        }

        // TODO: ? make virtual calls
        //public static SudokuGrid FromText(FileStream textFile)
        //{
        //    textFile.Position = 0;
        //    string content = new StreamReader(textFile).ReadToEnd();
        //    textFile.Position = 0;
        //    return 
        //}

        // TODO: figure out how to implement
        /// <summary>
        /// Grid constraints.
        /// </summary>
        public SudokuGridConstraints Constraints { get { return constraints; } }
        /// <summary>
        /// Grid metrics.
        /// </summary>
        public SudokuGridMetrics Metrics { get { return metrics; } }
        // WI: weak spot: everyone within the assembly can access and potentially corrupt that data
        /// <summary>
        /// Two-dimensional array of cells.
        /// First dimension: Y.
        /// Second dimension: X.
        /// </summary>
        internal SudokuGridCell[,] Cells {
            get { return cells; }
            set { cells = value; }
        }
        //{ get { return cells; } }
        /// <summary>
        /// Two-dimensional indexer for the grid cells array.
        /// </summary>
        /// <param name="y">Cell's vertical index.</param>
        /// <param name="x">Cell's horizontal index.</param>
        /// <returns></returns>
        public byte this[byte y, byte x]
        {
            get
            {
                if (cells != null)
                {
                    if ((y < metrics.MaximumNumber) && (x < metrics.MaximumNumber))
                    {
                        return cells[y, x].Number;
                    }
                    // TODO: move to resource storage
                    throw new ArgumentOutOfRangeException("Indecies were out of the grid's bounds!");
                }
                throw new ArgumentNullException("The grid cells are not initialized yet!");
            }
        }

        internal event EventHandler<CellNumberChangedEventArgs> CellNumberChanged = delegate { };

        // TODO: find the appropriate place for that info
        private readonly SudokuGridConstraints constraints;
        private readonly SudokuGridMetrics metrics;
        // TODO: make readonly
        private SudokuGridCell[,] cells;

        protected SudokuGrid(SudokuGridConstraints constraints, byte maxNumber, byte blockWidth, byte blockHeight)
        {
            this.constraints = constraints;
            metrics = new SudokuGridMetrics(maxNumber, blockWidth, blockHeight);
            cells = new SudokuGridCell[maxNumber, maxNumber];
        }
        
        // BUG: not working properly
        // WI: consider making static (? take care of )
        /// <summary>
        /// To success: 
        /// 1) the grid contains no lines with repeated numbers;
        /// 2) the grid contains at least one non-zero number;
        /// 3) the grid is square shaped.
        /// </summary>
        /// <returns>True if the grid is correct; else returns false.</returns>
        public bool CheckCorrectness()
        {
            if (Math.Sqrt(cells.Length) - Math.Truncate(Math.Sqrt(cells.Length)) > 0)
            {
                return false;
            }

            SudokuGridCell currCell;
            var numbersFound = new HashSet<byte>();
            bool containsOnlyZeros = true;
            Func<bool> returnFalse = () => false;
            IterateLine(y =>
            {
                IterateLine(x =>
                {
                    currCell = cells[y, x];
                    if (currCell.IsClue)
                    {
                        containsOnlyZeros = false;
                        if (numbersFound.Contains(currCell.Number))
                        {
                            returnFalse();
                        }
                        numbersFound.Add(currCell.Number);
                    }
                });
                numbersFound.Clear();
            });
            // the transposed previous
            IterateLine(y =>
            {
                IterateLine(x =>
                {
                    currCell = cells[x, y];
                    if (currCell.IsClue)
                    {
                        containsOnlyZeros = false;
                        if (numbersFound.Contains(currCell.Number))
                        {
                            returnFalse();
                        }
                        numbersFound.Add(currCell.Number);
                    }
                });
                numbersFound.Clear();
            });
            if (containsOnlyZeros)
            {
                return false;
            }
            
            IterateBlocksXY((x, y) =>
            {
                IterateBlockXY((bx, by) =>
                {
                    currCell = cells[(3 * y) + by, (3 * x) + bx];
                    if (currCell.IsClue 
                    && numbersFound.Contains(currCell.Number))
                    {
                        returnFalse();
                    }
                    numbersFound.Add(currCell.Number);
                });
                numbersFound.Clear();
            });

            return true;
        }

        // WI: !!! each of the methods below is amimed at square grids only
        // WI: consider making all the iteration-related methods static
        /// <summary>
        /// Iterates a row of the grid.
        /// </summary>
        internal void IterateLine(Action<byte> callback)
        {
            for (byte i = 0; i < metrics.MaximumNumber; ++i)
            {
                callback(i);
            }
        }

        /// <summary>
        /// Iterates rows of the grid top-to-bottom.
        /// </summary>
        internal void IterateLinesXY(Action<byte, byte> callback)
        {
            for (byte y = 0; y < metrics.MaximumNumber; ++y)
            {
                for (byte x = 0; x < metrics.MaximumNumber; ++x)
                {
                    callback(x, y);
                }
            }
        }

        /// <summary>
        /// Iterates inside the block located at the left-top corner.
        /// of the grid.
        /// </summary>
        internal void IterateBlockXY(Action<byte, byte> callback)
        {
            for (byte y = 0; y < metrics.BlockHeight; ++y)
            {
                for (byte x = 0; x < metrics.BlockWidth; ++x)
                {
                    callback(x, y);
                }
            }
        }

        /// <summary>
        /// Iterates blocks of the grid left-to-right top-to-bottom.
        /// </summary>
        internal void IterateBlocksXY(Action<byte, byte> callback)
        {
            var blocksVert = (byte)(metrics.MaximumNumber / metrics.BlockHeight);
            var blocksHor = (byte) (metrics.MaximumNumber / metrics.BlockWidth);
            for (byte y = 0; y < blocksVert; ++y)
            {
                for (byte x = 0; x < blocksHor; ++x)
                {
                    callback(x, y);
                }
            }
        }

        // TODO: consider making protected virtual
        private void OnCellNumberChanged(CellNumberChangedEventArgs e) { CellNumberChanged(this, e); }
    }
}