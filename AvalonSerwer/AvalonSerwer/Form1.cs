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
using System.Threading;
using System.Collections;

namespace AvalonSerwer
{
    public partial class Serwer : Form
    {
        int port;
        TcpListener serwer;
        ArrayList clientslist;
        ArrayList nameslist;
        Thread listen;
        TcpClient klient;
        TcpClient[] players;
        string[] nicks;
        bool gameRunning;
        public Serwer()
        {
            InitializeComponent();
            clientslist = new ArrayList();
            nameslist = new ArrayList();
            players = new TcpClient[10];
            nicks = new string[10];
        }

        private void ServerStartButton_Click(object sender, EventArgs e)
        {
            port = Convert.ToInt32(PortTextBox.Text);
            serwer = new TcpListener(IPAddress.Any, port);
            listen = new Thread(Listen);
            listen.Start();
        }

        private void Listen()
        {
            try
            {
                serwer.Start();
                while(true)
                {
                    klient = serwer.AcceptTcpClient();
                    nameslist.Add(klient);
                    Thread watekKlienta = new Thread(Akcja);
                    clientslist.Add(watekKlienta);
                    watekKlienta.Start();
                }
            }
            catch
            {
                serwer.Stop();
                MessageBox.Show("Serwer kaput");
                Application.Exit();
                listen.Abort();
            }
        }

        delegate void AkcjaDelegate();
        private void Akcja()
        {
        }

        private void ServerStopButton_Click(object sender, EventArgs e)
        {
        }
    }
}
