﻿using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Gaming.Input;

/*
  Author: Daniel Kopta and ...
  Chess browser backend 
*/

namespace ChessBrowser
{
    internal class Queries
    {

        /// <summary>
        /// This function runs when the upload button is pressed.
        /// Given a filename, parses the PGN file, and uploads
        /// each chess game to the user's database.
        /// </summary>
        /// <param name="PGNfilename">The path to the PGN file</param>
        internal static async Task InsertGameData(string PGNfilename, MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // TODO:
            //       Load and parse the PGN file
            //       We recommend creating separate libraries to represent chess data and load the file
            PgnReader pgnReader = new PgnReader();
            List<ChessGame> games = pgnReader.getChessGames(PGNfilename);

            // TODO:
            //       Use this to tell the GUI's progress bar how many total work steps there are
            //       For example, one iteration of your main upload loop could be one work step
            mainPage.SetNumWorkItems(games.Count);


            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();
                    
                    // TODO:
                    //       iterate through your data and generate appropriate insert commands
                    foreach (ChessGame game in games)
                    {
                        MySqlCommand cmd = new MySqlCommand();
                        //create events
                        cmd.CommandText = "insert ignore into Events(Name, Site, Date) " + "values (@EventName, @Site, @Date);";
                        cmd.Parameters.AddWithValue("@EventName", game.Event);
                        cmd.Parameters.AddWithValue("@Site", game.Site);
                        cmd.Parameters.AddWithValue("@Date", game.eventDate);

                        //create white player
                        MySqlCommand playercmd = new MySqlCommand();
                        playercmd.CommandText = "insert ignore into Players(Name, Elo) " + "values (@PlayerName, @Elo) ON DUPLICATE KEY UPDATE Elo = IF(@Elo > Elo, @Elo, Elo);";
                        playercmd.Parameters.AddWithValue("@PlayerName", game.whitePlayer);
                        playercmd.Parameters.AddWithValue("@Elo", game.whiteElo);

                        //create black player
                        MySqlCommand playercmd2 = new MySqlCommand();
                        playercmd2.CommandText = "insert ignore into Players(Name, Elo) " + "values (@PlayerName, @Elo);";
                        playercmd2.Parameters.AddWithValue("@PlayerName", game.blackPlayer);
                        playercmd2.Parameters.AddWithValue("@Elo", game.blackElo);


                        //create Games
                        MySqlCommand gamecmd = new MySqlCommand();
                        gamecmd.CommandText = "insert ignore into Games(Round, Result, Moves, BlackPlayer, WhitePlayer, eId) " + "values (@Round, @Result, @Moves, (select pID from Players where name = @BlackPlayer), (select pID from Players where name = @WhitePlayer), (select eId from Events where name = @EventName));";
                        gamecmd.Parameters.AddWithValue("@Round", game.Round);
                        gamecmd.Parameters.AddWithValue("@Result", game.Result);
                        gamecmd.Parameters.AddWithValue("@Moves", game.moveList);
                        gamecmd.Parameters.AddWithValue("@BlackPlayer", game.blackPlayer);
                        gamecmd.Parameters.AddWithValue("@WhitePlayer", game.whitePlayer);
                        gamecmd.Parameters.AddWithValue("@EventName", game.Event);

                        cmd.Connection = conn;
                        playercmd.Connection = conn;
                        playercmd2.Connection = conn;
                        gamecmd.Connection = conn;


                        cmd.ExecuteNonQuery();
                        playercmd.ExecuteNonQuery();
                        playercmd2.ExecuteNonQuery();
                        gamecmd.ExecuteNonQuery();

                        await mainPage.NotifyWorkItemCompleted();
                    }

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

        }


        /// <summary>
        /// Queries the database for games that match all the given filters.
        /// The filters are taken from the various controls in the GUI.
        /// </summary>
        /// <param name="white">The white player, or null if none</param>
        /// <param name="black">The black player, or null if none</param>
        /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
        /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
        /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
        /// <param name="start">The start of the date range</param>
        /// <param name="end">The end of the date range</param>
        /// <param name="showMoves">True if the returned data should include the PGN moves</param>
        /// <returns>A string separated by newlines containing the filtered games</returns>
        internal static string PerformQuery(string white, string black, string opening,
          string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
          MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // Build up this string containing the results from your query
            string parsedResult = "";

            // Use this to count the number of rows returned by your query
            // (see below return statement)
            int numRows = 0;

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    MySqlCommand cmd = conn.CreateCommand();



                    //create command string
                    StringBuilder sb = new StringBuilder();
                    if(showMoves)
                    {
                        // correct string should look like select p1.Name from Players p1 join Players p2 join Games as g join Events where (p1.pID = g.WhitePlayer and p2.pID = g.BlackPlayer) and g.eID=Events.eID limit 10;

                        sb.Append("select Events.Name as Event, Site, Date, p1.Name as WhitePlayer, p2.Name as BlackPlayer, Result, Moves from Players p1 join Players p2 join Games as g join Events where (p1.pID = g.WhitePlayer and p2.pID = g.BlackPlayer) and g.eID=Events.eID");
                    }
                    else
                    {
                        sb.Append("select Events.Name as Event, Site, Date, p1.Name as WhitePlayer, p2.Name as BlackPlayer, Result from Players p1 join Players p2 join Games as g join Events where (p1.pID = g.WhitePlayer and p2.pID = g.BlackPlayer) and g.eID=Events.eID");

                    }

                    if (white != null)
                    {
                        sb.Append(" and WhitePlayer = @WhitePlayer");
                        cmd.Parameters.AddWithValue("@WhitePlayer", white);

                    }
                    if (black != null)
                    {
                        sb.Append(" and BlackPlayer = @BlackPlayer");
                        cmd.Parameters.AddWithValue("@BlackPlayer", black);

                    }
                    if (winner != null)
                    {
                        sb.Append(" and Result = @Result");
                        cmd.Parameters.AddWithValue("@Result", winner);
                    }
                    if (useDate)
                    {
                        sb.Append(" and Events.Date > @StartTime and Events.Date < @EndTime");
                        cmd.Parameters.AddWithValue("@StartTime", start);
                        cmd.Parameters.AddWithValue("@EndTime", end);

                    }
                    sb.Append(" limit 10;");
                    cmd.CommandText = sb.ToString();



                    //read result and parse into return
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var Event = reader["Event"];
                            var Site = reader["Site"];
                            var Date = reader["Date"];
                            var WhitePlayer = reader["WhitePlayer"];
                            var BlackPlayer = reader["BlackPlayer"];
                            if (showMoves)
                            {
                                var moves = reader["Moves"];
                            }



                        }
                    };
                    



                    // TODO:
                    //       Generate and execute an SQL command,
                    //       then parse the results into an appropriate string and return it.
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return numRows + " results\n" + parsedResult;
        }

    }
}
