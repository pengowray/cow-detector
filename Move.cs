using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BovineChess;
public class Move {
    public bool isBlack;
    public int moveNumber;
    public string move;
    public int? nag; // numeric annotation glyph
    public string? comment;

    //TODO: track board state
    // - movement, pieces taken, castling, en passant, etc

    //TODO: Cleanup / normalize:
    // - normalize unicode characters (e.g. ♘ to N) -- ♙♘♗♖♕♔♚♛♜♝♞♟
    // - castling with 0's (0-0 or 0-0-0)
    // - castling with 2 square moves
    // - castling by taking the rook
    // - normalize case

    //TODO: support other bits of algebraic notation: 
    //presumably should be in comments though?
    // - "e.p." "(=)" "†" "ch"
    // - long algebraic notation (LAN) with a hyphen
    // - "!!" (brilliant move) etc

    //TODO: support notation used in UCI https://backscattering.de/chess/uci/
    // - long algebraic notation
    // - e7e8q (for promotion) 
    // - e1g1 (white short castling)
    // - 0000 (nullmove)

    // [Variant "Crazyhouse"]
    // - has @ symbols, example:
    // 1. d4 d5 2. Nf3 Bf5 3. g3 e6 4. Bg2 Bd6 5. Kf1 Nc6 6. h3 Nf6 7. Kg1 O-O 8. Nc3 Ne4 9. Nh4 Nxf2 10. Kxf2 Bxg3+ 11. Kxg3 P@g5 12. Nxf5 exf5 13. Nxd5 f4+ 14. Kf2 P@g3+ 15. Ke1 Re8 16. P@e5 Nxe5 17. dxe5 Rxe5 18. B@c3 P@f2+ 19. Kf1 Rxd5 20. Bxd5 P@e6 21. N@h6+ Kf8 22. R@g8+ Ke7 23. B@c5+ N@d6 24. N@f5+ exf5 25. Nxf5+ Kd7 26. P@e6+ fxe6 27. Rxd8+ Rxd8 28. Q@e7+ Kc8 29. Nxd6+ cxd6 30. Qxb7# 1-0

    // TWIC files:
    // - sometimes moves are written as "--" which is missing or a null move (probably not recorded or unreadable)
    // - sometimes just a single letter written down e.g. "R" (rook moved; maybe didn't record where; was also moved in previous and next movews): 1. e4 c5 2. Nf3 g6 3. d4 cxd4 4. Qxd4 Nf6 5. e5 Nc6 6. Qa4 Nd5 7. Qe4 Ndb4 8. Bb5 a6 9. Bxc6 Nxc6 10. Nc3 Bg7 11. Bf4 O-O 12. O-O-O d6 13. exd6 exd6 14. Rxd6 Qa5 15. Rd5 b5 16. Bd6 Bf5 17. Rxf5 gxf5 18. Qxc6 R 19. Qd5 Rfd8 20. Ne5 1-0

    // TODO: Seirawan Chess and Capablanca Chess
    // - "H" for Hawk, "E" for Elephant, "A" for Archbishop, "C" for Chancellor).

    // Notation used in icofy
    // - "45.Qd1 h1Q" instead of "45.Qd1 h1=Q" (both versions found of same game found in the files)
    // - full move list for testing: "1.e3 d5 2.d3 e5 3.Ne2 Bd6 4.Nd2 c6 5.Ng3 f5 6.Nb3 Nf6 7.Be2 O-O 8.Nh5 Nbd7 9.Bd2 b6 10.d4 Nxh5 11.Bxh5 e4 12.Be2 Qc7 13.g3 a5 14.a4 Nf6 15.h3 Be6 16.Bf1 g5 17.Nc1 Bxg3 18.Bg2 Bd6 19.Ne2 Nh5 20.Ng1 Qf7 21.Qe2 f4 22.Qd1 f3 23.Bf1 Ng3 24.Rh2 Nxf1 25.Rh1 Nxd2 26.Qxd2 c5 27.O-O-O Qf6 28.c3 Rfc8 29.Kb1 c4 30.Qc2 b5 31.Rd2 bxa4 32.h4 g4 33.h5 Bd7 34.h6 Be8 35.Qd1 Rcb8 36.Nh3 gxh3 37.Rg1+ Kf8 38.Rg4 h2 39.Qf1 Qxh6 40.Rd1 a3 41.Rxe4 Bg6 42.Ka2 Rxb2+ 43.Ka1 Bxe4 44.Rb1 Bxb1 45.Qd1 h1Q 46.Qxf3+ 0-1"

