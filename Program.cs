using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using ZstdSharp;

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
        // => [pgn download link] blob:https://www.chess.com/d366c5eb-14f2-4675-98e6-35c2a2ffdbd0
        // => [csv results link] https://www.chess.com/tournament/live/early-titled-tuesday-blitz-may-23-2023-4033933/download-results
        // => https://api.chess.com/pub/tournament/early-titled-tuesday-blitz-may-23-2023-4033933
        //https://api.chess.com/pub/player/annacramling
        // => https://www.chess.com/member/annacramling
        // => https://go.chess.com/Anna [affiliate link!]
        // => https://www.twitch.tv/annacramling
        // => https://api.chess.com/pub/player/theultimatecow
        // => https://api.chess.com/pub/player/annaybc
        // => https://api.chess.com/pub/player/{username}/games/{YYYY}/{MM}
        // => https://api.chess.com/pub/player/theultimatecow/games/2023/05
        // => https://api.chess.com/pub/player/annacramling/games/2023/05
        // => https://api.chess.com/pub/player/annacramling/tournaments
        //https://api.chess.com/pub/player/annacramling/games  -- currently playing games [?] doesn't seem to work
        //https://api.chess.com/pub/player/annacramling/archives -- dubious?
        //https://www.chess.com/member/MagnusCarlsen
        // 

        // double cow: (in above tournament): 77985224509
        // https://www.chess.com/game/live/77985224509
        // https://www.chess.com/analysis/game/live/77985224509

        // cow variation:
        // white cow interrupted by Bg4: https://www.chess.com/game/live/77984453969
        // black cow interrupted by Bb5: https://www.chess.com/game/live/77984480067
        // white cow interrupted by dxe3 (white wins): https://www.chess.com/game/live/77984527389
        // black cow interrupted by various: https://www.chess.com/game/live/77984527391
        // white cow missed: https://www.chess.com/game/live/77984577579 [...6. O-O-O]
        // slow cow, black (7 moves to complete): https://www.chess.com/game/live/78584576311
        // slow cow, black (8 moves to complete): https://www.chess.com/game/live/78588870287
        // slow cow, black (10 moves to complete): https://www.chess.com/game/live/78586245065
        // partial cow (black: 5/6), loses knight https://www.chess.com/game/live/78588877473 

        // empty game?: https://www.chess.com/game/live/77984453973 | https://www.chess.com/analysis/game/live/77984453973?tab=analysis

        //string arenaId = "cramling-bullet-2697041"; // https://www.chess.com/tournament/live/arena/cramling-bullet-2697041
        //string arenaId = "early-titled-tuesday-blitz-may-16-2023-4020317"; // [no cows] https://www.chess.com/tournament/live/early-titled-tuesday-blitz-may-16-2023-4020317
        //string arenaId = "cramling-tuesday-2699599";
        //string arenaId = "crazy-bullet-2699632";
        //string arenaId = "early-titled-tuesday-blitz-may-23-2023-4033933"; // older rounds seem to disappear? // blob:https://www.chess.com/d366c5eb-14f2-4675-98e6-35c2a2ffdbd0
        string arenaId = "late-titled-tuesday-blitz-may-23-2023-4033934"; // https://api.chess.com/pub/tournament/late-titled-tuesday-blitz-may-23-2023-4033934
        //string arenaId = "-33rd-chesscom-quick-knockouts-1401-1600"; // old example
        string endpoint = "https://api.chess.com/pub/tournament/{0}"; // url-id
        //string url = string.Format(endpoint, arenaId);
        //string url = "https://api.chess.com/pub/player/theultimatecow/games/2023/05";
        //string url = "https://api.chess.com/pub/player/MagnusCarlsen/games/2023/05";
        //string url = "https://api.chess.com/pub/player/mobamba604/games/2023/05"; // https://www.chess.com/players/andrea-botez
        //string url = "https://api.chess.com/pub/player/alexandrabotez/games/2023/05";
        //string url = "https://api.chess.com/pub/player/themagician/games/2023/05";//FM John Curtis Australia
        //string url = "https://api.chess.com/pub/player/hikaru/games/2023/05";
        //string url = "https://api.chess.com/pub/player/thechesstina/games/2023/05";
        //string url = "https://api.chess.com/pub/player/laurarrgh/games/2023/05";
        //string url = "https://api.chess.com/pub/player/gmbenjaminfinegold/games/2023/05"; // https://www.twitch.tv/itsbenandkaren
        string url = "https://api.chess.com/pub/player/DanielNaroditsky/games/2023/05"; // https://www.twitch.tv/gmnaroditsky
        //string url = "https://api.chess.com/pub/player/KNVB/games/2023/05"; // chessbruh / Aman Hambleton -- beat a cow
        //string url = "https://api.chess.com/pub/player/dinabelenkaya/games/2023/05";
        //string url = "https://api.chess.com/pub/player/gothamchess/games/2023/05";
        // => https://www.chess.com/game/live/78352375713 // gotham played cow opening (previously reported)
        //Console.WriteLine("url: " + url);

        //var games = AllGamesFromUrlAsync(url);

        // no cows
        //var games = AllGamesFromEventMultiPgnFile(@"C:\pgn\Early-Titled-Tuesday-Blitz-May-23-2023_2023-05-24-01-00.pgn");
        //var games = AllGamesFromEventMultiPgnFile(@"C:\pgn\Late-Titled-Tuesday-Blitz-May-23-2023_2023-05-24-07-00.pgn");

        // lichess
        // https://lichess.org/api/games/user/{username}
        //var games = AllGamesFromEventMultiPgnFile(@"C:\pgn\lichess_jjosujjosu_2023-05-24.pgn");
        //var games = AllGamesFromEventMultiPgnFile(@"C:\pgn\lichess_DrNykterstein_2023-05-24.pgn"); // https://lichess.org/api/games/user/DrNykterstein // DrNykterstein;Magnus Carlsen;2863
        // => https://lichess.org/UemmwQwt
        // => https://lichess.org/h1CKf15x - partial cow (black: 6/6 in 14 k[8] q[14]) - DrNykterstein v Puddingsjakk (1-0) - [A13]
        //var games = AllGamesFromEventMultiPgnFile(@"C:\pgn\lichess_penguingim1_2023-05-24.pgn"); // https://www.twitch.tv/penguingm1/

        //var games = AllGamesFromEventMultiPgnFile(@"c:\pgn\lichess_TSMFTXH_2023-05-25.pgn"); https://lichess.org/api/games/user/tsmftxh // hikaru?
        // => https://lichess.org/cKUFqHRw tortured complete cow on move 13 - partial cow (white: 6/6 in 13 K[3] Q[13]) - arian95 v penguingim1 (0-1)
        // => https://lichess.org/V0ZReyC4 complete cow on final move
        // => https://lichess.org/Ko5VCJpa cow completed with king's knight for both sides - (K[14] Q[31]) - penguingim1 v NoTheories (0-1)
        //var games = AllGamesFromEventMultiPgnFile(@"c:\pgn\lichess_Ruchess27_2023-05-25.pgn");
        // => https://lichess.org/qvESzjy9 partial cow (white: 6/6 in 11 K[6] Q[11]) - dragonchess83 v Ruchess27 (0-1) 
        // => 
        //AnishGiri;Anish Giri;2764
        //STL_Caruana;Fabiano Caruana;2835
        //STL_Dominguez;Dominguez Perez, Leinier;2758

        //general pgn files, e.g. https://www.pgnmentor.com/files.html
        //var games = AllGamesFromEventMultiPgnFile(@"C:\pgn\Carlsen.pgn");
        //var games = AllGamesFromZipFile(@"C:\pgn\pgnfiles\Giri.zip");
        //var games = AllGamesFromFolder(@"C:\pgn\pgnfiles");
        //var games = AllGamesFromZipFile(@"C:\pgn\elite\LichessEliteDatabase.zip");
        //var games = AllGamesFromFolder(@"C:\pgn\twic");
        //var games = AllGamesFromZipFile(@"C:\pgn\icofy\IB107PGN.zip");
        var games = AllGamesFromFolder(@"C:\pgn\icofy");

        //var games = AllGamesFromZstFile(@"C:\pgn\lichess\lichess_db_standard_rated_2013-01.pgn.zst");

        var outputPgn = @"c:\pgn\output\cows.pgn";
        var outputPgnPart = @"c:\pgn\output\partial-cows.pgn";

        using (StreamWriter outputFile = new StreamWriter(outputPgn))
        using (StreamWriter outputFile2 = new StreamWriter(outputPgnPart)) {

            CowStats stats = new();
            await foreach (var game in games) {
                //await Console.Out.WriteLineAsync(game.Url);
                var cows = stats.UpdateWithCows(game, showCowless: false);
                if (cows?.HasCows ?? false) {
                    outputFile?.WriteLineAsync(game.Pgn.TrimEnd());
                    outputFile?.WriteLineAsync();
                    outputFile?.WriteLineAsync();
                    outputFile?.Flush();
                } else if (cows?.HasPartialCows ?? false) {
                    outputFile2?.WriteLineAsync(game.Pgn.TrimEnd());
                    outputFile2?.WriteLineAsync();
                    outputFile2?.WriteLineAsync();
                    outputFile2?.Flush();
                }
            }

            stats.PrintStats();

            outputFile?.Flush();
            outputFile?.Close();

            outputFile2?.Flush();
            outputFile2?.Close();
        }


        //{"name":"CRAMLING BULLET","url":"https://www.chess.com/tournament/live/arena/cramling-bullet-2697041","creator":"annacramling","status":"finished","start_time":1684245313,"finish_time":1684247113,"settings":{"type":"standard","rules":"chess","is_rated":true,"is_official":false,"is_invite_only":false,"user_advance_count":1,"winner_places":3,"registered_user_count":207,"total_rounds":1,"time_class":"lightning","time_control":"60+0"},"players":[{"username":"notsofastyt","status":"winner"},{"username":"tormikull06","status":"registered"},
        // ...
        // "rounds":["https://api.chess.com/pub/tournament/cramling-bullet-2697041/1"]}

    }
    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromFolder(string folder) {
        // read all zip or pgn files in folder
        // todo: optionally recurse into subfolders

        foreach (var file in Directory.EnumerateFiles(folder)) {
            if (file.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase)) {
                await foreach (var game in AllGamesFromTarBz2(file)) {
                    yield return game;
                }
            } else if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
                await foreach (var game in AllGamesFromZip(file)) {
                    yield return game;
                }
            } else if (file.EndsWith(".zst", StringComparison.OrdinalIgnoreCase)) {
                await foreach (var game in AllGamesFromZst(file)) {
                    yield return game;
                }
            } else if (file.EndsWith(".pgn", StringComparison.OrdinalIgnoreCase)) {
                await foreach (var game in AllGamesFromEventMultiPgnFile(file)) {
                    yield return game;
                }
            }
        }
    }

    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromZip(string file) {
        using (var archive = ZipFile.OpenRead(file)) {
            foreach (var entry in archive.Entries) {
                if (entry.FullName.EndsWith(".pgn", StringComparison.OrdinalIgnoreCase)) {
                    var fileStream = new StreamReader(entry.Open());
                    using (fileStream) {
                        var all = AllGamesFromEventMultiPgnStream(fileStream, file + "/" + entry.FullName);
                        await foreach (var game in all) {
                            yield return game;
                        }
                    }
                }
            }
        }
    }

    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromTarBz2(string tarBz2Path, [EnumeratorCancellation] CancellationToken ct = default) {
        using (var bz2Stream = File.OpenRead(tarBz2Path)) {
            using (var bzip2InputStream = new BZip2InputStream(bz2Stream))
            using (var tarInputStream = new TarInputStream(bzip2InputStream, Encoding.Latin1)) { // not sure about this encoding
                TarEntry tarEntry;
                while ((tarEntry = await tarInputStream.GetNextEntryAsync(ct)) != null) {
                    if (tarEntry != null && !tarEntry.IsDirectory) {
                        // Calculate the size of the entry to read only that much
                        var buffer = new byte[tarEntry.Size];
                        await tarInputStream.ReadAsync(buffer, 0, buffer.Length, ct);

                        using (var entryStream = new MemoryStream(buffer))
                        using (var reader = new StreamReader(entryStream)) {
                            if (tarEntry.Name.EndsWith(".pgn", StringComparison.OrdinalIgnoreCase)) {
                                var all = AllGamesFromEventMultiPgnStream(reader, tarBz2Path + "/" + tarEntry.Name);
                                await foreach (var game in all.WithCancellation(ct)) {
                                    yield return game;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromZst(string file) {
        using var input = File.OpenRead(file);
        using var fileStream = new StreamReader(new DecompressionStream(input));
        var all = AllGamesFromEventMultiPgnStream(fileStream, file);
        await foreach (var game in all) {
            yield return game;
        }
    }


    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromEventMultiPgnStream(StreamReader reader, string filename) {
        StringBuilder sb = new StringBuilder();
        int entryCount = 0;
        int lineNumber = 1;
        int sbLineNumber = 1;

        using (reader) {
            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
                if (line.StartsWith("[Event")) {
                    if (sb.Length > 0) {
                        yield return new ParsedPGN($"{filename}#{sbLineNumber} (game {entryCount})", sb.ToString());
                        sb.Clear();
                        entryCount++;
                        sbLineNumber = lineNumber;
                    }
                }
                sb.AppendLine(line);
                lineNumber++;
            }
        }
        if (sb.Length > 0) {
            yield return new ParsedPGN($"{filename}#{sbLineNumber} (game {entryCount})", sb.ToString());
        }

    }

    public static async IAsyncEnumerable<ParsedPGN> AllGamesFromEventMultiPgnFile(string file) {
        //TODO: make less scuffed (check for start of "[" block instead of "[Event", but this works for lichess and chess.com examples i've seen so far)
        StringBuilder sb = new StringBuilder();
        int entryCount = 0;
        int lineNumber = 1;
        int sbLineNumber = 1;
        using (var reader = System.IO.File.OpenText(file)) {
            var all = AllGamesFromEventMultiPgnStream(reader, file);
            await foreach (var game in all) {
                yield return game;
            }
        }
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
