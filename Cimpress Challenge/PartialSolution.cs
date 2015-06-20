using System.Collections.Generic;
using System.Linq;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     Represents a partial solution and provides methods to efficiently fix variables in that partial solution
    /// </summary>
    public class PartialSolution
    {
        private readonly List<Variable> _activeVariables;

        /// <summary>
        ///     Creates a new partial solution
        /// </summary>
        /// <param name="numberOfVariables">The number of variables this partial solution can hold</param>
        public PartialSolution(int numberOfVariables)
        {
            _activeVariables = new List<Variable>();
            UsageOfVariables = new VariableState[numberOfVariables];
            FreeVariables = numberOfVariables;
        }

        /// <summary>
        ///     The state in the solution for each variable
        /// </summary>
        private VariableState[] UsageOfVariables { get; set; }

        /// <summary>
        ///     Gets the index of the variable in the sorted variable list which represents the largest unused square
        /// </summary>
        public int LargestUnusedSquareIndex { get; set; }

        /// <summary>
        ///     Gets the active (fixed to 1) variables
        /// </summary>
        public List<Variable> ActiveVariables
        {
            get { return _activeVariables; }
        }

        /// <summary>
        ///     Returns the number of free variables
        /// </summary>
        public int FreeVariables { get; private set; }

        /// <summary>
        ///     Fixes the given variable to 1 in this partial solution
        /// </summary>
        /// <param name="variable">The given variable</param>
        /// <param name="ensureNoOverUsage">If set to true, then the variable only gets fixed, if it is unused</param>
        /// <returns></returns>
        public bool FixUp(Variable variable, bool ensureNoOverUsage = false)
        {
            if (ensureNoOverUsage && UsageOfVariables[variable.IndexNumber] != VariableState.Unused)
                return false;
            RunImplications(new List<Variable> {variable}, new List<Variable>());
            return true;
        }

        /// <summary>
        ///     Fixes the given variable to 0 in this partial solution
        /// </summary>
        /// <param name="variable">The given variable</param>
        /// <param name="ensureNoOverUsage">If set to true, then the variable only gets fixed, if it is unused</param>
        /// <returns></returns>
        public bool FixDown(Variable variable, bool ensureNoOverUsage = false)
        {
            if (ensureNoOverUsage && UsageOfVariables[variable.IndexNumber] != VariableState.Unused)
                return false;
            RunImplications(new List<Variable>(), new List<Variable> {variable});
            return true;
        }

        /// <summary>
        ///     Fixing a variable has implications, the most obvious one is that a lot of variables get fixed to 0 when one
        ///     variable gets fixed to 1 (when they share the same constraint/cell)
        /// </summary>
        /// <param name="fixUp">A list of variables that should get fixed up</param>
        /// <param name="fixDown">A list of variables that should get fixed down</param>
        private void RunImplications(List<Variable> fixUp, List<Variable> fixDown)
        {
            while (fixUp.Count != 0 || fixDown.Count != 0)
            {
                for (int i = 0; i < fixUp.Count; i++)
                {
                    UsageOfVariables[fixUp[i].IndexNumber] = VariableState.Basis;
                    FreeVariables--;
                    foreach (Cell instanceCell in fixUp[i].CoveredCells)
                    {
                        if (fixUp[i].Size > 1)
                            fixDown.AddRange(instanceCell.CoveringVariables.Except(new List<Variable> {fixUp[i]}));
                    }
                    fixDown = fixDown.Distinct().ToList();
                    ActiveVariables.Add(fixUp[i]);
                    fixUp.Remove(fixUp[i]);
                }
                for (int i = 0; i < fixDown.Count; i++)
                {
                    UsageOfVariables[fixDown[i].IndexNumber] = VariableState.Forbidden;
                    FreeVariables--;
                    fixDown.Remove(fixDown[i]);
                }
            }
            while (LargestUnusedSquareIndex < UsageOfVariables.Length &&
                   UsageOfVariables[LargestUnusedSquareIndex] != VariableState.Unused)
            {
                LargestUnusedSquareIndex++;
            }
        }
    }

    /// <summary>
    ///     Indicates the state of a variable
    /// </summary>
    internal enum VariableState : byte
    {
        /// <summary>
        ///     Not used so far
        /// </summary>
        Unused = 0,

        /// <summary>
        ///     In the basis of the solution, i.e. fixed to 1
        /// </summary>
        Basis = 1,

        /// <summary>
        ///     Forbidden in the solution, i.e. fixed to 0
        /// </summary>
        Forbidden = 2
    }
}