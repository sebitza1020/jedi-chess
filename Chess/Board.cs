using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess
{
    public class ChessBoard
    {
        private static int[] pieceWeights = { 1, 3, 4, 5, 7, 20 };

        public piece_t[][] Grid { get; private set; }
        public Dictionary<Player, position_t> Kings { get; private set; }
        public Dictionary<Player, List<position_t>> Pieces { get; private set; }
        public Dictionary<Player, position_t> LastMove { get; private set; }

        public ChessBoard()
        {
            // init tabla goala
            Grid = new piece_t[8][];
            for (int i = 0; i < 8; i++)
            {
                Grid[i] = new piece_t[8];
                for (int j = 0; j < 8; j++)
                    Grid[i][j] = new piece_t(Piece.NONE, Player.JEDI);
            }

            // init ultimele mutari
            LastMove = new Dictionary<Player, position_t>();
            LastMove[Player.SITH] = new position_t();
            LastMove[Player.JEDI] = new position_t();

            // init pozitii rege
            Kings = new Dictionary<Player, position_t>();

            // init lista cu pozitii ale pieselor
            Pieces = new Dictionary<Player, List<position_t>>();
            Pieces.Add(Player.SITH, new List<position_t>());
            Pieces.Add(Player.JEDI, new List<position_t>());
        }

        public ChessBoard(ChessBoard copy)
        {
            // init lista cu pozitii ale pieselor
            Pieces = new Dictionary<Player, List<position_t>>();
            Pieces.Add(Player.SITH, new List<position_t>());
            Pieces.Add(Player.JEDI, new List<position_t>());

            // init copiere tabla
            Grid = new piece_t[8][];
            for (int i = 0; i < 8; i++)
            {
                Grid[i] = new piece_t[8];
                for (int j = 0; j < 8; j++)
                {
                    Grid[i][j] = new piece_t(copy.Grid[i][j]);

                    // adaugarea pozitiei piesei in lista
                    if (Grid[i][j].piece != Piece.NONE)
                        Pieces[Grid[i][j].player].Add(new position_t(j, i));
                }
            }

            // copiere ultima mutare
            LastMove = new Dictionary<Player, position_t>();
            LastMove[Player.SITH] = new position_t(copy.LastMove[Player.SITH]);
            LastMove[Player.JEDI] = new position_t(copy.LastMove[Player.JEDI]);

            // copiere pozitii rege
            Kings = new Dictionary<Player, position_t>();
            Kings[Player.SITH] = new position_t(copy.Kings[Player.SITH]);
            Kings[Player.JEDI] = new position_t(copy.Kings[Player.JEDI]);
        }

        public int fitness(Player max)
        {
            int fitness = 0;
            int[] SITHPieces = { 0, 0, 0, 0, 0, 0 };
            int[] JEDIPieces = { 0, 0, 0, 0, 0, 0 };
            int SITHMoves = 0;
            int JEDIMoves = 0;

            // adunare numar piese si mutari
            foreach (position_t pos in Pieces[Player.SITH])
            {
                SITHMoves += LegalMoveSet.getLegalMove(this, pos).Count;
                SITHPieces[(int)Grid[pos.number][pos.letter].piece]++;
            }

            // adunare numar piese si mutari
            foreach (position_t pos in Pieces[Player.JEDI])
            {
                JEDIMoves += LegalMoveSet.getLegalMove(this, pos).Count;
                JEDIPieces[(int)Grid[pos.number][pos.letter].piece]++;
            }

            
            if (max == Player.SITH)
            {
                
                for (int i = 0; i < 6; i++)
                {
                    fitness += pieceWeights[i] * (SITHPieces[i] - JEDIPieces[i]);
                }

               
                fitness += (int)(0.5 * (SITHMoves - JEDIMoves));
            }
            else
            {
                
                for (int i = 0; i < 6; i++)
                {
                    fitness += pieceWeights[i] * (JEDIPieces[i] - SITHPieces[i]);
                }

                
                fitness += (int)(0.5 * (JEDIMoves - SITHMoves));
            }

            return fitness;
        }

        public void SetInitialPlacement()
        {
            for (int i = 0; i < 8; i++)
            {
                SetPiece(Piece.TROOPS, Player.JEDI, i, 1);
            }

            SetPiece(Piece.PADAWAN, Player.JEDI, 0, 0);
            SetPiece(Piece.PADAWAN, Player.JEDI, 7, 0);

            SetPiece(Piece.KNIGHT, Player.JEDI, 1, 0);
            SetPiece(Piece.KNIGHT, Player.JEDI, 6, 0);

            SetPiece(Piece.ADEPT, Player.JEDI, 2, 0);
            SetPiece(Piece.ADEPT, Player.JEDI, 5, 0);

            SetPiece(Piece.GRANDMASTER, Player.JEDI, 4, 0);
            SetPiece(Piece.GRANDMASTER, Player.SITH, 4, 7);
            Kings[Player.JEDI] = new position_t(4, 0);
            Kings[Player.SITH] = new position_t(4, 7);
            SetPiece(Piece.MASTER, Player.JEDI, 3, 0);
            SetPiece(Piece.MASTER, Player.SITH, 3, 7);
        }

        public void SetPiece(Piece piece, Player player, int letter, int number)
        {
            // setare valoare casuta
            Grid[number][letter].piece = piece;
            Grid[number][letter].player = player;

            // adauga piesa in lista
            Pieces[player].Add(new position_t(letter, number));

            // actualizare pozitie rege
            if (piece == Piece.GRANDMASTER)
            {
                Kings[player] = new position_t(letter, number);
            }
        }
    }
}
