using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChessBrowser
{
    internal class PgnReader
    {
        public List<ChessGame> getChessGames(String fileName)
        {
            List<ChessGame> games = new List<ChessGame>();
            using (StreamReader read = new StreamReader(fileName))
            {
                string? line = read.ReadLine();
                //if not null create new game and add data
                while(line != null)
                {
                    ChessGame game = new ChessGame();
                    //loop until blank line appears
                    while (line != null && line.Length != 0)
                    {
                        //match for the Tag goes, until space
                        Match matchCollection = Regex.Match(line, @"[[a-zA-Z]+\s");
                        if (matchCollection.Success)
                        {
                            
                            string tag = matchCollection.Value.Substring(1, matchCollection.Length-2);
                            //get info other than tag
                            string test = line.Substring(tag.Length + 3);
                            string data = test.Substring(0, test.Length - 2);

                            switch (tag)
                            {
                                case "Event":
                                    game.Event = data;
                                    break;
                                case "Site":
                                    game.Site = data;
                                    break;

                                case "Date":
                                    game.Date = data.Replace(".", "-");
                                    break;

                                case "Round":
                                    game.Round = double.Parse(data);
                                    break;

                                case "White":
                                    game.whitePlayer = data;
                                    break;

                                case "Black":
                                    game.blackPlayer = data;
                                    break;

                                case "Result":
                                    switch (data)
                                    {
                                        case "1/2-1/2":
                                            game.Result = 'D';
                                            break;
                                        case "1-0":
                                            game.Result = 'W';
                                            break;
                                        default:
                                            game.Result = 'B';
                                            break;
                                    }
                                    break;

                                case "WhiteElo":
                                    game.whiteElo = int.Parse(data);
                                    break;

                                case "BlackElo":
                                    game.blackElo = int.Parse(data);
                                    break;

                                case "EventDate":
                                    game.eventDate = data;
                                    break;

                                default:
                                    break;
                            }
                            line = read.ReadLine();
                        }
                    }
                    //if space is found then read next line and get move list
                    StringBuilder sb = new StringBuilder();
                    line = read.ReadLine();
                    while (line != null && line.Length != 0)
                    { 
                        sb.Append(line);
                        line = read.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                    }
                    game.moveList = sb.ToString();
                    games.Add(game);
                    line = read.ReadLine();
                }
            }
                

            return games;
        }

    }
}