    // see also:
    // https://en.wikipedia.org/wiki/Portable_Game_Notation
    // https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
    // https://en.wikipedia.org/wiki/Chess_notation
    // https://en.wikipedia.org/wiki/Chess_annotation_symbols
    // https://en.wikipedia.org/wiki/Chess_symbols_in_Unicode
    // https://en.wikipedia.org/wiki/Universal_Chess_Interface
    // https://en.wikipedia.org/wiki/Descriptive_notation [obsolete]

    private static readonly Regex MoveParts = new Regex(@"(O-O|O-O-O|--|([PNBRQK]?)(?<drop>\@)?([a-h]?[1-8]?)(x)?([a-h][1-8])(?:=?([NBRQ]))?(\+\+|\+|#)?([\!\?]+(N|TN)?)?)(?<nag>\s*\$[0-9]+)?", RegexOptions.Compiled);

    // TODO: parse time handling commands: (maybe in another class as it uses both tags and comments)
    // https://www.enpassant.dk/chess/palview/enhancedpgn.htm
    // {[%clk 1:55:21]}
    // {[%egt 1:25:42]} - Elapsed Game Time 
    // {[%emt 0:34:18]} - Elapsed Move Time
    // {[%mct 17:10:42]} - time displayed on a mechanical clock
    // also relevant:
    // [Clock "B/0:45:56"] - Clock tag
    // [TimeControl "40/7200:3600"] - Time Control tag
    // [TimeControl "60 mins"] // non standard time control from DGT board [ePGN "0.1;DGT LiveChess/2.2"] (due to DGT LiveViewer GUI freeform field)
    // [WhiteClock "1:07:00"] - mainly intended to cover the case where a game is adjourned and the displayed clock times at start of play are thus non standard
    // [BlackClock "0:56:00"]


    // technically this is a Ply, not a Move.
    // TODO: Rename: Ply
    public Move(bool isBlack, int moveNumber, string move, int? nag, string? comment) {
        this.isBlack = isBlack;
        this.moveNumber = moveNumber;
        this.move = move;
        this.nag = nag;
        this.comment = comment;

        if (this.moveNumber < 1) {
            throw new Exception("Invalid move number: " + moveNumber);
        }
    }

    public Move(bool isBlack, string moveNumber, string move, int? nag, string time)
            : this(isBlack, int.Parse(moveNumber.Trim('.')), move, nag, time) { 
    }

    public override string ToString() {
        string commentText = (comment == null) ? "" : $" {{{comment}}}";
        string nagText = (nag == null) ? "" : $" ${nag}";
        return $"{moveNumber}{(isBlack ? "..." : ".")} {move}{nag}{commentText}";
    }


    public string? IsMoveTo(string onlyPieceAndPosShort) {
        
        var parts = MoveParts.Matches(move);
        if (parts.Count == 0) {
            return null;
        }

        //check for castling
        if (onlyPieceAndPosShort.StartsWith("O-O")) {
            var simpleMatch = parts[0].Groups[2].Value;
            var castleMatch = simpleMatch == onlyPieceAndPosShort;
            return simpleMatch;
        }

        var part = parts[0];
        var piece = part.Groups[2].Value;
        var pos = part.Groups[5].Value;
        var pieceAndPos = piece + pos;

        if (onlyPieceAndPosShort.StartsWith('P')) onlyPieceAndPosShort = onlyPieceAndPosShort.Substring(1);
        if (pieceAndPos.StartsWith('P')) pieceAndPos = pieceAndPos.Substring(1);

        //debug
        //Console.WriteLine($"IsMoveTo: {onlyPieceAndPosShort} (actual {pieceAndPos}) match: {pieceAndPos == onlyPieceAndPosShort}");

        if (pieceAndPos == onlyPieceAndPosShort) return pieceAndPos;
        return null;
    }

    public string? IsMoveTo(string[] onlyPieceAndPosShort) {
        
        // returns the matching piece and pos if it matches (normalized), or "O-O" or "O-O-O" for castling
        // returns null if no match

        var parts = MoveParts.Matches(move);
        if (parts.Count == 0) {
            return null;
        }

        var part = parts[0];
        var piece = part.Groups[2].Value;
        var pos = part.Groups[5]?.Value ?? "";
        var pieceAndPos = piece + pos;
        if (pieceAndPos.StartsWith('P')) pieceAndPos = pieceAndPos.Substring(1);

        foreach (var possible in onlyPieceAndPosShort) {
            //check for castling
            if (possible.StartsWith("O-O")) {
                var castleMatch = piece == possible;
                return piece;
            }

            var poss = possible;
            if (possible.StartsWith('P')) poss = possible.Substring(1);

            if (pieceAndPos == poss) {
                return poss;
            }
        }

        return null;
    }

}
