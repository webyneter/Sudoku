using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Webyneter.Sudoku.Core.Grids;

namespace Webyneter.Sudoku.Core.Solving
{
    // TODO: make use of the initial grid's local cells collection copy instead of the original one
    public sealed class SudokuSolvingIterationAssumptionTechnique : SudokuSolvingTechnique
    {
        // TODO: remove
        static SudokuSolvingIterationAssumptionTechnique()
        {
            RegisterDerivedTechnique(typeof(SudokuSolvingIterationAssumptionTechnique));
        }
        
        private SudokuGridPosition currCheckingPos;
        private SudokuGridPosition? lastFilledPos;
        private ushort emptyCellsCount;
        // WI: consider changing types to Set<byte> (benefit from sorting is about to emerge on large-scale grids which are rarely expected to be the case)
        // TODO: optimize the use of so many arrays of sorted sets (tremendous memory/perfomance drawbacks on large desks expected)
        private readonly SortedSet<byte>[] rowsOfNumbers;
        private readonly SortedSet<byte>[] columnsOfNumbers;
        private readonly SortedSet<byte>[,] blocksOfNumbers;
        private readonly SortedSet<byte> forbiddenCandidates;
        private SudokuGrid assumptionsGrid;
        
        public SudokuSolvingIterationAssumptionTechnique(SudokuGrid grid) : base(grid)
        {
            rowsOfNumbers = new SortedSet<byte>[Grid.Metrics.MaximumNumber];
            columnsOfNumbers = new SortedSet<byte>[Grid.Metrics.MaximumNumber];
            Grid.IterateLine(i =>
            {
                rowsOfNumbers[i] = new SortedSet<byte>();
                columnsOfNumbers[i] = new SortedSet<byte>();
            });
            blocksOfNumbers = new SortedSet<byte>[Grid.Metrics.MaximumNumber / Grid.Metrics.BlockHeight,
                Grid.Metrics.MaximumNumber / Grid.Metrics.BlockWidth];
            Grid.IterateBlocksXY((x, y) =>
            {
                blocksOfNumbers[y, x] = new SortedSet<byte>();
            });
            forbiddenCandidates = new SortedSet<byte>();
            Grid.CellNumberChanged += (s, e) => RegisterCellAsClue(e.Cell);
            
            emptyCellsCount = Grid.Metrics.CellsTotal;

            //Grid.IterateLinesXY((x, y) =>
            //{
            //    SudokuGridCell cell = Grid.Cells[y, x];
            //    if (cell.IsClue)
            //    {
            //        RegisterCellAsClue(cell);
            //    }
            //});
        }

        // TODO: rename
        private void RegisterCellAsClue(SudokuGridCell cell)
        {
            rowsOfNumbers[cell.Position.Y].Add(cell.Number);
            columnsOfNumbers[cell.Position.X].Add(cell.Number);
            byte x, y;
            FindContainingBlock(cell, out x, out y);
            blocksOfNumbers[y, x].Add(cell.Number);
            --emptyCellsCount;
        }

        
        // TODO: implement the remaining functionality to find all possible solutions
        // јлгоритм: последовательный проход по клеткам слева направо и вниз
        // и усечение мн-ва вариантов цифр, 
        // которые могут находитьс€ в данной клетке; единственный
        // оставшийс€ вариант подставл€етс€ в клетку.  аждый раз,
        // как совершаетс€ повторный обход доски, должна заполн€тьс€
        // хот€ бы одна клетка. »наче алгоритм зацикливаетс€,
        // и запускаетс€ другой алгоритм:
        // в первую найденную клетку, имеющую наименьшее кол-во вариантов,
        // подставл€етс€ наименьший из них (несущественно) и поиск продолжаетс€
        // на специальной доске assumptionsGrid по исходному
        // алгоритму: если полученное решение непротиворечиво, то
        // оно становитс€ окончательным. »наче: а) если получаем 
        // противоречие, то исходна€ подстановка неверна и, 
        // вернувшись к точке осущствлени€ подстановки, 
        // подставл€ем другую цифру, вновь примен€€ исохдный алгоритм;
        // б) если получаем зацикливание, то снова находим эл-т 
        // с наименьшим кол-вом вариантов, подставл€ем один из них 
        // и примен€ем исходный алгоритм.
        // TODO: protected override List<SudokuGridCell[,]> SearchSolutions()
        protected override bool SearchSolutionDebug()
        {
            // TODO: call Reset() instead
            forbiddenCandidates.Clear();
            Grid.IterateLinesXY((x, y) =>
            {
                SudokuGridCell cell = Grid.Cells[y, x];
                if (cell.IsClue)
                {
                    RegisterCellAsClue(cell);
                }
            });

            var resultCells = _SearchSolutionDebug();
            if (resultCells == null)
            {
                return false;
            }
            Grid.Cells = resultCells;
            // TODO: call Reset() when the solution process is finished
            return true;
        }

