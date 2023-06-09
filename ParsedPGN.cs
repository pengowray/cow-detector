﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// url this file was retrieved from. May include free text. Might end with #[line number] if it was retrieved from a multi-game file.
    /// Use GetUniqueUrl() to get a url to view the game itself
    /// TODO: better names / system
    /// </summary>
    public string Url { get; set; }
    public string Pgn { get; private set; }

    public Dictionary<string, string> Tags { get; private set; }


    public List<Move> Moves { get; private set; }

    // Found at the end of the moves list: "1-0", "0-1", "1/2-1/2"
    // If both set, should be same as Tags["Result"]
    // Should not be set if Tags["Result"] is "*"
    public string EndResult { get; private set; } 

    /// <summary>
    /// Returns the original pgn, with \n line endings, and with the given tag added to the end of the tag list (or replacing an existing)
    /// </summary>
    /// <param name="tagName"></param>
    /// <param name="tagBody"></param>
    /// <returns></returns>
    public string OriginalPgnWithAddedTag(string tagName, string tagBody) {
        var lines = Pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        const char NL = '\n';
        var sb = new StringBuilder();
        bool tagAdded = false;
        foreach (var line in lines) {
            if (line.StartsWith("[")) {
                if (!tagAdded && tagName != null && line.StartsWith($"[{tagName} ")) {
                    sb.Append($"[{tagName} \"{tagBody}\"]" + NL);
                    tagAdded = true;
                } else {
                    sb.Append(line + NL);
                }
            } else {
                if (!tagAdded && tagName != null) {
                    sb.Append($"[{tagName} \"{tagBody}\"]" + NL);
                    tagAdded = true;
                }
                sb.Append(line + NL);
            }
        }
        return sb.ToString().TrimEnd();
    }

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

    /// <summary>
    /// 
    /// </summary>
    /// <returns>the game's unique url, if available</returns>
    public string? GetUniqueUrl() {
        // [LichessURL] is used by later files in Lichess Elite Database <https://database.nikonoel.fr/> instead of [Site] because SCID (Shane's Chess Information Database) can't handle too many unique site fields

        // for lichess [Site] is like: "https://lichess.org/78HRmFuB", for chess.com it's just always "Chess.com"

        var url = GetTag("LichessURL") ?? GetTag("Site");
        if (string.IsNullOrWhiteSpace(url)) {
            return null;
        }
        string lower = url.ToLowerInvariant();
        if (lower == "https://lichess.org/" || lower == "Chess.com" || lower == "lichess.org") {
            // not unique
            //TODO: better handling of generic urls
            return null;
        }
        if (lower == "https://www.redhotpawn.com") {
            var gameid = GetTag("GameId");
            if (string.IsNullOrWhiteSpace(gameid)) return null;
            return $"https://www.redhotpawn.com/chess/chess-game-history.php?gameid={gameid}";
        }
        return url;
    }

    public void SetPgn(string pgn) {
        Pgn = pgn;
        ExtractTags();
        ParsePgn();
    }

    // "1." (white move) or "1..." (black move, optional)
    // the move itself: at least 2 characters; parse later
    // (?![0-9]\.) is a negative lookahead so "10." is not matched as black's move (after white's move)
    // RegexOptions.Singleline to allow for newlines (e.g. in comments)
    // note: \@ is just for Crazy House and Bughouse variations
    // "--" and single letter moves e.g. "R" are for The Week in Chess (twic) PGNs which have missing, partial or null moves recorded
    // icofy base: doesn't use a space after the number: "1.c4 f5 2.Nc3 Nf6 3.g3 g6 4.Bg2 Bg7 5.d3 d6 6.e4 Nc6 7.Nge2 O-O 8.O-O e5" so changed a \s+ to \s*
    private static readonly Regex MovesRegex = new Regex(@"((?<num>\d+)(?<dots>\.|\.\.\.))(\s*(?![0-9]+\.)((?<move>(O-O(-O)?|0-0(-0)?|\-\-|[a-zA-Z][a-z0-9A-Z\@]?)[a-z0-9A-Z\-\=\+\#\!\?⌓□∞⩲⩱±∓⯹\(\)\/\@]*)(\s+\$(?<nag>\d+))?(\s*{(?<comment>.*?)})?(\s+(?<result>0\-1|1\-0|1\/2\-1\/2|\*))?)){1,2}\s*", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    public void ParsePgn() {
        var lines = Pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        List<Move> moves = new List<Move>();
        int currentMoveNumber = 0;

        StringBuilder sb = new StringBuilder();
        foreach (var line in lines) {
            // ignore: [tags], comments (;), and escape mechanism (%)
            if (line.StartsWith("[") || line.StartsWith(";") || line.StartsWith("%")) {
                continue;
            }
            sb.AppendLine(line);
        }
        string full = sb.ToString();

        var matches = MovesRegex.Matches(full);
        foreach (Match match in matches) {
            var moveNumText = match.Groups["num"].Value;
            var dots = match.Groups["dots"].Value;
            bool isBlack = dots == "...";

            if (moveNumText != null && int.TryParse(moveNumText, out int moveNumber)) {
                // TODO: check and report somewhere else instead
                if (moveNumber == 0) {
                    Console.WriteLine($"Warning: Move number is 0: {moveNumber}. (should be 1 or higher); dots: {dots}");
                } else if (currentMoveNumber != 0 && moveNumber != currentMoveNumber) {
                    Console.WriteLine($"Warning: Out of sequence moves: expected:{currentMoveNumber}. found:{moveNumber}. dots: {dots}");
                    Console.WriteLine("- Moves so far: " + string.Join(" ", moves));
                    Console.WriteLine("- Url: " + Url);
                    Console.WriteLine("- Full pgn:\n" + Pgn);
                }
                currentMoveNumber = moveNumber;
            }

            string move = match.Groups["move"].Captures[0].Value;
            string? comment = (match.Groups["comment"].Captures.Count >= 1) ? match.Groups["comment"].Captures[0].Value : null;
            string? nagText = (match.Groups["nag"].Captures.Count >= 1) ? match.Groups["nag"].Captures[0].Value : null;
            int? nag = nagText != null ? int.Parse(nagText) : null;

            var firstHalfMove = new Move(isBlack, currentMoveNumber, move, nag, comment);
            moves.Add(firstHalfMove);

            bool hasSecondPly = match.Groups["move"].Captures.Count >= 2;
            if (hasSecondPly && isBlack) {
                Console.WriteLine($"Error: Second ply for black move: {currentMoveNumber}... {move}");
            }

            if (hasSecondPly) {
                string movePly2 = match.Groups["move"].Captures[1].Value;
                string? commentPly2 = (match.Groups["comment"].Captures.Count >= 2) ? match.Groups["comment"].Captures[1].Value : null;
                string? nagText2 = (match.Groups["nag"].Captures.Count >= 2) ? match.Groups["nag"].Captures[1].Value : null;
                int? nag2 = nagText2 != null ? int.Parse(nagText2) : null;

                var secondHalfMove = new Move(isBlack: true, currentMoveNumber, movePly2, nag2, commentPly2);
                moves.Add(secondHalfMove);

                currentMoveNumber++;

            } else if (isBlack) {
                currentMoveNumber++;
            }

            bool hasResult = match.Groups["result"].Captures.Count > 0;
            if (hasResult) {
                var endResult = match.Groups["result"].Captures[0].Value;
                if (!string.IsNullOrWhiteSpace(endResult)) EndResult = endResult;
            }
            
        }

        Moves = moves;
    }


    public void ExtractTags() {
        var tags = new Dictionary<string, string>();
        var tagRegex = new Regex(@"\[(\w+)\s+""([^""]+)""\]", RegexOptions.Multiline);

        foreach (Match match in tagRegex.Matches(Pgn)) {
            tags.Add(match.Groups[1].Value, match.Groups[2].Value);
        }

        Tags = tags;
    }



    public CowInfo? Cows() {
        return new CowInfo(this);
    }

}
