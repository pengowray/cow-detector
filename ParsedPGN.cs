using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


// Pgn contents example: from https://www.chess.com/game/live/77985696703
/*
[Event "Live Chess"]
[Site "Chess.com"]
[Date "2023.05.16"]
[Round "-"]
[White "imamur"]
[Black "Nicks_Move"]
[Result "0-1"]
[CurrentPosition "5rk1/p2n1ppp/8/1q2n3/8/4P2P/P2QKPP1/5B1R w - -"]
[Timezone "UTC"]
[ECO "D15"]
[ECOUrl "https://www.chess.com/openings/Slav-Defense-Modern-Three-Knights-Variation"]
[UTCDate "2023.05.16"]
[UTCTime "14:16:32"]
[WhiteElo "462"]
[BlackElo "480"]
[TimeControl "60"]
[Termination "Nicks_Move won on time"]
[StartTime "14:16:32"]
[EndDate "2023.05.16"]
[EndTime "14:18:36"]
[Link "https://www.chess.com/game/live/77985696703"]

1. d4 {[%clk 0:00:59.4]} 1... d5 {[%clk 0:00:58.1]} 2. c4 {[%clk 0:00:58.9]} 2... c6 {[%clk 0:00:56.4]} 3. Nc3 {[%clk 0:00:58.3]} 3... Nf6 {[%clk 0:00:54.7]} 4. Nf3 {[%clk 0:00:57.3]} 4... Bg4 {[%clk 0:00:52]} 5. e3 {[%clk 0:00:52.9]} 5... Nbd7 {[%clk 0:00:50.3]} 6. h3 {[%clk 0:00:51.8]} 6... Bxf3 {[%clk 0:00:48.6]} 7. Qxf3 {[%clk 0:00:51.7]} 7... e5 {[%clk 0:00:41.9]} 8. dxe5 {[%clk 0:00:50.2]} 8... Nxe5 {[%clk 0:00:40.8]} 9. Qe2 {[%clk 0:00:45.4]} 9... Bd6 {[%clk 0:00:39.1]} 10. cxd5 {[%clk 0:00:43.1]} 10... cxd5 {[%clk 0:00:35.7]} 11. Qb5+ {[%clk 0:00:39.7]} 11... Nfd7 {[%clk 0:00:30.8]} 12. Nxd5 {[%clk 0:00:37.6]} 12... O-O {[%clk 0:00:26.3]} 13. Qxb7 {[%clk 0:00:27]} 13... Rc8 {[%clk 0:00:24.4]} 14. Nc3 {[%clk 0:00:22.7]} 14... Qa5 {[%clk 0:00:20.3]} 15. Qf3 {[%clk 0:00:19.5]} 15... Bb4 {[%clk 0:00:17.3]} 16. Bd2 {[%clk 0:00:14.9]} 16... Bxc3 {[%clk 0:00:14.6]} 17. Bxc3 {[%clk 0:00:13.8]} 17... Rxc3 {[%clk 0:00:12.9]} 18. bxc3 {[%clk 0:00:12.7]} 18... Qxc3+ {[%clk 0:00:11.9]} 19. Ke2 {[%clk 0:00:09.1]} 19... Qxa1 {[%clk 0:00:10.8]} 20. Qd5 {[%clk 0:00:03.6]} 20... Qb2+ {[%clk 0:00:09.1]} 21. Qd2 {[%clk 0:00:00.3]} 21... Qb5+ {[%clk 0:00:06.5]} 0-1
*/


namespace BovineChess;

public class ParsedPGN {
    public string Url { get; set; }
    public string Pgn { get; set; }

    public Dictionary<string, string> Tags { get; private set; }
    public List<Move> Moves { get; private set; }

    public ParsedPGN() {
    }
    public ParsedPGN(string url, string pgn) {
        Url = url;
        SetPgn(pgn);
    }

    public string? GetTag(string key) {
        if (Tags == null) {
            return null;
        }
        if (Tags.TryGetValue(key, out string value)) {
            return value;
        }
        return null;
    }

    public void SetPgn(string pgn) {
        Pgn = pgn;
        Tags = ExtractTags(pgn);
        Moves = ParsePgn(pgn);

    }

    // "1." (white move) or "1..." (black move, optional)
    // the move itself: at least 2 characters; parse later
    // 
    private static readonly Regex MovesRegex = new Regex(@"(\d+(\.|\.\.\.))\s+?([a-z0-9A-Z\-\=\+\#]{2,})(\s+{.*?})?\s*", RegexOptions.Compiled | RegexOptions.Multiline);

    public static List<Move> ParsePgn(string pgn) {
        var lines = pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        List<Move> moves = new List<Move>();
        int currentMoveNumber = 1;

        foreach (var line in lines) {
            if (line.StartsWith("[") || line.StartsWith(";")) {
                continue;
            }
            var matches = MovesRegex.Matches(line);
            foreach (Match match in matches) {
                bool isBlack = match.Groups[2].Value == "...";
                if (!isBlack) {
                    var moveNumberText = match?.Groups[1]?.Value?.Trim('.');
                    if (moveNumberText != null && int.TryParse(moveNumberText, out int moveNumber)) {
                        currentMoveNumber = moveNumber;
                    }
                }
                string move = match.Groups[3].Value;
                string comment = match.Groups[6].Value;

                var moveObj = new Move(isBlack, currentMoveNumber, move, comment);
                moves.Add(moveObj);

                if (isBlack) currentMoveNumber++;
            }
        }

        return moves;
    }


    public static Dictionary<string, string> ExtractTags(string pgn) {
        var metadata = new Dictionary<string, string>();
        var metadataRegex = new Regex(@"\[(\w+)\s+""([^""]+)""\]", RegexOptions.Multiline);

        foreach (Match match in metadataRegex.Matches(pgn)) {
            metadata.Add(match.Groups[1].Value, match.Groups[2].Value);
        }

        return metadata;
    }



    public CowInfo? Cows() {
        return new CowInfo(this);
    }

}
