using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FScoreBot
{
    public class GameInfo
    {
        public string Torneio { get; set; }
        public string Data { get; set; }
        public string Mandante { get; set; }
        public string Visitante { get; set; }

        public int PlacarMandante { get; set; }
        public int PlacarVisitante { get; set; }

        public List<ItemForma> FormaMandante { get; set; }
        public List<ItemForma> FormaVisitante { get; set; }

        public string Status { get; set; }

        public List<Odds> OddsPrematch { get; set; }
    }

    public class Odds
    {
        public string Banca { get; set; }
        public double Casa { get; set; }
        public double Empate { get; set; }
        public double Fora { get; set; }

        public Odds (string banca, double c, double e, double f)
        {
            Banca = banca;
            Casa = c;
            Empate = e;
            Fora = f;
        }
    }
}
