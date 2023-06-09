﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BovineChess;
public class CowInfo {

    // white cow queen-side: d3 Nd2 Nb3
    // white cow king-side: e3 Ne2 Ng3
    // black cow king-side: e6 Ne7 Ng6
    // black cow queen-side: d6 Nd7 Nb6

    // todo variations to track (ideas; name ideas)
    // - crossed horn: d3 Nd2 Nf3 (+ mirrored versions)
    // - various orders of cow moves (e.g. e3 before d3)
    // - slow cow (other moves before cow is completed)
    // - cow denied (e.g. opponent rushes pawn to a4 or h4 preventing cow knights)
    // - cow's gambit (make a cow anyway)
    // - stampede attack - vs cow (via chatgpt https://youtu.be/buvtoYNOdWQ)

    // other related:

    // related named openings:
    // - Van't Kruijs Opening: d3 (ECO)
    // - Mieses Opening: e3 (ECO)
    // - Valencia Opening: d3 e5 Nd2
    // - Hippopotamus Defense
    // - The Defense Game by PAFU (2002) www.beginnersgame.com - 8 move openings

    // cow vids:
    // - https://youtu.be/jBvieY3leXk - introduction
    // - https://youtu.be/3_f1h2udGcE


    static readonly string[] CowWhiteQueenSideMoves = { "d3", "Nd2", "Nb3" };
    static readonly string[] CowWhiteKingSideMoves = { "e3", "Ne2", "Ng3" };
    static readonly string[] CowBlackKingSideMoves = { "e6", "Ne7", "Ng6" };
    static readonly string[] CowBlackQueenSideMoves = { "d6", "Nd7", "Nb6" };
    static readonly string[] CowWhiteMoves = { "d3", "Nd2", "Nb3", "e3", "Ne2", "Ng3" };
    static readonly string[] CowBlackMoves = { "e6", "Ne7", "Ng6", "d6", "Nd7", "Nb6" };

    public OpeningMatch CowWhiteQueenSide;
    public OpeningMatch CowWhiteKingSide;
    public OpeningMatch CowBlackKingSide;
    public OpeningMatch CowBlackQueenSide;
    public OpeningMatch CowWhite;
    public OpeningMatch CowBlack;
    
    public bool HasWhiteCow { get; private set; }
    public bool HasBlackCow { get; private set; }
    public bool HasCows => HasWhiteCow || HasBlackCow;
    public int CowCount => (HasWhiteCow ? 1 : 0) + (HasBlackCow ? 1 : 0);

    public int WhiteCowCompleteness => CowWhite.Seen.Length;
    public int BlackCowCompleteness => CowBlack.Seen.Length;

    //TODO: more flexible with partial cow: complete one side, slow cow < 12 moves, 5/6 cow, etc?
    // was: PartialWhiteCow => CowWhite.SeenNoBreaks.Length >= 3;
    // now it's more slowCow than partialCow
    public bool PartialWhiteCow => (CowWhite.Seen.Length >= 6 && CowWhite.LastMove.moveNumber <= 16) || (CowWhite.Seen.Length >= 5 && CowWhite.LastMove.moveNumber <= 8);
    public bool PartialBlackCow => (CowBlack.Seen.Length >= 6 && CowBlack.LastMove.moveNumber <= 16) || (CowBlack.Seen.Length >= 5 && CowBlack.LastMove.moveNumber <= 8);

    public bool HasPartialCows => PartialWhiteCow || PartialBlackCow;


    public CowInfo(ParsedPGN game) {
        CowWhiteQueenSide = new OpeningMatch(CowWhiteQueenSideMoves, game, isBlack: false);
        CowWhiteKingSide = new OpeningMatch(CowWhiteKingSideMoves, game, isBlack: false);
        CowBlackKingSide = new OpeningMatch(CowBlackKingSideMoves, game, isBlack: true);
        CowBlackQueenSide = new OpeningMatch(CowBlackQueenSideMoves, game, isBlack: true);
        CowWhite = new OpeningMatch(CowWhiteMoves, game, isBlack: false);
        CowBlack = new OpeningMatch(CowBlackMoves, game, isBlack: true);

        HasWhiteCow = CowWhite.SeenNoBreaks.Length >= 6;
        HasBlackCow = CowBlack.SeenNoBreaks.Length >= 6;
    }

    public override string ToString() {
        if (CowBlack == null)
            return "null cows";

        if (HasCows) {
            if (CowCount == 2) {
                return "both cows";
            } else {
                return HasWhiteCow ? "white cow" : "black cow";
            }
        } else if (HasPartialCows) {
            // TODO: show as chronological: when were milestones hit
            string whitePart = "";
            string blackPart = "";
            if (PartialWhiteCow) {
                whitePart = $"white: {CowWhite?.LastMove?.moveNumber}. {WhiteCowCompleteness}/6";
                if (CowWhiteKingSide.Seen.Length >= 3) whitePart += $" {CowWhiteKingSide.LastMove?.moveNumber}. K"; //  " ♚";
                if (CowWhiteQueenSide.Seen.Length >= 3) whitePart += $" {CowWhiteQueenSide.LastMove?.moveNumber}. Q"; //  " ♛";
            }
            if (PartialBlackCow) {
                blackPart = $"black: {CowBlack?.LastMove?.moveNumber}... {BlackCowCompleteness}/6";
                if (CowBlackKingSide.Seen.Length >= 3) blackPart += $" {CowBlackKingSide.LastMove?.moveNumber}... k"; // " ♔";
                if (CowBlackQueenSide.Seen.Length >= 3) blackPart += $" {CowBlackQueenSide.LastMove?.moveNumber}... q"; //  " ♕";
            }
            if (whitePart != "" && blackPart != "")
                return $"partial cows ({whitePart}; {blackPart})";
            else
                return $"partial cow ({whitePart}{blackPart})";
            
        } else {
            return "No cows";
        }
    }
    
}
