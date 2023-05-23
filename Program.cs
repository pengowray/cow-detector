using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chesscom_analysis;
internal class Program {

    static void Main(string[] args) {
        MainAsync(args).GetAwaiter().GetResult();

    }
    static async Task MainAsync(string[] args) {

        //api
        //https://www.chess.com/news/view/published-data-api
        //https://www.chess.com/clubs/forum/view/guide-unofficial-api-documentation

        //api examples:
        //https://api.chess.com/pub/tournament/cramling-bullet-2697041
        // => https://www.chess.com/tournament/live/arena/cramling-bullet-2697041
        //https://api.chess.com/pub/tournament/cramling-bullet-2697041/1
        //https://api.chess.com/pub/player/annacramling
        // => https://www.chess.com/member/annacramling
        // => https://go.chess.com/Anna [affiliate link!]
        // => https://www.twitch.tv/annacramling
        // => https://api.chess.com/pub/player/theultimatecow
        // => https://api.chess.com/pub/player/annaybc
        // => https://api.chess.com/pub/player/{username}/games/{YYYY}/{MM}
        // => https://api.chess.com/pub/player/theultimatecow/games/2023/05
        //https://api.chess.com/pub/player/annacramling/games  -- currently playing games
        //https://api.chess.com/pub/player/annacramling/archives

        // double cow: (in above tournament): 77985224509
        // https://www.chess.com/game/live/77985224509
        // https://www.chess.com/analysis/game/live/77985224509

        string arenaId = "cramling-bullet-2697041"; // https://www.chess.com/tournament/live/arena/cramling-bullet-2697041
        //string arenaId = "early-titled-tuesday-blitz-may-16-2023-4020317"; // [no cows] https://www.chess.com/tournament/live/early-titled-tuesday-blitz-may-16-2023-4020317
        string endpoint = "https://api.chess.com/pub/tournament/{0}"; // url-id
        string url = string.Format(endpoint, arenaId);
        //string url = "https://api.chess.com/pub/player/theultimatecow/games/2023/05";

        Console.WriteLine("url: " + url);

        var client = new HttpClient();
        var jsonText = await client.GetStringAsync(url);
        var json = JsonDocument.Parse(jsonText);

        //{"name":"CRAMLING BULLET","url":"https://www.chess.com/tournament/live/arena/cramling-bullet-2697041","creator":"annacramling","status":"finished","start_time":1684245313,"finish_time":1684247113,"settings":{"type":"standard","rules":"chess","is_rated":true,"is_official":false,"is_invite_only":false,"user_advance_count":1,"winner_places":3,"registered_user_count":207,"total_rounds":1,"time_class":"lightning","time_control":"60+0"},"players":[{"username":"notsofastyt","status":"winner"},{"username":"tormikull06","status":"registered"},
        // ...
        // "rounds":["https://api.chess.com/pub/tournament/cramling-bullet-2697041/1"]}
        Console.WriteLine(jsonText);

        //creator in System.Text.Json
        if (json.RootElement.TryGetProperty("name", out var nameJsonEl)) {
            Console.WriteLine($"name: {nameJsonEl.GetString()}");
        }

        bool success1 = json.RootElement.TryGetProperty("games", out var gamesAll1);
        if (success1) {
            // e.g. user
            CowCheck(gamesAll1);
            return;
        }

        var rounds = json.RootElement.GetProperty("rounds").EnumerateArray().Select(x => x.GetString()).ToList();

        foreach (var round in rounds) {
            Console.WriteLine($"round: {round}");

            var roundText = await client.GetStringAsync(round);
            var roundJson = JsonDocument.Parse(roundText);

            bool success = roundJson.RootElement.TryGetProperty("games", out var gamesAll);
            if (!success) {
                // no games found, but maybe has groups (which then have games)
                // tournaments subdivided into groups
                bool success2 = roundJson.RootElement.TryGetProperty("groups", out var groups);
                if (!success2) {
                    //Console.WriteLine("no games or groups");
                    continue;
                } 

                //groups example: https://api.chess.com/pub/tournament/early-titled-tuesday-blitz-may-16-2023-4020317/11/1
                foreach (var group in groups.EnumerateArray().ToList()) {
                    var groupText = await client.GetStringAsync(group.GetString());
                    CowCheck(groupText);
                }
                return;
                

            }

            //var games = gamesAll.EnumerateArray().ToList();
            CowCheck(gamesAll);
        }
    }

