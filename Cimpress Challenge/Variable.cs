using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     A variable in the mixed-integer program
    /// </summary>
    public class Variable
    {
        /// <summary>
        ///     Creates a new variable
        /// </summary>
        /// <param name="name">The name of the variable for the lp file</param>
        /// <param name="x">The x-coordinate in the grid</param>
        /// <param name="y">The y-coordinate in the grid</param>
        /// <param name="size">The size of the corresponding square</param>
        public Variable(string name, int x, int y, int size)
        {
            Name = name;
            X = x;
            Y = y;
            Size = size;
            CoveredCells = new List<Cell>();
        }

        /// <summary>
        ///     The name of the variable in the lp
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        ///     The x-coordinate in the grid
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     The y-coordinate in the grid
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        ///     The size of the square which this variable represents
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     The cells this variable/square covers
        /// </summary>
        [JsonIgnore]
        public List<Cell> CoveredCells { get; set; }

        /// <summary>
        ///     An index which is used to access this variable in a sorted array
        /// </summary>
        [JsonIgnore]
        public int IndexNumber { get; set; }
    }
}