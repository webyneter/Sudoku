using System;


namespace Webyneter.Sudoku.Core.Grids
{
    // TODO: bind to owning cell
    /// <summary>
    /// Coordinates of a particular grid cell on a grid 
    /// centered to the upper left grid cell.
    /// </summary>
    public struct SudokuGridPosition : IEquatable<SudokuGridPosition>
    {
        /// <summary>
        /// Represents the SudokuGridPosition instance having its coordinates
        /// set to 0. This field is read-only.
        /// </summary>
        public static readonly SudokuGridPosition Empty = new SudokuGridPosition();

        /// <summary>
        /// X coordinate
        /// </summary>
        public readonly byte X;
        /// <summary>
        /// Y coordinate
        /// </summary>
        public readonly byte Y;

        // TODO: move out of here
        /// <summary>
        /// Indicates whether the grid border was once crossed.
        /// </summary>
        internal bool CrossedGridBorder { get { return crossedGridBorder; } }

        private bool crossedGridBorder;

        /// <summary>
        /// Initializes a new instance of SudokuCommons.DeskCellPosition structure 
        /// with specified coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="crossedEarlier"></param>
        internal SudokuGridPosition(byte x, byte y, bool crossedEarlier)
        {
            X = x;
            Y = y;
            crossedGridBorder = crossedEarlier;
        }

        /// <summary>
        /// Shifts this position for the specified number of cells. 
        /// </summary>
        /// <param name="cellsShifting">Left shift performed if the value is negative; 
        /// right shift performed if the value is positive; 
        /// no shift performed if the value is 0.</param>
        /// <param name="metrics">Metrics of the grid with this position.</param>
        /// <returns></returns>
        internal bool Shift(ushort cellsShifting, SudokuGridMetrics metrics)
        {
            if (cellsShifting == 0)
            {
                return false;
            }
            int cellDist = metrics.MaximumNumber * Y + X;
            bool crossedEarlier = crossedGridBorder;
            this = GetPosition((ushort)((cellDist + cellsShifting) % metrics.CellsTotal), metrics);
            if (crossedEarlier 
                || !crossedGridBorder && ((cellDist + Math.Abs(cellsShifting)) / metrics.CellsTotal) == 1)
            {
                crossedGridBorder = true;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static bool operator ==(SudokuGridPosition pos1, SudokuGridPosition pos2) { return pos1.Equals(pos2); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static bool operator !=(SudokuGridPosition pos1, SudokuGridPosition pos2) { return !(pos1.Equals(pos2)); }

        public override string ToString() { return "(X, Y) = (" + X + ", " + Y + ")"; }

        public override int GetHashCode() { return base.GetHashCode(); }

        public override bool Equals(object obj)
        {
            if (obj is SudokuGridPosition)
            {
                return Equals((SudokuGridPosition)obj);
            }
            return false;
        }

        // BUG: crossedDeskBorder SHOULD NOT be taken into consideration
        public bool Equals(SudokuGridPosition position) { return (X == position.X) && (Y == position.Y); }

        private SudokuGridPosition GetPosition(ushort cellDist, SudokuGridMetrics metrics)
        {
            for (byte i = 0; i < metrics.MaximumNumber; ++i)
            {
                for (byte j = 0; j < metrics.MaximumNumber; ++j)
                {
                    if (metrics.MaximumNumber * i + j == cellDist)
                    {
                        return new SudokuGridPosition(j, i, false);
                    }
                }
            }
            return Empty;
        }
    }
}