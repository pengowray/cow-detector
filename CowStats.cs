using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    public int cowIncomplete = 0;

    // games with partial cows:
    public int partialCow;
    public int[] cowAmounts = new int[7];

    public HashSet<string> allUsers = new();
    public HashSet<string> cowUser = new();
    public HashSet<string> partialCowUser = new();

    public CowInfo UpdateWithCows(ParsedPGN board, bool showCowless = false) {
        var cows = board.Cows();

        totalGames++;

        if (cows == null) {
            return null;
        }

        if (board == null) {
            return null;
        }

        string variant = board.GetTag("Variant");
        string nonStandard = (variant == null || variant == "Standard") ? "" : $" (Variant: {variant})";

        if (cows.HasCows) {

            gamesWithCows++;
            totalCows += cows.CowCount;

            if (cows.CowCount == 2) {
                doubleCows++;
            }

            string? date = board.GetTag("UTCDate"); // "2023.05.16"
            string? white = board.GetTag("White"); // username
            string? black = board.GetTag("Black"); // username
            string? termination = board.GetTag("Termination"); // "Username won on time"
            string? result = board.GetTag("Result"); // "0-1" or "1-0" or "1/2-1/2" or "*" (incomplete)
            string? eco = board.GetTag("ECO"); // e.g. "D15" // Encyclopaedia of Chess Openings
            string? ecoUrl = board.GetTag("ECOUrl"); // e.g. "https://www.chess.com/openings/Slav-Defense-Modern-Three-Knights-Variation"
            string ecoText = "";
            string? site = board.GetUniqueUrl(); // board.GetTag("Site"); 

            if (cows.HasWhiteCow) {
                //ecoText = $" - ECO: {ecoUrl} [{eco}]";
                ecoText = $" - [{eco}]";
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
            } else if (result == "1/2-1/2") { 
                if (cows.CowCount == 1) cowDraws++;
            } else { // result == "*" or missing
                cowIncomplete++;
            }

            Console.WriteLine($"{date} - {board.Url} - {cows} - {white} v {black} ({result}{nonStandard}) - {termination}{ecoText} - {site}");

        } else if (cows.HasPartialCows) {
            string? date = board.GetTag("UTCDate"); // "2023.05.16"
            string? white = board.GetTag("White"); // username
            string? black = board.GetTag("Black"); // username
            string? eco = board.GetTag("ECO"); // e.g. "D15" // Encyclopaedia of Chess Openings
            string? result = board.GetTag("Result");
            string? site = board.GetUniqueUrl(); // board.GetTag("Site"); 

            if (cows.PartialBlackCow) {
                partialCowUser.Add(black);
            } else if (cows.PartialWhiteCow) {
                partialCowUser.Add(white);
            }

            Console.WriteLine($"[Partial cow(s)] {date} - {board.Url} - {cows} - {white} v {black} ({result}{nonStandard}) - [{eco}] - {site}");
            partialCow++;

        } else {
            string white = board.GetTag("White"); // username
            string black = board.GetTag("Black"); // username
            allUsers.Add(white);
            allUsers.Add(black);
            string eco = board.GetTag("ECO"); // e.g. "D15"
            string result = board.GetTag("Result");

            //debug
            ///last move so i can see it parsed
            if (showCowless) Console.WriteLine($"[no cow] {board.Url} - {white} v {black} ({result}) - [{eco}] - Final:{board?.Moves?.LastOrDefault()}");
        }

        return cows;

    }

    public void PrintStats() {
        double percent = (double)cowUser.Count() / (double)allUsers.Count(); // * 100.0;
        Console.WriteLine($" - total games: {totalGames}; games with cows: {gamesWithCows}, including {doubleCows} with double cows for a total of {totalCows} cows.");
        Console.WriteLine($" - games with one cow: Cow wins/losses/draws: {cowWins}/{cowLosses}/{cowDraws}");
        Console.WriteLine($" - white cows: {whiteCows}; black cows: {blackCows}");
        Console.WriteLine($" - total players: {allUsers.Count()}; players who used cow opening at least once: {cowUser.Count()} ({percent:P2}) " + string.Join(" ", cowUser.OrderBy(u => u)));
        Console.WriteLine($" - games with no full cows but some partial cow: {partialCow}. Partial cow players (if any): " + string.Join(" ", partialCowUser.OrderBy(u => u)));
    }

}

