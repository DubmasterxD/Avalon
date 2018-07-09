using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;

namespace Avalon
{
    public partial class Game : Form
    {
        TcpClient klient;
        string adresip;
        int port;

        public Game()
        {
            InitializeComponent();
        }

        void Polacz()
        {
            if (NickTextBox.Text != "")
            {
                try
                {
                    adresip = IPTextBox.Text;
                    port = Convert.ToInt32(PortTextBox.Text);
                    klient = new TcpClient(adresip, port);
                    //NetworkStream ns = klient.GetStream();
                    //bw = new BinaryWriter(ns);
                    //Thread paczajka = new Thread(Patrz);
                    //paczajka.Start();
                    IPTextBox.Visible = false;
                    IPLabel.Visible = false;
                    PortLabel.Visible = false;
                    PortTextBox.Visible = false;
                    ConnectButton.Visible = false;
                    Connectbckg.Visible = false;
                    NickLabel.Visible = false;
                    NickTextBox.Visible = false;
                    NoNickLabel.Visible = false;
                }
                catch
                {
                    MessageBox.Show("Najpierw serwer!");
                }
            }
            else
            {
                NoNickLabel.Visible = true;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            ExitButton.BackgroundImage = null;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            ExitButton.BackgroundImage = Avalon.Properties.Resources.xHighlight;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = null;
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = Avalon.Properties.Resources.minimizeHighlight;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            Polacz();
        }
    }
}
