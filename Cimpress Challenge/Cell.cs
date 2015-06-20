using System.Collections.Generic;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     A cell in the grid - in mixed-integer programming terms, this directly translates to a row or constraint
    /// </summary>
    public class Cell
    {
        /// <summary>
        ///     Creates a cell with the given coordinates
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            CoveringVariables = new List<Variable>();
        }

        /// <summary>
        ///     The x-coordinate of the cell
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     The y-coordinate of the cell
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        ///     Memory of all variables that can cover a certain cell
        /// </summary>
        public List<Variable> CoveringVariables { get; set; }
    }
}