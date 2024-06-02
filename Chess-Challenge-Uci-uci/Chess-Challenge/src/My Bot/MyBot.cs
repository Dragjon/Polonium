/***********************************************************************                                                                   
  ,---.            ,--.  ,--.                                         
 /  O  \ ,--,--, ,-'  '-.|  ,---. ,--.--. ,--,--.,--.  ,--.,--.  ,--. 
|  .-.  ||      \'-.  .-'|  .-.  ||  .--'' ,-.  | \  `'  /  \  `'  /  
|  | |  ||  ||  |  |  |  |  | |  ||  |   \ '-'  | /  /.  \  /  /.  \  V0.0.0
`--' `--'`--''--'  `--'  `--' `--'`--'    `--`--''--'  '--''--'  '--' 
                                                                      
 Anthraxx - A hopefully strong PSQT-only chess engine

***********************************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    /***********************************************************************************
     * PeSTO's evaluation function                                                     *
     * References: https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function      *
     **********************************************************************************/

    static int[,] mgPst = {
        {
            0,   0,   0,   0,   0,   0,  0,   0,
            98, 134,  61,  95,  68, 126, 34, -11,
            -6,   7,  26,  31,  65,  56, 25, -20,
            -14,  13,   6,  21,  23,  12, 17, -23,
            -27,  -2,  -5,  12,  17,   6, 10, -25,
            -26,  -4,  -4, -10,   3,   3, 33, -12,
            -35,  -1, -20, -23, -15,  24, 38, -22,
            0,   0,   0,   0,   0,   0,  0,   0,
        },

        {
            -167, -89, -34, -49,  61, -97, -15, -107,
            -73, -41,  72,  36,  23,  62,   7,  -17,
            -47,  60,  37,  65,  84, 129,  73,   44,
            -9,  17,  19,  53,  37,  69,  18,   22,
            -13,   4,  16,  13,  28,  19,  21,   -8,
            -23,  -9,  12,  10,  19,  17,  25,  -16,
            -29, -53, -12,  -3,  -1,  18, -14,  -19,
            -105, -21, -58, -33, -17, -28, -19,  -23,
        },

        {
            -29,   4, -82, -37, -25, -42,   7,  -8,
            -26,  16, -18, -13,  30,  59,  18, -47,
            -16,  37,  43,  40,  35,  50,  37,  -2,
            -4,   5,  19,  50,  37,  37,   7,  -2,
            -6,  13,  13,  26,  34,  12,  10,   4,
            0,  15,  15,  15,  14,  27,  18,  10,
            4,  15,  16,   0,   7,  21,  33,   1,
            -33,  -3, -14, -21, -13, -12, -39, -21,
        },

        {
            32,  42,  32,  51, 63,  9,  31,  43,
            27,  32,  58,  62, 80, 67,  26,  44,
            -5,  19,  26,  36, 17, 45,  61,  16,
            -24, -11,   7,  26, 24, 35,  -8, -20,
            -36, -26, -12,  -1,  9, -7,   6, -23,
            -45, -25, -16, -17,  3,  0,  -5, -33,
            -44, -16, -20,  -9, -1, 11,  -6, -71,
            -19, -13,   1,  17, 16,  7, -37, -26,
        },

        {
            -28,   0,  29,  12,  59,  44,  43,  45,
            -24, -39,  -5,   1, -16,  57,  28,  54,
            -13, -17,   7,   8,  29,  56,  47,  57,
            -27, -27, -16, -16,  -1,  17,  -2,   1,
            -9, -26,  -9, -10,  -2,  -4,   3,  -3,
            -14,   2, -11,  -2,  -5,   2,  14,   5,
            -35,  -8,  11,   2,   8,  15,  -3,   1,
            -1, -18,  -9,  10, -15, -25, -31, -50,
        },

        {
            -65,  23,  16, -15, -56, -34,   2,  13,
            29,  -1, -20,  -7,  -8,  -4, -38, -29,
            -9,  24,   2, -16, -20,   6,  22, -22,
            -17, -20, -12, -27, -30, -25, -14, -36,
            -49,  -1, -27, -39, -46, -44, -33, -51,
            -14, -14, -22, -46, -44, -30, -15, -27,
            1,   7,  -8, -64, -43, -16,   9,   8,
            -15,  36,  12, -54,   8, -28,  24,  14,
        },

    };

    static int[,] egPst = {
        {
              0,   0,   0,   0,   0,   0,   0,   0,
            178, 173, 158, 134, 147, 132, 165, 187,
             94, 100,  85,  67,  56,  53,  82,  84,
             32,  24,  13,   5,  -2,   4,  17,  17,
             13,   9,  -3,  -7,  -7,  -8,   3,  -1,
              4,   7,  -6,   1,   0,  -5,  -1,  -8,
             13,   8,   8,  10,  13,   0,   2,  -7,
              0,   0,   0,   0,   0,   0,   0,   0,
        },

        {
            -58, -38, -13, -28, -31, -27, -63, -99,
            -25,  -8, -25,  -2,  -9, -25, -24, -52,
            -24, -20,  10,   9,  -1,  -9, -19, -41,
            -17,   3,  22,  22,  22,  11,   8, -18,
            -18,  -6,  16,  25,  16,  17,   4, -18,
            -23,  -3,  -1,  15,  10,  -3, -20, -22,
            -42, -20, -10,  -5,  -2, -20, -23, -44,
            -29, -51, -23, -15, -22, -18, -50, -64,
        },

        {
            -14, -21, -11,  -8, -7,  -9, -17, -24,
             -8,  -4,   7, -12, -3, -13,  -4, -14,
              2,  -8,   0,  -1, -2,   6,   0,   4,
             -3,   9,  12,   9, 14,  10,   3,   2,
             -6,   3,  13,  19,  7,  10,  -3,  -9,
            -12,  -3,   8,  10, 13,   3,  -7, -15,
            -14, -18,  -7,  -1,  4,  -9, -15, -27,
            -23,  -9, -23,  -5, -9, -16,  -5, -17,
        },

        {
            13, 10, 18, 15, 12,  12,   8,   5,
            11, 13, 13, 11, -3,   3,   8,   3,
             7,  7,  7,  5,  4,  -3,  -5,  -3,
             4,  3, 13,  1,  2,   1,  -1,   2,
             3,  5,  8,  4, -5,  -6,  -8, -11,
            -4,  0, -5, -1, -7, -12,  -8, -16,
            -6, -6,  0,  2, -9,  -9, -11,  -3,
            -9,  2,  3, -1, -5, -13,   4, -20,
        },

        {
             -9,  22,  22,  27,  27,  19,  10,  20,
            -17,  20,  32,  41,  58,  25,  30,   0,
            -20,   6,   9,  49,  47,  35,  19,   9,
              3,  22,  24,  45,  57,  40,  57,  36,
            -18,  28,  19,  47,  31,  34,  39,  23,
            -16, -27,  15,   6,   9,  17,  10,   5,
            -22, -23, -30, -16, -16, -23, -36, -32,
            -33, -28, -22, -43,  -5, -32, -20, -41,
        },

        {
            -74, -35, -18, -18, -11,  15,   4, -17,
            -12,  17,  14,  17,  17,  38,  23,  11,
             10,  17,  23,  15,  20,  45,  44,  13,
             -8,  22,  24,  27,  26,  33,  26,   3,
            -18,  -4,  21,  24,  27,  23,   9, -11,
            -19,  -3,  11,  21,  23,  16,   7,  -9,
            -27, -11,   4,  13,  14,   4,  -5, -17,
            -53, -34, -21, -11, -28, -14, -24, -43
        },

    };

    static int[] mgPieceValues = { 82, 337, 365, 477, 1025, 0 };
    static int[] egPieceValues = { 94, 281, 297, 512, 936, 0 };

    static int[] gamePhaseInc = { 0, 1, 1, 2, 4, 0 };

    public static int Eval(Board board)
    {
        int[] mg = { 0, 0 };
        int[] eg = { 0, 0 };
        int gamePhase = 0;

        for (int sq = 0; sq < 64; sq++)
        {
            Piece piece = board.GetPiece(new Square(sq));
            if (piece.PieceType != PieceType.None)
            {
                int color = piece.IsWhite ? 0 : 1;
                int xor = piece.IsWhite ? 56 : 0;
                int pieceInt = (int)piece.PieceType - 1;
                mg[color] += mgPst[pieceInt, sq ^ xor] + mgPieceValues[pieceInt];
                eg[color] += egPst[pieceInt, sq ^ xor] + egPieceValues[pieceInt];
                gamePhase += gamePhaseInc[pieceInt];
            }
        }

        int stm = board.IsWhiteToMove ? 0 : 1;
        int mgScore = mg[stm] - mg[stm ^ 1];
        int egScore = eg[stm] - eg[stm ^ 1];

        if (gamePhase > 24) gamePhase = 24;

        int egPhase = 24 - gamePhase;

        return (mgScore * gamePhase + egScore * egPhase) / 24;
    }


    // Variables for search
    static ulong nodes;
    static int globalDepth = 0;
    static int infinity = 1_000_000;
    static int nullMoveR = 3;
    static int rfpMargin = 55;
    static int rfpDepth = 8;
    static int lmrCount = 5;
    static int lmrDepth = 2;
    static double lmrBase = 0.75D;
    static double lmrMul = 0.4D;

    static int mateScore = 500_000;
    static int hardBoundTM = 10;
    static int softBoundTM = 40;

    static Move[] TT = new Move[33554432];
    static Move[] killers = new Move[1024];
    static Move searchBestMove = Move.NullMove;
    static Move rootBestMove = Move.NullMove;

    int[,] mvvlvaTable = {
        { 150_000_000, 250_000_00, 350_000_000, 450_000_000, 550_000_000, 650_000_000 },
        { 140_000_000, 240_000_00, 340_000_000, 440_000_000, 540_000_000, 640_000_000 },
        { 130_000_000, 230_000_00, 330_000_000, 430_000_000, 530_000_000, 630_000_000 },
        { 120_000_000, 220_000_00, 320_000_000, 420_000_000, 520_000_000, 620_000_000 },
        { 110_000_000, 210_000_00, 310_000_000, 410_000_000, 510_000_000, 610_000_000 },
        { 100_000_000, 200_000_00, 300_000_000, 400_000_000, 500_000_000, 600_000_000 },
    };

    public Move Think(Board board, Timer timer)
    {
        nodes = 0;
        int[,] history = new int[64, 64];
        Move[,] counters = new Move[64, 64];

        int QSearch(int alpha, int beta)
        {
            if (globalDepth > 1 && timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / hardBoundTM)
            {
                throw new TimeoutException();
            }

            int standPat = Eval(board);
            int max = standPat;
            ulong hash = board.ZobristKey % 33554432;

            if (standPat >= beta)
            {
                return beta;
            }
            if (standPat > alpha)
            {
                alpha = standPat;
            }
            foreach (Move move in board.GetLegalMoves(true).OrderByDescending(move => move == TT[hash] ? 1_000_000_000 : mvvlvaTable[(int)move.MovePieceType - 1, (int)move.CapturePieceType - 1]))
            {
                nodes++;
                board.MakeMove(move);
                int score = -QSearch(-beta, -alpha);
                board.UndoMove(move);
                if (score >= beta)
                {
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                }

                if (score > max)
                {
                    TT[hash] = move;
                    max = score;
                }
            }

            return max;

        }

        int AlphaBeta(int depth, int ply, int alpha, int beta, Move parentMove)
        {

            if (globalDepth > 1 && timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / hardBoundTM)
            {
                throw new TimeoutException();
            }

            if (depth <= 0)
            {
                return QSearch(alpha, beta);
            }

            int max = -infinity;
            ulong hash = board.ZobristKey % 33554432;
            bool isRoot = ply == 0;
            bool pvNode = beta - alpha != 1;
            bool nodeIsCheck = board.IsInCheck();
            Move[] legals = board.GetLegalMoves();

            if (nodeIsCheck && legals.Length == 0)
            {
                return -mateScore + ply;
            }
            if (!isRoot && board.IsDraw())
            {
                return 0;
            }

            int eval = Eval(board);

            if (depth <= rfpDepth && eval - rfpMargin * depth >= beta && !nodeIsCheck)
            {
                return eval;
            }

            if (eval > beta && board.TrySkipTurn())
            {
                int nullScore = -AlphaBeta(depth - nullMoveR - 1, ply + 1, -beta, -beta + 1, Move.NullMove);
                board.UndoSkipTurn();
                if (nullScore >= beta)
                {
                    return nullScore;
                }
            }

            int moveCount = 0;

            foreach (Move move in legals.OrderByDescending(move => move == TT[hash] ? 1_000_000_000 : move.IsCapture ? mvvlvaTable[(int)move.MovePieceType - 1, (int)move.CapturePieceType - 1] : move == killers[ply] ? 10_000_000 : move == counters[parentMove.StartSquare.Index, parentMove.TargetSquare.Index] ? 1_000_000 : history[move.StartSquare.Index, move.TargetSquare.Index]))
            {
                nodes++;
                moveCount++;

                board.MakeMove(move);

                bool boardIsCheck = board.IsInCheck();

                int checkExtension = boardIsCheck ? 1 : 0;
                int lmr = moveCount > lmrCount && depth >= lmrDepth && !move.IsCapture && !nodeIsCheck && !pvNode ? (int)(lmrBase + Math.Log(depth) * Math.Log(moveCount) * lmrMul) : 0;

                int score = 0;

                if (moveCount == 1 && pvNode)
                {
                    score = -AlphaBeta(depth + checkExtension - 1, ply + 1, -beta, -alpha, move);
                }
                else
                {
                    score = -AlphaBeta(depth + checkExtension - lmr - 1, ply + 1, -alpha - 1, -alpha, move);

                    if (score > alpha && beta > score)
                    {
                        score = -AlphaBeta(depth + checkExtension - 1, ply + 1, -beta, -alpha, move);
                    }
                }

                board.UndoMove(move);

                if (score > alpha)
                {
                    alpha = score;

                }

                if (score > max)
                {
                    TT[hash] = move;

                    if (isRoot)
                    {
                        searchBestMove = move;
                    }
                    max = score;

                }

                if (score >= beta)
                {
                    if (!move.IsCapture)
                    {
                        killers[ply] = move;
                        history[move.StartSquare.Index, move.TargetSquare.Index] = Math.Min(history[move.StartSquare.Index, move.TargetSquare.Index] + depth * depth, 999_999);

                        if (!parentMove.IsNull)
                        {
                            counters[parentMove.StartSquare.Index, parentMove.TargetSquare.Index] = move;
                        }
                    }
                    break;
                }
            }

            return max;
        }

        try
        {
            int alpha = -infinity;
            int beta = infinity;
            globalDepth = 0;
            for (int depth = 1; depth < 256; ++depth)
            {
                globalDepth = depth;

                if (depth > 1 && timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / softBoundTM)
                {
                    break;
                }

                killers = new Move[1024];

                int score = AlphaBeta(depth, 0, alpha, beta, Move.NullMove);
                rootBestMove = searchBestMove;
                Console.WriteLine($"info depth {depth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 1)} score cp {score} pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
            }
        }

        catch (TimeoutException)
        {

        }

        return rootBestMove;
    }
}