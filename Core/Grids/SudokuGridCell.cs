using System;
using System.Collections.Generic;
using System.Linq;


namespace Webyneter.Sudoku.Core.Grids
{
    // TODO: get rid of bidirectional dependency between a grid and its cells: when the cell is created, it should be binded to the specified grid immediately in case no cell with the specified position already exists on the grid -- if there is one, return false (??? throw exception; use factory method)
    // WI: consider separating the concepts of a cell and its candidates
    // WI: consider converting to structure after the cell-candidates concepts separation
    /// <summary>
    /// Represents a single cell of any grid.
    /// </summary>
    public sealed class SudokuGridCell
    {
        /// <summary>
        /// Represents a set of numeric candidates to be substituted to the corresponding cell.
        /// </summary>
        internal class CandidatesSortedSet : SortedSet<byte>
        {
            /// <summary>
            /// 
            /// </summary>
            public SudokuGridCell ParentCell { get; private set; }
            /// <summary>
            /// 
            /// </summary>
            public event EventHandler CandidatesDecreased = delegate { };
            /// <summary>
            /// 
            /// </summary>
            public event EventHandler ContradictionFound = delegate { };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentCell"></param>
            public CandidatesSortedSet(SudokuGridCell parentCell)
            {
                ParentCell = parentCell;
                CandidatesDecreased += (s, e) =>
                {
                    // no substitutions exist => incorrect substitution has been made earlier
                    if (Count == 0)
                    {
                        OnContradictionFound(EventArgs.Empty);
                    }
                    // the only possible substitution found
                    else if (Count == 1)
                    {
                        parentCell.Number = this.ToArray()[0];
                        parentCell.Candidates = null;
                    }
                };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            new public bool Add(byte item)
            {
                if (item > 0 
                    && item <= ParentCell.ParentGrid.Metrics.MaximumNumber)
                {
                    return base.Add(item);
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            new public bool Remove(byte item)
            {
                bool wasRemoved = base.Remove(item);
                if (wasRemoved)
                {
                    OnCandidatesDecreased(EventArgs.Empty);
                }
                return wasRemoved;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="other"></param>
            new public void ExceptWith(IEnumerable<byte> other)
            {
                int count = Count;
                base.ExceptWith(other);
                if (Count < count)
                {
                    OnCandidatesDecreased(EventArgs.Empty);
                }
            }

            protected virtual void OnCandidatesDecreased(EventArgs e) { CandidatesDecreased(this, e); }

            protected virtual void OnContradictionFound(EventArgs e) { ContradictionFound(this, e); }
        }
        
        // TODO: store in an external resource
        public static readonly byte EmptyCellNumber = 0;

        /// <summary>
        /// Grid instance which includes this cell.
        /// </summary>
        public SudokuGrid ParentGrid { get; private set; }
        /// <summary>
        /// The number substituted into this cell.
        /// </summary>
        public byte Number
        {
            get { return number; }
            set
            {
                if (value != number)
                {
                    number = value;
                    OnNumberChanged(EventArgs.Empty);
                }
            }
        }

        public bool IsClue { get { return number != EmptyCellNumber; } }

        /// <summary>
        /// Coordinates of this cell at the owing grid.
        /// </summary>
        public SudokuGridPosition Position { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler NumberChanged = delegate { };
        // todo: move candidates into a separate layer specific to the solver (or group of solvers)
        // wi: introduce solvers grouping by the kind of underlying strategies (those, which need the concept of candidate numbers, etc.): use events to notify the cell to change its value (resolve the issue with access)
        /// <summary>
        /// The set of numeric candidates.
        /// </summary>
        internal CandidatesSortedSet Candidates { get; private set; }
        /// <summary>
        /// Indicates whether the number substitution was incorrect.
        /// </summary>
        internal bool ContradictionFound { get; private set; }

        private byte number;

        /// <summary>
        /// Initializes a new instance of grid cell.
        /// </summary>
        /// <param name="parentGrid">The grid including this cell.</param>
        /// <param name="position">The cell's position at the specified grid.</param>
        /// <param name="number">The number substituted into this cell.</param>
        internal SudokuGridCell(SudokuGrid parentGrid, SudokuGridPosition position, byte number)
        {
            ParentGrid = parentGrid;
            Position = position;
            if (number == EmptyCellNumber)
            {
                this.number = number;
                Candidates = new CandidatesSortedSet(this);
                Candidates.ContradictionFound += (s, e) => ContradictionFound = true;
                switch (parentGrid.Constraints)
                {
                    case SudokuGridConstraints.Traditional:
                        for (byte i = 1; i <= parentGrid.Metrics.MaximumNumber; ++i)
                        {
                            Candidates.Add(i);
                        }
                        break;

                    case SudokuGridConstraints.Diagonal:
                        break;

                    case SudokuGridConstraints.Even:
                        break;

                    case SudokuGridConstraints.Odd:
                        break;

                    case SudokuGridConstraints.Areas:
                        break;
                }
            }
            else
            {
                Number = number;
            }
        }

        private void OnNumberChanged(EventArgs e) { NumberChanged(this, e); }
    }
}