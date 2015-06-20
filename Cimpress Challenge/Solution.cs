using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     The solution to a given instance
    /// </summary>
    public class Solution
    {
        /// <summary>
        ///     Creates a solution object
        /// </summary>
        /// <param name="instance">The instance for which the created solution object represents a solution</param>
        /// <param name="variables">The list of variables/squares which make up the solution</param>
        public Solution(Instance instance, List<Variable> variables)
        {
            id = instance.Id;
            squares = variables;
        }

        /// <summary>
        ///     The unique id of the instance to which this is the solution
        /// </summary>
        public string id { get; private set; }

        /// <summary>
        ///     The list of squares which make up the solution
        /// </summary>
        public List<Variable> squares { get; private set; }

        /// <summary>
        ///     Serializes this object to Json
        /// </summary>
        /// <returns>The json string for the Cimpress REST service</returns>
        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}