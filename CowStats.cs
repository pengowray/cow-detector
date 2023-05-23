using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BovineChess;

public class CowStats {
    public int gamesWithCows = 0;
    public int totalGames = 0;
    public int totalCows = 0;
    public int doubleCows = 0;
    public int whiteCows = 0;
    public int blackCows = 0;

    // for games with one cow only:
    public int cowWins = 0;
    public int cowLosses = 0;
    public int cowDraws = 0;
    
    public HashSet<string> cowUser = new();
    public HashSet<string> allUsers = new();

    public void UpdateWithCows(ParsedPGN board) {
        var cows = board.Cows();

        totalGames++;

        if (cows == null) {
            return;
        }

        if (cows.HasCows) {

            gamesWithCows++;

            if (cows.HasBlackCow && cows.HasWhiteCow) {
                totalCows += 2;
                doubleCows++;
            } else {
                totalCows++;
            }

            string? date = board.GetTag("UTCDate"); // "2023.05.16"
            string? white = board.GetTag("White"); // username
            string? black = board.GetTag("Black"); // username
            string? termination = board.GetTag("Termination"); // "Username won on time"
            string? result = board.GetTag("Result"); // "0-1" or "1-0" or "1/2-1/2" or "*" (incomplete)
            string? eco = board.GetTag("ECO"); // e.g. "D15" // Encyclopaedia of Chess Openings
            string? ecoUrl = board.GetTag("ECOUrl"); // e.g. "https://www.chess.com/openings/Slav-Defense-Modern-Three-Knights-Variation"
            string ecoText = "";
            if (cows.HasWhiteCow) {
                ecoText = $" - ECO: {ecoUrl} [{eco}]";
            }
            allUsers.Add(white);
            allUsers.Add(black);
            if (cows.HasWhiteCow) {
                cowUser.Add(white);
                whiteCows++;
            }
            if (cows.HasBlackCow) {
                cowUser.Add(black);
                blackCows++;
            }

            if (result == "1-0") {
                if (cows.HasWhiteCow) {
                    cowWins++;
                } else if (cows.HasBlackCow) {
                    cowLosses++;
                }
            } else if (result == "0-1") {
                if (cows.HasWhiteCow) {
                    cowLosses++;
                } else if (cows.HasBlackCow) {
                    cowWins++;
                }
            } else if (result == "1/2-1/2" && ((cows.HasWhiteCow || cows.HasBlackCow) && !(cows.HasWhiteCow && cows.HasBlackCow))) {
                cowDraws++;
            }

            Console.WriteLine($"{date} - {board.Url} - {cows} - {white} v {black} - {termination} ({result}){ecoText}");

        } else {
            string white = board.GetTag("White"); // username
            string black = board.GetTag("Black"); // username
            allUsers.Add(white);
            allUsers.Add(black);
            string eco = board.GetTag("ECO"); // e.g. "D15"

            //debug
            string result = board.GetTag("Result");
            Console.WriteLine($"[no cow game] {board.Url} - {result} - {eco}");
        }

    }

    public void PrintStats() {
        double percent = (double)cowUser.Count() / (double)allUsers.Count(); // * 100.0;
        Console.WriteLine($" - total games: {totalGames}; games with cows: {gamesWithCows}, including {doubleCows} with double cows for a total of {totalCows} cows.");
        Console.WriteLine($" - games with one cow: Cow wins/losses/draws: {cowWins}/{cowLosses}/{cowDraws}");
        Console.WriteLine($" - white cows: {whiteCows}; black cows: {blackCows}");
        Console.WriteLine($" - total players: {allUsers.Count()}; players who used cow opening at least once: {cowUser.Count()} ({percent:P2})");
    }

}

