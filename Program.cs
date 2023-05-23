using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BovineChess;
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
        //https://www.chess.com/play/arena/2699599?ref_id=70349336
        // => https://www.chess.com/tournament/live/arena/cramling-tuesday-2699599
        //https://www.chess.com/tournament/live/arena/crazy-bullet-2699632
        //https://www.chess.com/tournament/live/early-titled-tuesday-blitz-may-23-2023-4033933
        //https://api.chess.com/pub/player/annacramling
        // => https://www.chess.com/member/annacramling
        // => https://go.chess.com/Anna [affiliate link!]
        // => https://www.twitch.tv/annacramling
        // => https://api.chess.com/pub/player/theultimatecow
        // => https://api.chess.com/pub/player/annaybc
        // => https://api.chess.com/pub/player/{username}/games/{YYYY}/{MM}
        // => https://api.chess.com/pub/player/theultimatecow/games/2023/05
        // => https://api.chess.com/pub/player/annacramling/games/2023/05
        //https://api.chess.com/pub/player/annacramling/games  -- currently playing games [?] doesn't seem to work
        //https://api.chess.com/pub/player/annacramling/archives -- dubious?

        // double cow: (in above tournament): 77985224509
        // https://www.chess.com/game/live/77985224509
        // https://www.chess.com/analysis/game/live/77985224509

        // cow variation:
        // white cow interrupted by Bg4: https://www.chess.com/game/live/77984453969
        // black cow interrupted by Bb5: https://www.chess.com/game/live/77984480067
        // white cow interrupted by dxe3 (white wins): https://www.chess.com/game/live/77984527389
        // black cow interrupted by various: https://www.chess.com/game/live/77984527391
        // white cow missed: https://www.chess.com/game/live/77984577579 [...6. O-O-O]
        // slow cow (8 moves to complete): https://www.chess.com/game/live/78588870287
        // partial cow (black: 5/6), loses knight https://www.chess.com/game/live/78588877473 

        // empty game?: https://www.chess.com/game/live/77984453973 | https://www.chess.com/analysis/game/live/77984453973?tab=analysis

        //string arenaId = "cramling-bullet-2697041"; // https://www.chess.com/tournament/live/arena/cramling-bullet-2697041
        //string arenaId = "early-titled-tuesday-blitz-may-16-2023-4020317"; // [no cows] https://www.chess.com/tournament/live/early-titled-tuesday-blitz-may-16-2023-4020317
        //string arenaId = "cramling-tuesday-2699599";
        //string arenaId = "crazy-bullet-2699632";
        string arenaId = "early-titled-tuesday-blitz-may-23-2023-4033933";
        string endpoint = "https://api.chess.com/pub/tournament/{0}"; // url-id
        string url = string.Format(endpoint, arenaId);
        //string url = "https://api.chess.com/pub/player/theultimatecow/games/2023/05";

        Console.WriteLine("url: " + url);

        var games = AllGamesFromUrlAsync(url);

        CowStats stats = new();
        await foreach (var game in games) {
            stats.UpdateWithCows(game);
        }

        stats.PrintStats();

        //{"name":"CRAMLING BULLET","url":"https://www.chess.com/tournament/live/arena/cramling-bullet-2697041","creator":"annacramling","status":"finished","start_time":1684245313,"finish_time":1684247113,"settings":{"type":"standard","rules":"chess","is_rated":true,"is_official":false,"is_invite_only":false,"user_advance_count":1,"winner_places":3,"registered_user_count":207,"total_rounds":1,"time_class":"lightning","time_control":"60+0"},"players":[{"username":"notsofastyt","status":"winner"},{"username":"tormikull06","status":"registered"},
        // ...
        // "rounds":["https://api.chess.com/pub/tournament/cramling-bullet-2697041/1"]}

    }

    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromUrlAsync(string url) {
        var client = new HttpClient();
        var jsonText = await client.GetStringAsync(url);
        var json = JsonDocument.Parse(jsonText);

        //creator in System.Text.Json
        if (json.RootElement.TryGetProperty("name", out var nameJsonEl)) {
            Console.WriteLine($"name: {nameJsonEl.GetString()}");
        }

        bool success1 = json.RootElement.TryGetProperty("games", out var gamesAll1);
        if (success1) {
            // e.g. user
            var games = ParseGames(gamesAll1);
            foreach (var game in games) {
                yield return game;
            }
            yield break;
        }

        var rounds = json.RootElement.GetProperty("rounds").EnumerateArray().Select(x => x.GetString()).ToList();

        foreach (var round in rounds) {
            Console.WriteLine($"round: {round}");

            var roundText = await client.GetStringAsync(round); // download round info
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
                    var games1 = ParseGames(groupText);
                    foreach (var game in games1) {
                        yield return game;
                    }
                }
                yield break; // done
            }

            //var games = gamesAll.EnumerateArray().ToList();
            var games2 = ParseGames(gamesAll);
            foreach (var game in games2) {
                yield return game;
            }

        }
    }

    public static IEnumerable<ParsedPGN> ParseGames(string json) {
        var roundJson = JsonDocument.Parse(json);

        bool success = roundJson.RootElement.TryGetProperty("games", out var gamesAll);
        if (!success) {
            throw new Exception("No 'games' in json root element");
        }

        return ParseGames(gamesAll);

    }

    public static IEnumerable<ParsedPGN> ParseGames(JsonElement gamesAll) {

        var games = gamesAll.EnumerateArray().ToList();

        foreach (var game in games) {
            //Console.WriteLine($"game: {game}");
            var gameUrl = game.GetProperty("url").GetString();
            var gamePgn = game.GetProperty("pgn").GetString();
            var board = new ParsedPGN();
            board.Url = gameUrl;
            board.SetPgn(gamePgn);

            yield return board;
        }
    }

    public static CowStats CowChecker(IEnumerable<ParsedPGN> games, CowStats addToTheseStats = null) {

        // todo: separate stats per game + per round + black + white
        // todo: number of players using cow opening

        var stats = addToTheseStats ?? new();

        foreach (var board in games) {

            stats.UpdateWithCows(board);

            //debug
            //if (board?.Url?.Contains("77985696703") ?? false) {
            //    Console.WriteLine("----");
            //    Console.WriteLine(board.Url);
            //    Console.WriteLine(board.Pgn);
            //    Console.WriteLine("----");
            //}
        }

        stats.PrintStats();

        return stats;
    }

}
