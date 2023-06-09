﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BovineChess;
public class OpeningMatch {
    
    
    public string[] Seen { get; private set; }
    public Move LastMove { get; private set; } // last move of seen moves
    public string[] SeenNoBreaks { get; private set; } // don't allow other moves in between
    public string[] SeenInOrder { get; private set; }
    public Move LastMoveInOrderMove { get; private set; } // last move of SeenInOrder
    public string[] SeenInOrderNoBreaks { get; private set; }


    public OpeningMatch(string[] openingMoves, ParsedPGN game, bool? isBlack) {

        List<string> seen = new();
        List<string> seenNoBreaks = new();
        List<string> seenInOrder = new();
        List<string> seenInOrderNoBreaks = new();

        int n = 0;
        bool brokenChain = false;
        bool brokenChainInOrder = false;

        foreach (var move in game.Moves) {
            //if (move.moveNumber > 6) {
            //    break;
            //}

            if (!isBlack.HasValue || move.isBlack == isBlack) {

                var matched = move.IsMoveTo(openingMoves);
                if (matched != null) {
                    if (!seen.Contains(matched)) {
                        seen.Add(matched);
                        LastMove = move;
                    } else {
                        brokenChain = true;
                    }
                    if (!brokenChain) {
                        seenNoBreaks.Add(matched);
                    }
                } else {
                    brokenChain = true;
                }

                if (n < openingMoves.Length) {
                    var matchedInOrder = move.IsMoveTo(openingMoves[n]);
                    if (matchedInOrder != null) {
                        seenInOrder.Add(matchedInOrder);
                        LastMoveInOrderMove = move;

                        if (!brokenChainInOrder) {
                            seenInOrderNoBreaks.Add(matched);
                        }

                    } else {
                        brokenChainInOrder = true;
                    }
                }
            }

            n++;

        }

        Seen = seen.ToArray();
        SeenNoBreaks = seenNoBreaks.ToArray();
        SeenInOrder = seenInOrder.ToArray();
        SeenInOrderNoBreaks = seenInOrderNoBreaks.ToArray();
    }
}
