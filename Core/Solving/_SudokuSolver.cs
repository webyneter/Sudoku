using System;
using System.Collections.Generic;
using System.Linq;

using SudokuCommons.Desks;


namespace SudokuCommons.Solving
{
    /// <summary>
    /// Represents various solving algorithms 
    /// for the particular desk class instance. 
    /// </summary>
    internal class _SudokuSolver 
    {
        public SudokuDeskCell.NumberChangedDelegate CellNumberChangedHandler { get; }
        public SudokuDesk OwnerDesk { get; }

        private ushort emptyCellsCount;
        private SortedSet<byte>[] rowsWithNumbers;
        private SortedSet<byte>[] columnsWithNumbers;
        private SortedSet<byte>[,] blocksWithNumbers;
        private SudokuDesk testDesk;
        private SudokuDeskPosition currCheckingPos;
        private SortedSet<byte> forbiddenVariants;
        private SudokuDeskPosition? lastFilledPos;

        /// <summary>
        /// Initializes a new instance of solver 
        /// and links it to the specified desk.
        /// </summary>
        /// <param name="ownerDesk"></param>
        public _SudokuSolver(SudokuDesk ownerDesk)
        {
            OwnerDesk = ownerDesk;
            emptyCellsCount = ownerDesk.Metrics.CellsTotal;
            CellNumberChangedHandler = (sender) => emptyCellsCount--;
            CellNumberChangedHandler += RegisterForbiddenNumberFrom;
            rowsWithNumbers = new SortedSet<byte>[ownerDesk.Metrics.MaximumNumber];
            columnsWithNumbers = new SortedSet<byte>[ownerDesk.Metrics.MaximumNumber];
            ownerDesk.IterateLine((i) =>
            {
                rowsWithNumbers[i] = new SortedSet<byte>();
                columnsWithNumbers[i] = new SortedSet<byte>();
            });
            blocksWithNumbers = new SortedSet<byte>[
                ownerDesk.Metrics.MaximumNumber / ownerDesk.Metrics.BlockHeight,
                ownerDesk.Metrics.MaximumNumber / ownerDesk.Metrics.BlockWidth];
            ownerDesk.IterateBlocks((i, j) =>
            {
                blocksWithNumbers[i, j] = new SortedSet<byte>();
            });
        }


        public bool Solve(SudokuSolvingTechnique technique)
        {
            SudokuDeskCell[,] result = technique.ApplyTo(OwnerDesk);
            if (result == null)
            {
                return false;
            }
            OwnerDesk.IterateLines((i, j) => OwnerDesk.Cells[i, j] = result[i, j]);
            return true;
        }
        


        /// <summary>
        /// The desk is solved by iteration-and-assumption.
        /// </summary>
        /// <returns>Returns true if the solution was found. Otherwise returns false.</returns>
        public bool SolveByIA()
        {
            // currCheckingPos is (0, 0) by default and is changed during testing only
            forbiddenVariants = new SortedSet<byte>();

            SudokuDeskCell[,] result = DoSolveByIA();
            
            if (result == null)
            {
                return false;
            }
            
            OwnerDesk.IterateLines((i, j) => OwnerDesk.Cells[i, j] = result[i, j]);
            return true;
        }


        // Алгоритм: последовательный проход по клеткам слева направо и вниз
        // и усечение мн-ва вариантов цифр, 
        // которые могут находиться в данной клетке; единственный
        // оставшийся вариант подставляется в клетку. Каждый раз,
        // как совершается повторный обход доски, должна заполняться
        // хотя бы одна клетка. Иначе алгоритм зацикливается,
        // и запускается другой алгоритм:
        // в первую найденную клетку, имеющую наименьшее кол-во вариантов,
        // подставляется наименьший из них (несущественно) и поиск продолжается
        // на специальной доске testDesk по исходному
        // алгоритму: если полученное решение непротиворечиво, то
        // оно становится окончательным. Иначе: а) если получаем 
        // противоречие, то исходная подстановка неверна и, 
        // вернувшись к точке осущствления подстановки, 
        // подставляем другую цифру, вновь применяя исохдный алгоритм;
        // б) если получаем зацикливание, то снова находим эл-т 
        // с наименьшим кол-вом вариантов, подставляем один из них 
        //и применяем исходный алгоритм.
        private SudokuDeskCell[,] DoSolveByIA()
        {
            bool looped;
            bool contradicted;
            SudokuDeskPosition? minVarsCellPos = null;
            SudokuDeskCell[,] result;
            while (emptyCellsCount > 0)
            {
                MakeException(out looped, out contradicted);
                if (looped)
                {
                    result = MakeAssumption(ref minVarsCellPos);
                    if (result != null)
                    {
                        return result;   
                    }
                }
                else if (contradicted)
                {
                    return null;
                }
            }
            // for validation only
            return OwnerDesk.Cells;
        }

