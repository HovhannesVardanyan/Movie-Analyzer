using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
namespace MovieAnalyzer
{
    /// <summary>
    /// 
    /// This is class designed to divide actors into connected clusters. Two actors are connected, if they have played in the same movie.
    /// 
    /// </summary>
    class MovieAnalyzer
    {
        private Graph<string, string, string> graph = new Graph<string, string, string>();
        public MovieAnalyzer(string file) // takes the name of the file, which contains a list of movie names
        {
            FillTheGraph(ParseToActors(DownloadJsons(ReadFile(file))));
        }
        private void MakeConnections(List<List<string>> lst) // connects Node(Actors), if they have played in the same movie
        {
            foreach(var movie in lst)
            {
                for (int i = 0; i < movie.Count - 1; i++)
                {
                    for(int j = i + 1; j < movie.Count - 1;j++)
                    {
                        graph.ConnectNodes(movie[i],movie[j],movie[movie.Count - 1]);
                    }
                }
            }
        }
        private void FillTheGraph(List<List<string>> list) // creates nodes with the names of actors
        {
            foreach (List<string> movie in list)
            {
                for (int i = 0; i < movie.Count - 1; i++)
                {
                    if (!graph.ExistsNode(movie[i]))
                        graph.AddNode(movie[i], movie[movie.Count - 1]);
                    else if(!graph[movie[i]].Contains(movie[movie.Count - 1]))
                        graph.ChangeValue(movie[i], graph[movie[i]] + "," + movie[movie.Count - 1]);
                }
            }
            MakeConnections(list);
        }
        private List<string> ReadFile(string file) // Gets the movie names from the specified file
        {
            List<string> lst = new List<string>();

            using (StreamReader sr = new StreamReader(file))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string name = line.Substring(5, line.Length - 5).Trim().Replace(' ','+');

                    lst.Add(name);
                }
            }
            return lst;
        }
        private List<List<string>> ParseToActors(List<string> jsons) // parses the data obtained as json files into a string of info
        {
            List<List<string>> ans = new List<List<string>>();

            foreach (string json in jsons)
            {
                ImdbEntity  e = JsonConvert.DeserializeObject<ImdbEntity>(json);
                var actors = e.Actors.Split(',').ToList();
                actors.Add(e.Genre);
                ans.Add(actors);
            }

            return ans;
        }
        private List<string> DownloadJsons(List<string> titles) // downloads info about specified movies using OMDB API
        {
            Console.WriteLine("Downloading ... ");
            WebClient wc = new WebClient();
            List<string> jsons = new List<string>();
            foreach (var title in titles)
            {
                jsons.Add(wc.DownloadString($"http://www.omdbapi.com/?t={title}&apikey=3921b7ac"));
            }
            Console.WriteLine("Donwloading Complete");
            return jsons;
        }
        public void AnalyzeComponents(int maxSize,string directory) // the main method that does all the clustering
        {
            Console.WriteLine("Analyzing...");
            Graph<string, string, string>.SubGroupsParallel(graph, maxSize,directory);
            Process.Start(Directory.GetCurrentDirectory() + "\\" + directory);
        }
        public override string ToString()
        {
            return graph.ToString();
        }
    }
    class ImdbEntity
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public string Metascore { get; set; }
        public string imdbRating { get; set; }
        public string imdbVotes { get; set; }
        public string imdbID { get; set; }
        public string Type { get; set; }
        public string Response { get; set; }
    }

}
