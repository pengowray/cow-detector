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

    // TODO: Seirawan Chess and Capablanca Chess
    // "H" for Hawk, "E" for Elephant, "A" for Archbishop, "C" for Chancellor).

    // see:
    // https://en.wikipedia.org/wiki/Portable_Game_Notation
    // https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
    // https://en.wikipedia.org/wiki/Chess_notation
    // https://en.wikipedia.org/wiki/Chess_annotation_symbols
    // https://en.wikipedia.org/wiki/Chess_symbols_in_Unicode
    // https://en.wikipedia.org/wiki/Universal_Chess_Interface

    private static readonly Regex MoveParts = new Regex(@"(O-O|O-O-O|([PNBRQK]?)(?<drop>\@)?([a-h]?[1-8]?)(x)?([a-h][1-8])(?:=?([NBRQ]))?(\+\+|\+|#)?([\!\?]+(N|TN)?)?)", RegexOptions.Compiled);

    // technically this is a Ply, not a Move.
    // TODO: Rename: Ply
    public Move(bool isBlack, int moveNumber, string move, string? comment) {
        this.isBlack = isBlack;
        this.moveNumber = moveNumber;
        this.move = move;
        this.comment = comment;

        if (this.moveNumber < 1) {
            throw new Exception("Invalid move number: " + moveNumber);
        }
    }

    public Move(bool isBlack, string moveNumber, string move, string time)
            : this(isBlack, int.Parse(moveNumber.Trim('.')), move, time) { 
    }

    public override string ToString() {
        return $"{moveNumber}{(isBlack ? "..." : ".")} {move}{(comment != "" ? $" {{{comment}}}" : "")}";
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