        private void MakeException(out bool looped, out bool contradicted)
        {
            looped = false;
            contradicted = false;
            SudokuDeskPosition currCellPos = currCheckingPos;
            currCheckingPos.Shift(1, OwnerDesk.Metrics);
            if ((lastFilledPos == null) && currCellPos.CrossedDeskBorder 
                || lastFilledPos.HasValue && lastFilledPos.Value.Equals(currCellPos))
            {
                looped = true;
                return;
            }
            SudokuDeskCell currCell = OwnerDesk.Cells[currCellPos.Y, currCellPos.X];
            if (currCell.Number == 0)
            {
                byte x, y;
                DetermineBlock(currCell, out x, out y);
                forbiddenVariants.UnionWith(rowsWithNumbers[currCellPos.Y]);
                forbiddenVariants.UnionWith(columnsWithNumbers[currCellPos.X]);
                forbiddenVariants.UnionWith(blocksWithNumbers[y, x]);
                currCell.Variants.ExceptWith(forbiddenVariants);
                forbiddenVariants.Clear();
                if (currCell.ContradictionFound)
                {
                    contradicted = true;
                    return;
                }
                if (currCell.Number != 0)
                {
                    lastFilledPos = currCell.Position;    
                }
            }
        }

        private void DetermineBlock(SudokuDeskCell cell, out byte x, out byte y)
        {
            x = (byte)(cell.Position.X / cell.OwnerDesk.Metrics.BlockWidth);
            y = (byte)(cell.Position.Y / cell.OwnerDesk.Metrics.BlockHeight);
        }

        private void RegisterForbiddenNumberFrom(SudokuDeskCell cell)
        {
            rowsWithNumbers[cell.Position.Y].Add(cell.Number);
            columnsWithNumbers[cell.Position.X].Add(cell.Number);
            byte x, y;
            DetermineBlock(cell, out x, out y);
            blocksWithNumbers[y, x].Add(cell.Number);
        }
        
        private SudokuDeskCell[,] MakeAssumption(ref SudokuDeskPosition? minVarsCellPos)
        {
            SudokuDeskCell currCell;
            SudokuDeskPosition currPos;
            if (minVarsCellPos == null)
            {
                minVarsCellPos = FindCellWithMinimumVariantsIn(OwnerDesk).Position;    
            }
            SudokuDeskCell minCell = OwnerDesk.Cells[minVarsCellPos.Value.Y, minVarsCellPos.Value.X];
            byte assumedNumber = minCell.Variants.ToArray()[0];
            testDesk = (SudokuDesk)OwnerDesk.GetType()
                .GetConstructor(new []{ typeof(SudokuDeskRestrictions) })
                .Invoke(new [] { (object)OwnerDesk.Restrictions });
            SudokuDeskPosition minVarsCellPosCopy = minVarsCellPos.Value;
            testDesk.IterateLines((i, j) =>
            {
                currPos = new SudokuDeskPosition(j, i);
                if (!currPos.Equals(minVarsCellPosCopy))
                {
                    testDesk.Cells[i, j] = new SudokuDeskCell(testDesk, currPos, 
                        OwnerDesk.Cells[i, j].Number);
                    currCell = testDesk.Cells[i, j];
                    if (currCell.Variants != null)
                    {
                        currCell.Variants.IntersectWith(OwnerDesk.Cells[i, j].Variants);   
                    }
                }
                else
                {
                    testDesk.Cells[i, j] = new SudokuDeskCell(testDesk, currPos, 
                        assumedNumber);
                    testDesk.GetSolver().lastFilledPos = currPos;
                    testDesk.GetSolver().currCheckingPos = currPos;
                    testDesk.GetSolver().currCheckingPos.Shift(1, testDesk.Metrics);
                }
            });

            if (testDesk.GetSolver().SolveByIA()) // correct assumption was made
            {
                return testDesk.Cells;
            }
            // incorrect assumption was made (executes on contradiction only)
            minCell.Variants.Remove(assumedNumber);
            if (minCell.Number != 0) // new number acquired
            {
                minVarsCellPos = null;
                lastFilledPos = minCell.Position;
                currCheckingPos = minCell.Position;
                currCheckingPos.Shift(1, OwnerDesk.Metrics);
            }

            return null;
        }

        private SudokuDeskCell FindCellWithMinimumVariantsIn(SudokuDesk desk)
        {
            byte minVariants = desk.Metrics.MaximumNumber;
            SudokuDeskCell currCell;
            SudokuDeskCell result = null;
            desk.IterateLines((i, j) =>
            {
                currCell = desk.Cells[i, j];

                if ((currCell.Variants != null) &&
                    (currCell.Variants.Count < minVariants))
                {
                    minVariants = (byte)currCell.Variants.Count;
                    result = currCell;
                }
            });
            return result;
        }
    }
}