    public static void CowCheck(string json) {
        var roundJson = JsonDocument.Parse(json);

        bool success = roundJson.RootElement.TryGetProperty("games", out var gamesAll);
        if (!success) {
            return;
        }

        CowCheck(gamesAll);

    }
    public static void CowCheck(JsonElement gamesAll) {
        
        // todo: separate stats per game + per round + black + white
        // todo: number of players using cow opening
        int gamesWithCows = 0;
        int totalGames = 0;
        int totalCows = 0;
        int doubleCows = 0;
        int whiteCows = 0;
        int blackCows = 0;
        int cowWins = 0;
        int cowLosses = 0;
        int cowDraws = 0;
        HashSet<string> cowUser = new();
        HashSet<string> allUsers = new();

        var games = gamesAll.EnumerateArray().ToList();

        foreach (var game in games) {
            //Console.WriteLine($"game: {game}");
            var gameUrl = game.GetProperty("url").GetString();
            var gamePgn = game.GetProperty("pgn").GetString();
            var board = new ChessComBoard();
            board.Url = gameUrl;
            board.SetPgn(gamePgn);

            //debug
            //if (board?.Url?.Contains("77985696703") ?? false) {
            //    Console.WriteLine("----");
            //    Console.WriteLine(board.Url);
            //    Console.WriteLine(board.Pgn);
            //    Console.WriteLine("----");
            //}

            totalGames++;

            var cows = board.Cows();

            if (cows != null) {
                gamesWithCows++;
                if (cows == "double") {
                    totalCows += 2;
                    doubleCows++;
                } else {
                    totalCows++;
                }

                string date = board.Metadata["UTCDate"]; // "2023.05.16"
                string white = board.Metadata["White"]; // username
                string black = board.Metadata["Black"]; // username
                string termination = board.Metadata["Termination"]; // "Username won on time"
                string result = board.Metadata["Result"]; // "0-1" or "1-0" or "1/2-1/2"
                string eco = board.Metadata["ECO"]; // e.g. "D15" // Encyclopaedia of Chess Openings
                string ecoUrl = board.Metadata["ECOUrl"]; // e.g. "https://www.chess.com/openings/Slav-Defense-Modern-Three-Knights-Variation"
                string ecoText = "";
                if (cows == "white" || cows == "double") {
                    ecoText = $" - ECO: {ecoUrl} [{eco}]";
                }
                allUsers.Add(white);
                allUsers.Add(black);
                if (cows == "white" || cows == "double") {
                    cowUser.Add(white);
                    whiteCows++;
                }
                if (cows == "black" || cows == "double") {
                    cowUser.Add(black);
                    blackCows++;
                }

                if (result == "1-0") {
                    if (cows == "white") {
                        cowWins++;
                    } else if (cows == "black") {
                        cowLosses++;
                    }
                } else if (result == "0-1") {
                    if (cows == "white") {
                        cowLosses++;
                    } else if (cows == "black") {
                        cowWins++;
                    }
                } else if (result == "1/2-1/2" && (cows == "white" || cows == "black")) { 
                    cowDraws++;
                }

                Console.WriteLine($"{date} - {board.Url} - {cows} - {white} v {black} - {termination} ({result}){ecoText}");

            } else {
                string white = board.Metadata["White"]; // username
                string black = board.Metadata["Black"]; // username
                allUsers.Add(white);
                allUsers.Add(black);

                //debug
                //Console.WriteLine("[no cow game]");
                //string result = board.Metadata["Result"];
                //Console.WriteLine($"{board.Url} - {result}");
            }

        }

        double percent = (double)cowUser.Count() / (double)allUsers.Count(); // * 100.0;
        Console.WriteLine($" - total games: {totalGames}; games with cows: {gamesWithCows}, including {doubleCows} with double cows for a total of {totalCows} cows.");
        Console.WriteLine($" - games with one cow: Cow wins/losses/draws: {cowWins}/{cowLosses}/{cowDraws}");
        Console.WriteLine($" - white cows: {whiteCows}; black cows: {blackCows}");
        Console.WriteLine($" - total players: {allUsers.Count()}; players who used cow opening at least once: {cowUser.Count()} ({percent:P2})");

    }

}
