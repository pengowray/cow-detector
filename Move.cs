using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chesscom_analysis;
public struct Move {
    public bool isBlack;
    public int moveNumber;
    public string move;
    public string time;

    public Move(bool isBlack, string moveNumber, string move, string time) {
        this.isBlack = isBlack;
        this.moveNumber = int.Parse(moveNumber.TrimEnd('.')); // e.g. "1." TODO: try 
        this.move = move;
        this.time = time;
    }
}
