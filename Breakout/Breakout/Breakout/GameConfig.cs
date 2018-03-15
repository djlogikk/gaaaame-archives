using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Breakout
{
    public partial class GameConfig : Form
    {
        private Game1 m_game;

        public GameConfig(Game1 theGame)
        {
            m_game = theGame;

            InitializeComponent();
        }

        private void resCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_game.ChangeResolution(resCombo.SelectedIndex);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            m_game.SetFullScreen(checkBox1.Checked);
        }

        private void GameConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            e.Cancel = true;
        }

        private void GameConfig_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void GameConfig_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void GameConfig_Deactivate(object sender, EventArgs e)
        {
            m_game.SupressGameKeys(false);
        }

        private void GameConfig_Activated(object sender, EventArgs e)
        {
            m_game.SupressGameKeys(true);
        }
    }
}
