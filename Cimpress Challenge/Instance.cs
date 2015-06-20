using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace Cimpress_Challenge
{
    /// <summary>
    ///     Represents an instance from the Cimpress challenge
    /// </summary>
    public class Instance
    {
        public int width { get; set; }
        public int height { get; set; }
        public bool[][] puzzle { get; set; }
        public string id { get; set; }

        /// <summary>
        ///     The width of the grid
        /// </summary>
        [JsonIgnore]
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        ///     The height of the grid
        /// </summary>
        [JsonIgnore]
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        ///     The grid itself, true represents a free cell, false is an obstacle
        /// </summary>
        [JsonIgnore]
        public bool[][] Puzzle
        {
            get { return puzzle; }
        }

        /// <summary>
        ///     The unique Id of the instance
        /// </summary>
        [JsonIgnore]
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        ///     Allows to read an instance file from the disk
        /// </summary>
        /// <param name="pathToJson">The path to the json file</param>
        /// <returns></returns>
        public static Instance GetInstanceFromJson(string pathToJson)
        {
            return JsonConvert.DeserializeObject<Instance>(File.ReadAllText(pathToJson));
        }

        /// <summary>
        ///     Generates a random grid for the given number of row, columns and percentage of obstacles
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="percentageOfObstacles">
        ///     The percentage of obstacles; a quick analysis showed, that this value follows an f
        ///     distribution
        /// </param>
        /// <returns></returns>
        public static Instance GetRandomInstance(int rows, int columns, double percentageOfObstacles = 0.05)
        {
            var generatedInstance = new Instance {id = string.Format("generated_{0}", Guid.NewGuid())};
            var random = new Random(1);
            generatedInstance.height = rows;
            generatedInstance.width = columns;
            generatedInstance.puzzle = new bool[generatedInstance.height][];
            for (int i = 0; i < generatedInstance.height; i++)
            {
                generatedInstance.puzzle[i] = new bool[generatedInstance.width];
                for (int j = 0; j < generatedInstance.width; j++)
                {
                    generatedInstance.puzzle[i][j] = !(random.NextDouble() <= percentageOfObstacles);
                }
            }
            return generatedInstance;
        }

        /// <summary>
        ///     Request a new instance from the REST service
        /// </summary>
        /// <returns></returns>
        public static Instance GetNewInstance()
        {
            var httpClient = new HttpClient();
            string httpResponseMessage =
                httpClient.GetStringAsync(string.Format("http://techchallenge.cimpress.com/{0}/{1}/puzzle",
                    ConfigurationManager.AppSettings["APIKEY"], ConfigurationManager.AppSettings["Mode"])).Result;
            var instance = JsonConvert.DeserializeObject<Instance>(httpResponseMessage);
            File.WriteAllText(
                Path.Combine(ConfigurationManager.AppSettings["InstanceDirectory"],
                    string.Format("{0}.json", instance.Id)), httpResponseMessage);
            return instance;
        }

        /// <summary>
        ///     Submits a solution
        /// </summary>
        /// <param name="solution">The solution</param>
        /// <returns></returns>
        public ResultJson SubmitSolution(Solution solution)
        {
            var httpClient = new HttpClient();
            HttpResponseMessage httpResponseMessage =
                httpClient.PostAsync(
                    string.Format("http://techchallenge.cimpress.com/{0}/{1}/solution",
                        ConfigurationManager.AppSettings["APIKEY"], ConfigurationManager.AppSettings["Mode"]),
                    new StringContent(JsonConvert.SerializeObject(solution))).Result;

            return JsonConvert.DeserializeObject<ResultJson>(httpResponseMessage.Content.ReadAsStringAsync().Result);
        }
    }
}