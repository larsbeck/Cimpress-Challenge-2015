using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     Tracks information about a started process
    /// </summary>
    internal class ProcessInfo
    {
        /// <summary>
        ///     The path to the Lp file
        /// </summary>
        public string PathToLp { get; set; }

        /// <summary>
        ///     Indicates whether this lp file yields a global optimum
        /// </summary>
        public bool ProvidesGlobalOptimum { get; set; }
    }

    /// <summary>
    ///     The solver class to solve Cimpress instances
    /// </summary>
    public class Solver
    {
        /// <summary>
        ///     Variables by name is needed to map the solution from cbc to our variables
        /// </summary>
        private readonly Dictionary<string, Variable> _allVariablesByName;

        /// <summary>
        ///     The cimpress grid instance
        /// </summary>
        private readonly Instance _instance;

        /// <summary>
        /// An instance helper
        /// </summary>
        private readonly InstanceHelper _instanceHelper;

        /// <summary>
        /// Stores the first part of the lp file in a string
        /// </summary>
        private string _firstPartOfLp;

        /// <summary>
        /// Stores the last part of the lp file in a string
        /// </summary>
        private string _lastPartOfLp;

        /// <summary>
        /// Indicates whether the allowed time is up
        /// </summary>
        private bool _timeIsUp;

        /// <summary>
        /// Stores a list of processes with additional information about the process
        /// </summary>
        private Dictionary<Process, ProcessInfo> processes;

        /// <summary>
        /// The timer used to notify the solver when to gracefully stop
        /// </summary>
        private Timer timer;

        /// <summary>
        ///     Creates a new solver instance
        /// </summary>
        /// <param name="instance">The cimpress grid instance</param>
        public Solver(Instance instance)
        {
            //we have 10 seconds, setting a timer telling us when to abort
            timer = new Timer(9800);
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            //store instance
            _instance = instance;

            //a datastructure to help building the model
            _instanceHelper = new InstanceHelper(instance);

            // Loop through the grid
            for (int row = 0; row < instance.Height; row++)
            {
                for (int column = 0; column < instance.Width; column++)
                {
                    // Do nothing if cell is an obstacle
                    if (!instance.Puzzle[row][column]) continue;
                    int maxSize = GetMaxSquareSize(instance, row, column);

                    // Create binary variable for every square size 
                    for (int i = 1; i <= maxSize; i++)
                    {
                        var variable = new Variable(string.Format("var_{0}_{1}_{2}_end", row, column, i), column, row, i);
                        _instanceHelper.AddVariable(variable);

                        // Remember coverable cells for each variable 
                        for (int vx = column; vx < column + i; vx++)
                        {
                            for (int vy = row; vy < row + i; vy++)
                            {
                                // Do nothing if cell is an obstacle
                                if (!instance.Puzzle[vy][vx]) continue;
                                _instanceHelper.Cells[vx, vy].CoveringVariables.Add(variable);
                                variable.CoveredCells.Add(_instanceHelper.Cells[vx, vy]);
                            }
                        }
                    }
                }
            }
            _allVariablesByName = _instanceHelper.Variables.ToDictionary(variable => variable.Name, variable => variable);
        }

        /// <summary>
        ///     The currently best solution
        /// </summary>
        public Solution BestSolution { get; set; }

        /// <summary>
        ///     Indicates that a global optimum was found
        /// </summary>
        public bool FoundGlobalOptimum { get; set; }

        /// <summary>
        ///     Returns the first part of the lp file contents
        /// </summary>
        private string GetFirstPartOfLp
        {
            get
            {
                if (_firstPartOfLp == null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Minimize");

                    //Symmetry breaking; this didn't help speeding up the solver, but is left here as a note to the interested reader
                    double divisor = _instance.Height * _instance.Height * _instance.Width +
                                     _instance.Height * _instance.Width +
                                     Math.Max(_instance.Width, _instance.Height);

                    //objective
                    sb.Append("squares: ");
                    int maxIndex = _instanceHelper.Variables.Count - 1;
                    for (int i = 0; i < _instanceHelper.Variables.Count; i++)
                    {
                        int factor = 1;
                        // + (((allVariables[i].X * _instance.Height * _instance.Width) + (allVariables[i].Y * _instance.Width) + allVariables[i].Size) / divisor);
                        if (i == maxIndex)
                        {
                            sb.AppendLine(string.Format("{0} {1}", factor, _instanceHelper.Variables[i].Name));
                        }
                        else
                        {
                            sb.AppendFormat("{0} {1} + ", factor, _instanceHelper.Variables[i].Name);
                        }
                    }

                    //constraints
                    sb.AppendLine("Subject to");
                    _firstPartOfLp = sb.ToString();
                }
                return _firstPartOfLp;
            }
        }

        /// <summary>
        ///     Returns the last part of the lp file
        /// </summary>
        private string GetLastPartOfLp
        {
            get
            {
                if (_lastPartOfLp == null)
                {
                    var sb = new StringBuilder();
                    //Loop through the grid
                    for (int row = 0; row < _instance.Height; row++)
                    {
                        for (int column = 0; column < _instance.Width; column++)
                        {
                            //Skip obstacles
                            if (!_instance.Puzzle[row][column]) continue;
                            //All covering variables of a cell will be in one constraint
                            List<Variable> variables = _instanceHelper.Cells[column, row].CoveringVariables;
                            if (variables == null) continue;
                            int maxIndexForVariables = variables.Count - 1;
                            sb.Append(string.Format("C{0}_{1}: ", row, column));

                            for (int i = 0; i < variables.Count; i++)
                            {
                                if (i == maxIndexForVariables)
                                {
                                    //the sum of all these variables has to be 1
                                    sb.AppendLine(variables[i].Name + " = 1");
                                }
                                else
                                {
                                    sb.Append(variables[i].Name + " + ");
                                }
                            }
                        }
                    }

                    //There are 10 types of variables, those who understand binary and those who don't - just kidding: definition of binary section in lp format
                    sb.AppendLine("Binary");
                    sb.AppendLine(string.Concat(_instanceHelper.Variables.Select(variable => variable.Name + " ")));
                    sb.AppendLine("End");
                    _lastPartOfLp = sb.ToString();
                }
                return _lastPartOfLp;
            }
        }

        /// <summary>
        ///     Computes the maximal size of a square starting at the given coordinates x and y
        /// </summary>
        /// <param name="instance">The cimpress grid instance</param>
        /// <param name="row">The row</param>
        /// <param name="column">The column</param>
        /// <returns></returns>
        private int GetMaxSquareSize(Instance instance, int row, int column)
        {
            // Calculate upper bound (maximal square size) - start with maximal size to boundaries 
            int maxSize = 1;
            while (maxSize <= Math.Min(instance.Width - column, instance.Height - row))
            {
                for (int tx = column; tx < column + maxSize; tx++)
                    if (!instance.Puzzle[row + maxSize - 1][tx]) return maxSize - 1;
                for (int ty = row; ty < row + maxSize; ty++)
                    if (!instance.Puzzle[ty][column + maxSize - 1]) return maxSize - 1;
                maxSize++;
            }
            return maxSize - 1;
        }

        /// <summary>
        ///     This delegate is fired when the time is up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timeIsUp = true;
        }

        /// <summary>
        ///     Solves the instance given in the constructor
        /// </summary>
        /// <returns>A solution to the given instance</returns>
        public async Task<Solution> Solve()
        {
            processes = new Dictionary<Process, ProcessInfo>();
            int i = -1;
            while (BestSolution == null || (!_timeIsUp && !FoundGlobalOptimum))
            {
                while (processes.Count < int.Parse(ConfigurationManager.AppSettings["NumberOfCbcInstances"]))
                {
                    //-1 indicates the first run which corresponds to the full model yielding the global optimum
                    List<Variable> fixedVariables;
                    if (i == -1)
                    {
                        fixedVariables = new List<Variable>();
                        i++;
                    }
                    // all models created here are heuristic solutions due to variable fixing
                    else
                    {
                        //if the counter advanced this far, we have tried all heuristic solutions we wanted -> will not occur for reasonably large models
                        if (_instanceHelper.VariablesBySize.Count == i)
                            break;

                        var chromosome = new PartialSolution(_instanceHelper.VariableCounter);
                        //for every new run, fix another variable
                        chromosome.FixUp(_instanceHelper.VariablesBySize.Skip(i++).First(), true);
                        //fixing as many variables as needed to get solution time down to 10 seconds
                        while (chromosome.FreeVariables > 4500)
                        {
                            chromosome.FixUp(_instanceHelper.VariablesBySize[chromosome.LargestUnusedSquareIndex], true);
                        }
                        fixedVariables = chromosome.ActiveVariables;
                    }
                    //write lp file
                    string pathToFile = Path.Combine(ConfigurationManager.AppSettings["LpDirectory"],
                        string.Format("{0}_{1}.lp", _instance.Id, Guid.NewGuid()));
                    File.WriteAllText(pathToFile, ToLp(fixedVariables));

                    //start cbc
                    var startInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = Path.Combine(ConfigurationManager.AppSettings["PathToCbc"], "cbc.exe"),
                        WindowStyle = ProcessWindowStyle.Normal,
                        Arguments = string.Format("{0} printing csv allow 0.9999 cuts root probing root solve solution {0}.solution", pathToFile)
                    };
                    // Start the process with the info we specified.
                    Process exeProcess = Process.Start(startInfo);
                    processes.Add(exeProcess,
                        new ProcessInfo { PathToLp = pathToFile, ProvidesGlobalOptimum = fixedVariables.Count == 0 });
                }

                //get the finished processes
                foreach (var process in processes.Where(pair => pair.Key.HasExited).ToList())
                {
                    //set the flag to stop the solution process because the global optimum was found
                    if (process.Value.ProvidesGlobalOptimum)
                        FoundGlobalOptimum = true;

                    string pathToLp = processes[process.Key].PathToLp;
                    //Delete lp file
                    File.Delete(pathToLp);
                    string pathToSolutionFile = pathToLp + ".solution";
                    //read the csv file
                    if (File.Exists(pathToSolutionFile))
                    {
                        string[] allLines = File.ReadAllLines(pathToSolutionFile);
                        List<Variable> variables =
                            //skip the header
                            allLines.Skip(1)
                            //split variable names and value in the optimal solution
                                .Select(s => s.Split(','))
                            //we are only interested in the ones that are in the basis
                                .Where(s1 => s1[1] == "1")
                            //lookup the name of the variables in the basis
                                .Select(strings => _allVariablesByName[strings[0]])
                                .ToList();
                        //if the solution is better than what we have so far, replace the current solution with this one
                        if (BestSolution == null || variables.Count < BestSolution.squares.Count)
                        {
                            BestSolution = new Solution(_instance, variables);
                        }
                    }
                    //Delete the solution file
                    File.Delete(pathToSolutionFile);
                    processes.Remove(process.Key);
                    process.Key.Dispose();
                }
                //wait for 20 ms
                await Task.Delay(20);
            }

            //solving is done (either because the time is up and we have a solution, or because we found the global optimum), now kill all running processes
            foreach (var process in processes.ToList())
            {
                try
                {
                    bool processStillRunning = !process.Key.HasExited;
                    if (processStillRunning)
                    {
                        process.Key.Kill();
                        File.Delete(process.Value.PathToLp);
                    }
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Process disposed. Cannot read its state.");
                }
                catch (Exception)
                {
                    Debug.WriteLine("Could not delete file " + process.Value.PathToLp);
                }
            }
            return BestSolution;
        }

        /// <summary>
        ///     Get the Lp representation of the model
        /// </summary>
        /// <param name="fixedVariables">The variables that should be fixed to 1</param>
        /// <returns>The Lp representation of the model</returns>
        public string ToLp(IEnumerable<Variable> fixedVariables = null)
        {
            var sb = new StringBuilder();
            sb.Append(GetFirstPartOfLp);

            if (fixedVariables != null)
            {
                //Fixing variables
                foreach (Variable fixedVariable in fixedVariables)
                {
                    sb.AppendLine("Fix: " + fixedVariable.Name + " = 1");
                }
            }
            sb.Append(GetLastPartOfLp);
            return sb.ToString();
        }
    }
}