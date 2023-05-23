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
    public string comment;

    //TODO: Cleanup / normalize:
    // - normalize unicode characters (e.g. ♘ to N) -- ♙♘♗♖♕♔♚♛♜♝♞♟
    // - castling with 0's (0-0 or 0-0-0)

    //TODO: support other bits of algebraic notation: 
    //presumably should be in comments though?
    // - "e.p." "(=)" "†" "ch"
    // - long algebraic notation (LAN) with a hyphen
    // - "!!" (brilliant move) etc

    // see:
    // https://en.wikipedia.org/wiki/Portable_Game_Notation
    // https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
    // https://en.wikipedia.org/wiki/Chess_notation
    // https://en.wikipedia.org/wiki/Chess_annotation_symbols
    // https://en.wikipedia.org/wiki/Chess_symbols_in_Unicode

    private static readonly Regex MoveParts = new Regex(@"(O-O|O-O-O|([PNBRQK]?)([a-h]?[1-8]?)(x)?([a-h][1-8])(?:=?([NBRQ]))?(\+|#)?)", RegexOptions.Compiled);

    public Move(bool isBlack, int moveNumber, string move, string time) {
        this.isBlack = isBlack;
        this.moveNumber = moveNumber;
        this.move = move;
        this.comment = time;

        if (this.moveNumber < 1) {
            throw new Exception("Invalid move number: " + moveNumber);
        }
    }

    public Move(bool isBlack, string moveNumber, string move, string time)
            : this(isBlack, int.Parse(moveNumber.Trim('.')), move, time) { 
    }

    public override string ToString() {
        return $"{moveNumber}{(isBlack ? "..." : ".")} {move}{(comment != "" ? $" {comment}" : "")}";
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
