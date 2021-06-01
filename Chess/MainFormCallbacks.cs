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
        private ToolStripMenuItem temp; 
        TimeSpan m_whiteTime = new TimeSpan(0);
        TimeSpan m_blackTime = new TimeSpan(0);

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateBoard();

            picTurn.SizeMode = PictureBoxSizeMode.StretchImage;
            picTurn.Image = graphics.TurnIndicator[Player.JEDI];

            temp = mnuDif3;
            AI.DEPTH = 3;

            SetStatus(false, "Choose New Game.");

            endCurrentGameToolStripMenuItem.Enabled = false;
        }

        private void windowClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }

        private void Shutdown(object sender, EventArgs e)
        {
            Stop();
            this.Close();
        }

        private void endGame(object sender, EventArgs e)
        {
            Stop();
        }

        private void NewGame(object sender, EventArgs e)
        {
            ToolStripMenuItem button = (ToolStripMenuItem)sender;
            if (button.Text.StartsWith("New AI vs AI"))
            {
                NewGame(0);
            }
            else if (button.Text.StartsWith("New AI vs Player"))
            {
                NewGame(1);
            }
            else if (button.Text.StartsWith("New Player"))
            {
                NewGame(2);
            }
        }

        private void Difficulty(object sender, EventArgs e)
        {
            if (temp != null)
            {
                temp.CheckState = CheckState.Unchecked;
            }

            bool was = AI.RUNNING;
            AI.STOP = true;

            temp = (ToolStripMenuItem)sender;
            temp.CheckState = CheckState.Checked;

            AI.DEPTH = Int32.Parse((String)temp.Tag);
            LogMove("AI Difficulty " + (string)temp.Tag + "\n");

            if (was)
            {
                LogMove("AI Replaying Move\n");
                new Thread(chess.AISelect).Start();
            }
        }

        private void tmrWhite_Tick(object sender, EventArgs e)
        {
            m_whiteTime = m_whiteTime.Add(new TimeSpan(0, 0, 0, 0, tmrWhite.Interval));
            lblWhiteTime.Text = string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d1}", m_whiteTime.Hours, m_whiteTime.Minutes, m_whiteTime.Seconds, m_whiteTime.Milliseconds / 100);
        }

        private void tmrBlack_Tick(object sender, EventArgs e)
        {
            m_blackTime = m_blackTime.Add(new TimeSpan(0, 0, 0, 0, tmrBlack.Interval));
            lblBlackTime.Text = string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d1}", m_blackTime.Hours, m_blackTime.Minutes, m_blackTime.Seconds, m_blackTime.Milliseconds / 100);
        }
    }
}
