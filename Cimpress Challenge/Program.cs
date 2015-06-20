using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Cimpress_Challenge
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Make sure the necessary directories exist
            EnsureDirectoriesExist();

            var dateTime = DateTime.UtcNow;
            //get a new instance
            var instance = Instance.GetNewInstance();
            //solve
            var solver = new Solver(instance);
            var solution = solver.Solve().Result;
            //submit solution
            var submitSolution = instance.SubmitSolution(solution);
            //write result to console
            Console.WriteLine("Total Time: {4}, Squares: {0}, Penalty: {1}, Score {2}, Errors: {3}",
                submitSolution.numberOfSquares, submitSolution.timePenalty, submitSolution.score,
                string.Concat(submitSolution.errors), DateTime.UtcNow - dateTime);
            Console.ReadLine();
        }

        private static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(ConfigurationManager.AppSettings["LpDirectory"]))
                Directory.CreateDirectory(ConfigurationManager.AppSettings["LpDirectory"]);
            if (!Directory.Exists(ConfigurationManager.AppSettings["InstanceDirectory"]))
                Directory.CreateDirectory(ConfigurationManager.AppSettings["InstanceDirectory"]);
        }
    }
}