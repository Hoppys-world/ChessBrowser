using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
    internal class ChessGame
    {
        public string Event { get; set; }
        public string Site { get; set; }
        public string Date { get; set; }
        public double Round {  get; set; }
        public string whitePlayer { get; set; }
        public string blackPlayer { get; set; }
        public char Result { get; set; }
        public int whiteElo {  get; set; }
        public int blackElo { get; set;}
        public string eventDate { get; set; }
        public string moveList { get; set; }

    }
}
