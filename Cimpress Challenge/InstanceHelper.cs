using System.Collections.Generic;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     A helper class that holds data about the instance and ensures efficient access
    /// </summary>
    public class InstanceHelper
    {
        /// <summary>
        ///     An internal sorted list of variables -> used to get a flattened sorted list VariablesBySize
        /// </summary>
        private readonly SortedList<int, List<Variable>> _sortedVariables =
            new SortedList<int, List<Variable>>(Comparer<int>.Create((first, second) => second.CompareTo(first)));

        private List<Variable> _variablesBySize = new List<Variable>();

        /// <summary>
        ///     Creates an InstanceHelper object for the given instance
        /// </summary>
        /// <param name="instance">The given instance</param>
        public InstanceHelper(Instance instance)
        {
            Instance = instance;
            Variables = new List<Variable>();
            Cells = new Cell[instance.Width, instance.Height];
            for (int i = 0; i < instance.Width; i++)
            {
                for (int j = 0; j < instance.Height; j++)
                {
                    Cells[i, j] = new Cell(i, j);
                }
            }
        }

        /// <summary>
        ///     The instance from the REST service
        /// </summary>
        public Instance Instance { get; set; }

        public Cell[,] Cells { get; set; }

        /// <summary>
        ///     The list of all variables in insertion order -> this is important due to best solver performance in terms of
        ///     variabilty experiment
        /// </summary>
        public List<Variable> Variables { get; set; }

        /// <summary>
        ///     A flattened list of all variables sorted by size
        /// </summary>
        public List<Variable> VariablesBySize
        {
            get
            {
                if (_variablesBySize.Count != VariableCounter)
                {
                    _variablesBySize = new List<Variable>();
                    int i = 0;
                    foreach (var pair in _sortedVariables)
                    {
                        foreach (Variable geneVariable in pair.Value)
                        {
                            geneVariable.IndexNumber = i++;
                            _variablesBySize.Add(geneVariable);
                        }
                    }
                }
                return _variablesBySize;
            }
            set { _variablesBySize = value; }
        }

        /// <summary>
        ///     The number of variables
        /// </summary>
        public int VariableCounter { get; set; }

        /// <summary>
        ///     Adds a variable and updates all datastructures
        /// </summary>
        /// <param name="variable">The variable to add</param>
        public void AddVariable(Variable variable)
        {
            List<Variable> outValue;
            _sortedVariables.TryGetValue(variable.Size, out outValue);
            if (outValue == null)
            {
                outValue = new List<Variable>();
                _sortedVariables.Add(variable.Size, outValue);
            }
            outValue.Add(variable);
            Variables.Add(variable);
            VariableCounter++;
        }
    }
}