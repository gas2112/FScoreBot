using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FScoreBot;
using Newtonsoft.Json;

namespace FScoreAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Gustavo\Treinamento\FScoreBot\FScoreBot\bin\Debug\18-11Qu.json";
            List<GameInfo> gameLib = JsonConvert.DeserializeObject<List<GameInfo>>(File.ReadAllText(jsonPath));

            //gameLib = gameLib.OrderBy(gm => gm.Torneio).ThenBy(gm => gm.Data).ToList();

            List<string> lstTournaments = new List<string>();

            foreach (GameInfo game in gameLib)
            {
                if (!lstTournaments.Contains(game.Torneio)) lstTournaments.Add(game.Torneio);
            }

            Console.WriteLine("STOP");

        }
    }
}
