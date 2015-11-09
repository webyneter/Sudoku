# Sudoku
[Sudoku](https://en.wikipedia.org/wiki/Sudoku) puzzles solver.

To get started with demo:

1. Build and run 'Sudoku Solver.exe': welcome message and input request for puzzles-to-solve folder name (relative to the current executable's location) will be displayed.
2. For a list of available puzzles check out the repo root's [input] folder: It contains puzzles' photos as well as the corresponding textual representation, and photos of their solutions (you may use them for solution verification later on) -- copy it to the executable's folder.
3. Fill in the name of the puzzles' folder.
4. Input the number of puzzle to solve from the list displayed.
5. Press <Enter>.
6. A solution will be displayed shortly.

*So far*, the application is capable of:
- Solving classic 9x9 puzzles using my own method: I call it 'Iteration-Assumption Method' which is essentially a combination of well-known techniques, such as One Rule-driven cell candidates decrease, and uncertainty resolution by means of cell candidate assumption (see [implementation](Core/Solving/SudokuSolvingIterationAssumptionTechnique.cs) for more details).
- Compressing a classic 9x9 puzzle into an optimized binary file (one with a '.tradsud' extension) half the size of the original textual version (vice-versa in case of decompression) and beyond (see [implementation](Core/Conversion/SudokuConverter.cs) for more details).

The application is being *actively developed* (take a look at the list of [milestones](https://github.com/webyneter/Sudoku/milestones) and related [issues](https://github.com/webyneter/Sudoku/issues?q=is%3Aissue+is%3Aopen)). By the way, TODOs, BUGs, and other ReSharper-specific idioms are used extensively at the moment and are about to be converted into issues as soon as possible.

Generally, my major goal is to create a fully-functional *online tournament platform* allowing for self-placed/guided puzzle solving training, peer-to-peer/team-to-team competition with support for leaderboards, player proficiency estimation, etc.
