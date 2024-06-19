/***********************************************************************                                                                   
   ___      _             _                 
  / _ \___ | | ___  _ __ (_)_   _ _ __ ___  
 / /_)/ _ \| |/ _ \| '_ \| | | | | '_ ` _ \ 
/ ___/ (_) | | (_) | | | | | |_| | | | | | |
\/    \___/|_|\___/|_| |_|_|\__,_|_| |_| |_|
                                                                                                               
 Polonium - A hopefully strong PSQT-only chess engine

***********************************************************************/

using System;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    /***********************************************************************************
     * PeSTO's evaluation function                                                     *
     * References: https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function      *
     **********************************************************************************/

    // Packed Eval
    // https://minuskelvin.net/chesswiki/content/packed-eval.html

    static int S(short mg, short eg)
    {
        return ((int)eg << 16) + (int)mg;
    }

    static short unpack_mg(int packed)
    {
        return (short)packed;
    }

    static short unpack_eg(int packed)
    {
        return (short)((packed + 0x8000) >> 16);
    }

    static int[,] pst = {
        {
            S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0),
            S(180, 272), S(216, 267), S(143, 252), S(177, 228), S(150, 241), S(208, 226), S(116, 259), S(71, 281),
            S(76, 188), S(89, 194), S(108, 179), S(113, 161), S(147, 150), S(138, 147), S(107, 176), S(62, 178),
            S(68, 126), S(95, 118), S(88, 107), S(103, 99), S(105, 92), S(94, 98), S(99, 111), S(59, 111),
            S(55, 107), S(80, 103), S(77, 91), S(94, 87), S(99, 87), S(88, 86), S(92, 97), S(57, 93),
            S(56, 98), S(78, 101), S(78, 88), S(72, 95), S(85, 94), S(85, 89), S(115, 93), S(70, 86),
            S(47, 107), S(81, 102), S(62, 102), S(59, 104), S(67, 107), S(106, 94), S(120, 96), S(60, 87),
            S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0),
        },
        {
            S(170, 223), S(248, 243), S(303, 268), S(288, 253), S(398, 250), S(240, 254), S(322, 218), S(230, 182),
            S(264, 256), S(296, 273), S(409, 256), S(373, 279), S(360, 272), S(399, 256), S(344, 257), S(320, 229),
            S(290, 257), S(397, 261), S(374, 291), S(402, 290), S(421, 280), S(466, 272), S(410, 262), S(381, 240),
            S(328, 264), S(354, 284), S(356, 303), S(390, 303), S(374, 303), S(406, 292), S(355, 289), S(359, 263),
            S(324, 263), S(341, 275), S(353, 297), S(350, 306), S(365, 297), S(356, 298), S(358, 285), S(329, 263),
            S(314, 258), S(328, 278), S(349, 280), S(347, 296), S(356, 291), S(354, 278), S(362, 261), S(321, 259),
            S(308, 239), S(284, 261), S(325, 271), S(334, 276), S(336, 279), S(355, 261), S(323, 258), S(318, 237),
            S(232, 252), S(316, 230), S(279, 258), S(304, 266), S(320, 259), S(309, 263), S(318, 231), S(314, 217),
        },
        {
            S(336, 283), S(369, 276), S(283, 286), S(328, 289), S(340, 290), S(323, 288), S(372, 280), S(357, 273),
            S(339, 289), S(381, 293), S(347, 304), S(352, 285), S(395, 294), S(424, 284), S(383, 293), S(318, 283),
            S(349, 299), S(402, 289), S(408, 297), S(405, 296), S(400, 295), S(415, 303), S(402, 297), S(363, 301),
            S(361, 294), S(370, 306), S(384, 309), S(415, 306), S(402, 311), S(402, 307), S(372, 300), S(363, 299),
            S(359, 291), S(378, 300), S(378, 310), S(391, 316), S(399, 304), S(377, 307), S(375, 294), S(369, 288),
            S(365, 285), S(380, 294), S(380, 305), S(380, 307), S(379, 310), S(392, 300), S(383, 290), S(375, 282),
            S(369, 283), S(380, 279), S(381, 290), S(365, 296), S(372, 301), S(386, 288), S(398, 282), S(366, 270),
            S(332, 274), S(362, 288), S(351, 274), S(344, 292), S(352, 288), S(353, 281), S(326, 292), S(344, 280),
        },
        {
            S(509, 525), S(519, 522), S(509, 530), S(528, 527), S(540, 524), S(486, 524), S(508, 520), S(520, 517),
            S(504, 523), S(509, 525), S(535, 525), S(539, 523), S(557, 509), S(544, 515), S(503, 520), S(521, 515),
            S(472, 519), S(496, 519), S(503, 519), S(513, 517), S(494, 516), S(522, 509), S(538, 507), S(493, 509),
            S(453, 516), S(466, 515), S(484, 525), S(503, 513), S(501, 514), S(512, 513), S(469, 511), S(457, 514),
            S(441, 515), S(451, 517), S(465, 520), S(476, 516), S(486, 507), S(470, 506), S(483, 504), S(454, 501),
            S(432, 508), S(452, 512), S(461, 507), S(460, 511), S(480, 505), S(477, 500), S(472, 504), S(444, 496),
            S(433, 506), S(461, 506), S(457, 512), S(468, 514), S(476, 503), S(488, 503), S(471, 501), S(406, 509),
            S(458, 503), S(464, 514), S(478, 515), S(494, 511), S(493, 507), S(484, 499), S(440, 516), S(451, 492),
        },
        {
            S(997, 927), S(1025, 958), S(1054, 958), S(1037, 963), S(1084, 963), S(1069, 955), S(1068, 946), S(1070, 956),
            S(1001, 919), S(986, 956), S(1020, 968), S(1026, 977), S(1009, 994), S(1082, 961), S(1053, 966), S(1079, 936),
            S(1012, 916), S(1008, 942), S(1032, 945), S(1033, 985), S(1054, 983), S(1081, 971), S(1072, 955), S(1082, 945),
            S(998, 939), S(998, 958), S(1009, 960), S(1009, 981), S(1024, 993), S(1042, 976), S(1023, 993), S(1026, 972),
            S(1016, 918), S(999, 964), S(1016, 955), S(1015, 983), S(1023, 967), S(1021, 970), S(1028, 975), S(1022, 959),
            S(1011, 920), S(1027, 909), S(1014, 951), S(1023, 942), S(1020, 945), S(1027, 953), S(1039, 946), S(1030, 941),
            S(990, 914), S(1017, 913), S(1036, 906), S(1027, 920), S(1033, 920), S(1040, 913), S(1022, 900), S(1026, 904),
            S(1024, 903), S(1007, 908), S(1016, 914), S(1035, 893), S(1010, 931), S(1000, 904), S(994, 916), S(975, 895),
        },
        {
            S(-65, -74), S(23, -35), S(16, -18), S(-15, -18), S(-56, -11), S(-34, 15), S(2, 4), S(13, -17),
            S(29, -12), S(-1, 17), S(-20, 14), S(-7, 17), S(-8, 17), S(-4, 38), S(-38, 23), S(-29, 11),
            S(-9, 10), S(24, 17), S(2, 23), S(-16, 15), S(-20, 20), S(6, 45), S(22, 44), S(-22, 13),
            S(-17, -8), S(-20, 22), S(-12, 24), S(-27, 27), S(-30, 26), S(-25, 33), S(-14, 26), S(-36, 3),
            S(-49, -18), S(-1, -4), S(-27, 21), S(-39, 24), S(-46, 27), S(-44, 23), S(-33, 9), S(-51, -11),
            S(-14, -19), S(-14, -3), S(-22, 11), S(-46, 21), S(-44, 23), S(-30, 16), S(-15, 7), S(-27, -9),
            S(1, -27), S(7, -11), S(-8, 4), S(-64, 13), S(-43, 14), S(-16, 4), S(9, -5), S(8, -17),
            S(-15, -53), S(36, -34), S(12, -21), S(-54, -11), S(8, -28), S(-28, -14), S(24, -24), S(14, -43),
        }
    };

    static int[] gamePhaseInc = { 0, 1, 1, 2, 4, 0 };

    public static int Eval(Board board)
    {
        int[] scoreArray = { 0, 0 };
        int gamePhase = 0;

        for (int sq = 0; sq < 64; sq++)
        {
            Piece piece = board.GetPiece(new Square(sq));
            if (piece.PieceType != PieceType.None)
            {
                int color = piece.IsWhite ? 0 : 1;
                int xor = piece.IsWhite ? 56 : 0;
                int pieceInt = (int)piece.PieceType - 1;
                scoreArray[color] += pst[pieceInt, sq ^ xor];
                gamePhase += gamePhaseInc[pieceInt];
            }
        }

        int stm = board.IsWhiteToMove ? 0 : 1;
        int score = scoreArray[stm] - scoreArray[stm ^ 1];
        int mgScore = unpack_mg(score);
        int egScore = unpack_eg(score);

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
    static int[] deltas = { 180, 400, 450, 550, 1100 };

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

                if (standPat + deltas[(int)move.CapturePieceType - 1] < alpha)
                {
                    return alpha;
                }

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