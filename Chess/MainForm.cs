using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Chess
{
    public partial class MainForm : Form, UIBoard
    {
        bool m_aigame = false;
        bool m_checkmate = false;
        bool m_manualBoard = false; 
        bool m_finalizedBoard = false;

        Chess chess;
       
        private void Stop()
        {
            SetStatus(false, "Choose New Game.");

            AI.STOP = true;
            chess = null;

            SetTurn(Player.JEDI);

            tmrWhite.Stop();
            tmrBlack.Stop();
            m_whiteTime = new TimeSpan(0);
            m_blackTime = new TimeSpan(0);
            lblWhiteTime.Text = m_whiteTime.ToString("c");
            lblBlackTime.Text = m_blackTime.ToString("c");

            SetBoard(new ChessBoard());
            txtLog.Text = "";

            m_checkmate = false;
            m_aigame = false;
            m_finalizedBoard = false;

            endCurrentGameToolStripMenuItem.Enabled = false;
        }

        private void NewGame(int nPlayers)
        {
            if (!m_manualBoard) Stop();

            m_aigame = (nPlayers == 0);
            chess = new Chess(this, nPlayers, !m_manualBoard);

            SetTurn(Player.JEDI);
            SetStatus(false, "Jedi's turn");

            m_whiteTime = new TimeSpan(0);
            m_blackTime = new TimeSpan(0);
            lblWhiteTime.Text = m_whiteTime.ToString("c");
            lblBlackTime.Text = m_blackTime.ToString("c");

            if (nPlayers < 2)
            {
                LogMove("AI Difficulty " + (string)temp.Tag + "\n");
            }
            
            SetStatus(false, "Jedi's Turn");
            if (m_aigame)
            {
                new Thread(chess.AISelect).Start();
            }
            tmrWhite.Start();

            endCurrentGameToolStripMenuItem.Enabled = true;
        }

        public void SetTurn(Player p)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetTurn(p)));
                return;
            }

            if (chess != null)
            {
                picTurn.Image = graphics.TurnIndicator[chess.Turn];
            }
            else
            {
                picTurn.Image = graphics.TurnIndicator[Player.JEDI];
            }

            if (!m_manualBoard)
            {
                if (p == Player.JEDI)
                {
                    tmrBlack.Stop();
                    tmrWhite.Start();
                }
                else
                {
                    tmrWhite.Stop();
                    tmrBlack.Start();
                }

                if (chess != null && (m_checkmate || chess.detectCheckmate()))
                {
                    tmrWhite.Stop();
                    tmrBlack.Stop();
                }
            }
        }

        public void SetBoard(ChessBoard board)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetBoard(board)));
                return;
            }

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    SetPiece(board.Grid[i][j].piece, board.Grid[i][j].player, j, i);
        }

        public void LogMove(string move)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LogMove(move)));
                return;
            }

            lblWhiteCheck.Visible = false;
            lblBlackCheck.Visible = false;

            if (move.Contains("+"))
            {
                lblWhiteCheck.Visible = chess.Turn == Player.SITH;
                lblBlackCheck.Visible = chess.Turn == Player.JEDI;
            }

            txtLog.AppendText(move);
        }

        public void SetStatus(bool thinking, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetStatus(thinking, message)));
                return;
            }

            lblStatus.Text = message;
            if (thinking)
            {
                prgThinking.MarqueeAnimationSpeed = 30;
                prgThinking.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                prgThinking.MarqueeAnimationSpeed = 0;
                prgThinking.Value = 0;
                prgThinking.Style = ProgressBarStyle.Continuous;
            }
        }
    }
}