        private SudokuGridCell[,] _SearchSolutionDebug()
        {
            SudokuGridPosition? minCandidatesCellPos = null;
            while (emptyCellsCount > 0)
            {
                bool looped;
                bool contradicted;
                ExceptCandidatesInCell(out looped, out contradicted);
                if (looped)
                {
                    var resultCells = MakeAssumptionInCell(ref minCandidatesCellPos);
                    if (resultCells != null)
                    {
                        return resultCells;
                    }
                }
                if (contradicted)
                {
                    return null;
                }
            }
            return Grid.Cells;
        }


        //protected override bool SearchSolution(out SudokuGridCell[,] solution)
        //{
        //    //Reset();

        //    forbiddenCandidates.Clear();

        //    var res = _SearchSolution();
        //    if (res == null)
        //    {
        //        solution = null;
        //        return false;
        //    }
        //    //Grid.IterateLinesXY((i, j) => Grid.Cells[j, i] = res[j, i]);
        //    Grid.Cells = res;
        //    solution = res;
        //    return true;
        //    //var solutions = new List<SudokuGridCell[,]>();

        //    //return solutions;
        //}

        //private SudokuGridCell[,] _SearchSolution()
        //{
        //    SudokuGridPosition? minCandidatesCellPos = null;
        //    while (emptyCellsCount > 0)
        //    {
        //        bool looped;
        //        bool contradicted;
        //        ExceptCandidatesInCell(out looped, out contradicted);
        //        if (looped)
        //        {
        //            SudokuGridCell[,] result = MakeAssumptionInCell(ref minCandidatesCellPos);
        //            if (result != null)
        //            {
        //                //solution = result;
        //                //Grid.IterateLinesXY((i, j) => Grid.Cells[j, i] = result[j, i]);
        //                return result;
        //            }
        //        }
        //        if (contradicted)
        //        {
        //            //solution = null;
        //            //return false;
        //            //break;
        //            return null;
        //        }
        //    }
        //    //solution = Grid.Cells;
        //    //return false;
        //    return Grid.Cells;
        //}


        private void Reset()
        {
            Grid.IterateLine(i =>
            {
                rowsOfNumbers[i].Clear();
                columnsOfNumbers[i].Clear();
            });
            Grid.IterateBlocksXY((x, y) =>
            {
                blocksOfNumbers[y, x].Clear();
            });
            forbiddenCandidates.Clear();

            emptyCellsCount = Grid.Metrics.CellsTotal;

            Grid.IterateLinesXY((x, y) =>
            {
                SudokuGridCell cell = Grid.Cells[y, x];
                if (cell.IsClue)
                {
                    RegisterCellAsClue(cell);
                }
            });
        }


        private void ExceptCandidatesInCell(out bool looped, out bool contradicted)
        {
            looped = false;
            contradicted = false;
            SudokuGridPosition currCellPos = currCheckingPos;
            currCheckingPos.Shift(1, Grid.Metrics);
            if (!lastFilledPos.HasValue && currCellPos.CrossedGridBorder
                || lastFilledPos.HasValue && lastFilledPos == currCellPos)
            {
                looped = true;
                return;
            }

            var currCell = Grid.Cells[currCellPos.Y, currCellPos.X];
            if (!currCell.IsClue)
            {
                forbiddenCandidates.UnionWith(rowsOfNumbers[currCellPos.Y]);
                forbiddenCandidates.UnionWith(columnsOfNumbers[currCellPos.X]);
                byte x, y;
                FindContainingBlock(currCell, out x, out y);
                forbiddenCandidates.UnionWith(blocksOfNumbers[y, x]);

                currCell.Candidates.ExceptWith(forbiddenCandidates);
                forbiddenCandidates.Clear();

                if (currCell.ContradictionFound)
                {
                    contradicted = true;
                    return;
                }

                if (currCell.IsClue)
                {
                    lastFilledPos = currCell.Position;
                    // WI: are currCheckingPos... missing here?
                    //currCheckingPos = currCell.Position;
                    //currCheckingPos.Shift(1, Grid.Metrics);
                }
            }
        }
        
