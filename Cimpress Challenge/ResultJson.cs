namespace Cimpress_Challenge
{
    /// <summary>
    ///     The json format which is returned form the Cimpress REST service
    /// </summary>
    public class ResultJson
    {
        /// <summary>
        ///     A list of errors
        /// </summary>
        public string[] errors { get; set; }

        /// <summary>
        ///     The number of squares that was submitted in the solution
        /// </summary>
        public int numberOfSquares { get; set; }

        /// <summary>
        ///     The overall score
        /// </summary>
        public int score { get; set; }

        /// <summary>
        ///     The penalty for overtime (roughly 3 points for every second after 10 seconds)
        /// </summary>
        public int timePenalty { get; set; }
    }
}