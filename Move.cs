using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace chesscom_analysis;
public struct Move {
    public bool isBlack;
    public int moveNumber;
    public string move;
    public string comment;

    //TODO: support ♙♘♗♖♕♔♚♛♜♝♞♟
    //TODO: support castling with 0's (0-0-0)
    //TODO: support other bits of algebraic notation: // presumably should be in comments though?
    // - "e.p." "(=)" "†" "ch"
    // - long algebraic notation (LAN) with a hyphen
    // - "!!" (brilliant move) etc


    private static readonly Regex MovePart = new Regex(@"(O-O|O-O-O|[PNBRQK]?([a-h]?[1-8]?)(x)?([a-h][1-8])(?:=?([NBRQ]))?(\+|#)?)", RegexOptions.Compiled);

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

}