        // WI: consider moving to cell class
        private void FindContainingBlock(SudokuGridCell cell, out byte x, out byte y)
        {
            x = (byte)(cell.Position.X / cell.ParentGrid.Metrics.BlockWidth);
            y = (byte)(cell.Position.Y / cell.ParentGrid.Metrics.BlockHeight);
        }

        private SudokuGridCell[,] MakeAssumptionInCell(ref SudokuGridPosition? minCandidatesCellPos)
        {
            if (!minCandidatesCellPos.HasValue)
            {
                // TODO: optimize: lookup that kind of cell repetitively when its number of candidates changes (decreases): compare against the minimum value from a variable (where to put it?)
                minCandidatesCellPos = FindCellWithMinimumCandidates(Grid).Position;// TODO: rename with FIRST
            }
            var minCandidatesCell = Grid.Cells[minCandidatesCellPos.Value.Y, minCandidatesCellPos.Value.X];
            
            byte assumedNumber = minCandidatesCell.Candidates.ToArray()[0];

            assumptionsGrid = (SudokuGrid)Grid.GetType()
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, 
                null, CallingConventions.HasThis, new[] { typeof(SudokuGridConstraints) }, null)
                .Invoke(new [] { (object)Grid.Constraints });

            // BUG: !!! as we use the standard grid here, which was not initially intended to be used anywhere except for decode-and-construct scenarios, by the time we reach this code the assumptionsGrid's cells are not initialized yet -- consider introducing some sort of temporary 'grid' (most probably, a kind of 'cells-only layer'). For that reason one of the ctor's code sections was moved into the Reset() method
            var assumptionsGridSolver = new SudokuSolvingIterationAssumptionTechnique(assumptionsGrid);

            // WI: cell-to-cell iteration is a very intensive process -- use cells array cloning instead with targeted minCandidatesCell... altering
            var minCandidatesCellPosCopy = minCandidatesCellPos.Value;
            assumptionsGrid.IterateLinesXY((x, y) =>
            {
                SudokuGridCell currCell;
                var currPos = new SudokuGridPosition(x, y, false);
                if (currPos.Equals(minCandidatesCellPosCopy))
                {
                    currCell = new SudokuGridCell(assumptionsGrid, currPos, assumedNumber);
                    assumptionsGridSolver.lastFilledPos = currPos;
                    assumptionsGridSolver.currCheckingPos = currPos;
                    assumptionsGridSolver.currCheckingPos.Shift(1, assumptionsGrid.Metrics);
                }
                else
                {
                    currCell = new SudokuGridCell(assumptionsGrid, currPos, Grid.Cells[y, x].Number);
                    if (currCell.Candidates != null)
                    {
                        // just to bring into accord with the latest value from the 'parent' grid
                        currCell.Candidates.IntersectWith(Grid.Cells[y, x].Candidates);
                    }
                }
                SudokuGrid.AssignNewCell(assumptionsGrid, currCell);
                //assumptionsGrid.Cells[y, x] = currCell;
            });

            //List<SudokuGridCell[,]> assumptionGridSolutuons = assumptionGridSolver.SearchSolutions();
            //if (assumptionGridSolutuons.Count > 0) // correct assumption was made
            //{
            //    return assumptionsGrid.Cells;
            //}

            //SudokuGridCell[,] solution;
            if (assumptionsGridSolver.SearchSolutionDebug()) // correct assumption was made
            {
                //return solution;
                return assumptionsGrid.Cells;
            }
            
            // incorrect assumption was made (executes on contradiction only)
            minCandidatesCell.Candidates.Remove(assumedNumber);
            // if there were only two candidates, with assumedNumber being one of them, then the remaining one is a clue
            if (minCandidatesCell.IsClue) 
            {
                minCandidatesCellPos = null;
                var newPos = new SudokuGridPosition(minCandidatesCell.Position.X, minCandidatesCell.Position.Y, false);
                lastFilledPos = newPos;
                currCheckingPos = newPos;
                currCheckingPos.Shift(1, Grid.Metrics);
            }
            return null;
        }
        
        private SudokuGridCell FindCellWithMinimumCandidates(SudokuGrid grid)
        {
            byte minCandidates = grid.Metrics.MaximumNumber;   
            SudokuGridCell minCandidatesCell = null;
            grid.IterateLinesXY((x, y) =>
            {
                var currCell = grid.Cells[y, x];
                if (currCell.Candidates != null &&
                    currCell.Candidates.Count < minCandidates)
                {
                    minCandidates = (byte)currCell.Candidates.Count;
                    minCandidatesCell = currCell;
                }
            });
            return minCandidatesCell;
        }
    }
}