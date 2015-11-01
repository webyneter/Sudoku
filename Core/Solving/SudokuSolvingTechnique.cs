using System;
using System.Collections.Generic;
using System.Linq;

using Webyneter.Sudoku.Core.Grids;


namespace Webyneter.Sudoku.Core.Solving
{
    // TODO: solver exposes the functionality for persisting and, if neccessary, rolling back the captured grid's state
    public abstract class SudokuSolvingTechnique
    {
        public static IEnumerable<KeyValuePair<string, Type>> NameToTechniqueMap { get { return nameToTechniqueMap; } }

        private static readonly Dictionary<string, Type> nameToTechniqueMap = new Dictionary<string, Type>();
        
        protected static bool RegisterDerivedTechnique(Type derived)
        {
            if (derived.IsSubclassOf(typeof(SudokuSolvingTechnique)) 
                && !nameToTechniqueMap.ContainsKey(derived.ToString()))
            {
                nameToTechniqueMap.Add(derived.ToString(), derived);
            }
            return false;
        }

        public SudokuGrid Grid { get; protected set; }
        public bool SolutionExists { get { return foundSolutions.Count > 0; } }

        private readonly SudokuGridCell[,] initialGridCells;
        private readonly List<SudokuGridCell[,]> foundSolutions = new List<SudokuGridCell[,]>();

        protected SudokuSolvingTechnique(SudokuGrid grid)
        {
            Grid = grid;
            // BUG: the solver changes cells' collection via exactly the same pointer
            initialGridCells = grid.Cells;
        }



        public void Solve()
        {
            SearchSolutionDebug();
            // if the Grid was changed from the moment of the latest solution search,
            // a new family of solutions may vary
            foundSolutions.Clear();
            foundSolutions.Add(Grid.Cells);


            //if (solution != null)
            //{
            //    foundSolutions.Add(solution);
            //}
            //SudokuGridCell[,] solution;
            //SearchSolution(out solution);
            //foundSolutions.Clear();
            //if (solution != null)
            //{
            //    foundSolutions.Add(solution);
            //}

            //List<SudokuGridCell[,]> solutions = SearchSolutions();
            //foundSolutions.Clear();
            //if (solutions.Count > 0)
            //{
            //    foundSolutions.AddRange(solutions);
            //}
        }

        public IEnumerable<int> EnumerateFoundSolutionsIndecies() { return Enumerable.Range(0, foundSolutions.Count); }

        /// <summary>
        /// Assign empty cells of the initial grid with the corresponding numbers
        /// of the solution having the specified index in found solutions collection.
        /// </summary>
        /// <param name="solutionIndex">Zero-based index in found solutions collection.</param>
        public void ApplySolutionToGrid(int solutionIndex = 0)
        {
            if (foundSolutions.Count == 0)
            {
                // TODO: throw 'no solutions were found yet' exception
                throw new NotImplementedException();
            }
            if (solutionIndex >= foundSolutions.Count)
            {
                // TODO: throw 'found solutions collection contains no entry with the specified index' exception
                throw new NotImplementedException();
            }
            Grid.IterateLinesXY((x, y) => Grid.Cells[y, x] = foundSolutions[solutionIndex][y, x]);
        }

        public void ResetGrid() { Grid.IterateLinesXY((x, y) => Grid.Cells[y, x] = initialGridCells[y, x]); }

        //protected abstract bool SearchSolution(out SudokuGridCell[,] solutions);

        protected abstract bool SearchSolutionDebug();

        // TODO: implement!
        //protected abstract List<SudokuGridCell[,]> SearchSolutions();
    }
}