using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Chess
{
    public class Chess
    {
        public ChessBoard Board { get; private set; }
        public Player Turn { get; private set; }
        public position_t Selection { get; private set; }

        private UIBoard m_UI;
        private int m_nPlayers;

        public Chess(UIBoard ui, int nPlayers = 1, bool setupBoard = true)
        {
            this.m_UI = ui;
            this.m_UI.SetStatus(true, "Generating...");


            // numar jucatori = {0, 1, 2}
            this.m_nPlayers = nPlayers;
            // albul intotdeauna incepe
            this.Turn = Player.JEDI;

            this.Board = new ChessBoard();
            if (setupBoard)
            {
                this.Board.SetInitialPlacement();
            }

            this.m_UI.SetBoard(Board);
            this.m_UI.SetStatus(false, "Jedi's turn.");
        }

        public void AISelect()
        {
            while (AI.RUNNING)
            {
                Thread.Sleep(100);
            }

            this.m_UI.SetStatus(true, "Thinking...");

            move_t move = AI.MiniMaxAB(this.Board, this.Turn);

            if (move.to.letter >= 0 && move.to.number >= 0)
            {
                MakeMove(move);
            }
            else 
            {
                if (!AI.STOP) 
                {
                    this.m_UI.LogMove("Null Move\n");
                }
            }

            bool checkmate = false;

            if (!AI.STOP)
            {
                switchPlayer();
                checkmate = detectCheckmate();
            }

            AI.RUNNING = false;

            if (!AI.STOP && this.m_nPlayers == 0 && !checkmate)
            {
                new Thread(AISelect).Start();
            }
        }

        public List<position_t> Select(position_t pos)
        {
            // daca a fost mutat
            if (this.Board.Grid[this.Selection.number][this.Selection.letter].piece != Piece.NONE
                && this.Turn == this.Board.Grid[this.Selection.number][this.Selection.letter].player
                && (this.m_nPlayers == 2 
                || this.Turn == Player.JEDI)) 
            {
                // preluam toate mutarile legale si verificam daca facem acest lucru
                List<position_t> moves = LegalMoveSet.getLegalMove(this.Board, this.Selection);
                foreach (position_t move in moves)
                {
                    if (move.Equals(pos))
                    {
                        // am efectuat mutarea respectiva
                        MakeMove(new move_t(this.Selection, pos));

                        if (this.Board.Grid[pos.number][pos.letter].piece == Piece.GRANDMASTER && Math.Abs(pos.letter - this.Selection.letter) == 2)
                        {
                            int row = (this.Turn == Player.JEDI) ? 0 : 7;

                            if (pos.letter < 4)
                            {
                                LegalMoveSet.move(this.Board, new move_t(new position_t(0, row), new position_t(3, row)));
                            }
                            else
                            {
                                LegalMoveSet.move(this.Board, new move_t(new position_t(7, row), new position_t(5, row)));
                            }
                        }
                                
                        // mutarea finala
                        switchPlayer();
                        if (detectCheckmate()) return new List<position_t>();

                        if (this.m_nPlayers == 1) 
                        {
                            new Thread(AISelect).Start();
                        }
                        return new List<position_t>();
                    }
                }
            }

            // clic, vom vedea mutarile care pot fi posibile
            if (this.Board.Grid[pos.number][pos.letter].player == this.Turn && (this.m_nPlayers == 2 || this.Turn == Player.JEDI))
            {
                List<position_t> moves = LegalMoveSet.getLegalMove(this.Board, pos);
                this.Selection = pos;
                return moves;
            }

            // reset
            this.Selection = new position_t();
            return new List<position_t>();
        }

        private void MakeMove(move_t m)
        {
            // in casuta din dreapta tablei vom vedea mutarile efectuate
            string move = (this.Turn == Player.JEDI) ? "\nW" : "\nB";

            move += ":\t";

            // piesa
            switch (this.Board.Grid[m.from.number][m.from.letter].piece)
            {
                case Piece.TROOPS:
                    move += "T";
                    break;
                case Piece.PADAWAN:
                    move += "P";
                    break;
                case Piece.KNIGHT:
                    move += "K";
                    break;
                case Piece.ADEPT:
                    move += "A";
                    break;
                case Piece.MASTER:
                    move += "M";
                    break;
                case Piece.GRANDMASTER:
                    move += "G";
                    break;
            }

            // elimina
            if (this.Board.Grid[m.to.number][m.to.letter].piece != Piece.NONE || LegalMoveSet.isEnPassant(this.Board, m))
            {
                move += "x";
            }

            // litere
            switch (m.to.letter)
            {
                case 0: move += "a"; break;
                case 1: move += "b"; break;
                case 2: move += "c"; break;
                case 3: move += "d"; break;
                case 4: move += "e"; break;
                case 5: move += "f"; break;
                case 6: move += "g"; break;
                case 7: move += "h"; break;
            }

            // numere
            move += (m.to.number + 1).ToString();

            // actualizeaza tabla cu piesele mutate
            this.Board = LegalMoveSet.move(this.Board, m);

            // daca cineva este in sah
            if (LegalMoveSet.isCheck(this.Board, (Turn == Player.JEDI) ? Player.SITH : Player.JEDI))
            {
                move += "+";
            }

            this.m_UI.LogMove(move + "\n");
        }

        private void switchPlayer()
        {
            this.Turn = (this.Turn == Player.JEDI) ? Player.SITH : Player.JEDI;
            this.m_UI.SetTurn(this.Turn);
            this.m_UI.SetStatus(false, ((this.Turn == Player.JEDI) ? "Jedi" : "Sith") + "'s Turn.");
            this.m_UI.SetBoard(this.Board);
        }

        public bool detectCheckmate()
        {
            bool wkingonly = this.Board.Pieces[Player.JEDI].Count == 1 && this.Board.Grid[this.Board.Pieces[Player.JEDI][0].number][this.Board.Pieces[Player.JEDI][0].letter].piece == Piece.GRANDMASTER;
            bool bkingonly = this.Board.Pieces[Player.SITH].Count == 1 && this.Board.Grid[this.Board.Pieces[Player.SITH][0].number][this.Board.Pieces[Player.SITH][0].letter].piece == Piece.GRANDMASTER;

            if (!LegalMoveSet.hasMoves(this.Board, this.Turn))
            {
                if (LegalMoveSet.isCheck(this.Board, this.Turn))
                {
                    this.m_UI.LogMove("Checkmate!\n");
                    this.m_UI.SetStatus(false, ((this.Turn == Player.JEDI) ? "Black" : "White") + " wins!");
                }
                else
                {
                    this.m_UI.LogMove("Stalemate!\n");
                }
                return true;
            }
            else if (wkingonly && bkingonly)
            {
                this.m_UI.LogMove("Draw.\n");
                return true;
            }
            return false;
        }
    }
}
