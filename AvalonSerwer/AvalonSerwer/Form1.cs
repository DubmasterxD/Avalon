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
        TcpListener server;
        Thread listen;
        ArrayList threadList;
        ArrayList clientList;
        TcpClient newClient;
        TcpClient leader;
        TcpClient[] players;
        string[] nicks;
        int numberOfPlayers;
        bool gameRunning;

        public Serwer()
        {
            InitializeComponent();
            threadList = new ArrayList();
            clientList = new ArrayList();
            players = new TcpClient[10];
            nicks = new string[10];
            numberOfPlayers = 0;
            gameRunning = false;
        }

        private void ServerStartButton_Click(object sender, EventArgs e)
        {
            port = Convert.ToInt32(PortTextBox.Text);
            server = new TcpListener(IPAddress.Any, port);
            listen = new Thread(Listen);
            listen.Start();
            ServerStartButton.Visible = false;
            ServerStopButton.Visible = true;
        }

        private void Listen()
        {
            try
            {
                server.Start();
                while(true)
                {
                    newClient = server.AcceptTcpClient();
                    clientList.Add(newClient);
                    Thread clientThread = new Thread(ClientAction);
                    threadList.Add(clientThread);
                    clientThread.Start();
                }
            }
            catch
            {
                server.Stop();
            }
        }

        private void ClientAction()
        {
            TcpClient Player = newClient;
            NetworkStream ns = newClient.GetStream();
            BinaryReader br = new BinaryReader(ns);
            BinaryWriter bw = new BinaryWriter(ns);
            string nick = "";
            int seat = 0;
            string cmd = "";
            try
            {
                while ((cmd=br.ReadString())!="disconnect")
                {
                    switch (cmd)
                    {
                        case "nick":
                            nick = br.ReadString();
                            bw.Write("seatstaken");
                            for (int i = 0; i < nicks.Length; i++)
                            {
                                if (nicks[i] != "" && nicks[i] != null)
                                {
                                    bw.Write((i + 1).ToString());
                                    bw.Write(nicks[i]);
                                }
                            }
                            bw.Write("end");
                            break;
                        case "takeseat":
                            int askingSeat = Convert.ToInt16(br.ReadString());
                            if (nicks[askingSeat - 1] == "" || nicks[askingSeat - 1] == null)
                            {
                                nicks[askingSeat - 1] = nick;
                                players[askingSeat - 1] = Player;
                                numberOfPlayers++;
                                SendToAll("seattaken");
                                SendToAll(askingSeat.ToString());
                                SendToAll(nicks[askingSeat - 1]);
                                bw.Write("seataccepted");
                                bw.Write(askingSeat.ToString());
                                seat = askingSeat;
                                if(askingSeat==1)
                                {
                                    leader = Player;
                                }
                                if(numberOfPlayers>=5)
                                {
                                    if (leader != null && leader.Connected)
                                    {
                                        NetworkStream leaderStream = leader.GetStream();
                                        BinaryWriter leaderWriter = new BinaryWriter(leaderStream);
                                        leaderWriter.Write("ableToStart");
                                    }
                                }
                            }
                            break;
                        case "stand":
                            if (seat != 0)
                            {
                                SendToAll("seatfree");
                                SendToAll(seat.ToString());
                                players[seat - 1] = null;
                                nicks[seat - 1] = null;
                                numberOfPlayers--;
                            }
                            if (seat == 1)
                            {
                                leader = null;
                                bw.Write("unableToStart");
                            }
                            seat = 0;
                            bw.Write("seatstaken");
                            for (int i = 0; i < nicks.Length; i++)
                            {
                                if (nicks[i] != "" && nicks[i] != null)
                                {
                                    bw.Write((i + 1).ToString());
                                    bw.Write(nicks[i]);
                                }
                            }
                            bw.Write("end");
                            if(numberOfPlayers<5)
                            {
                                if (leader != null && leader.Connected)
                                {
                                    NetworkStream leaderStream = leader.GetStream();
                                    BinaryWriter leaderWriter = new BinaryWriter(leaderStream);
                                    leaderWriter.Write("unableToStart");
                                }
                            }
                            break;
                        case "startChoose":
                            SendToAll("chooseStarted");
                            SendToAll(numberOfPlayers.ToString());
                            gameRunning = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {

            }
            finally
            {
                Player.Close();
            }
        }

        private void SendToAll(string message)
        {
            NetworkStream ns;
            BinaryWriter bw;
            foreach (TcpClient user in clientList)
            {
                if (user.Connected)
                {
                    ns = user.GetStream();
                    bw = new BinaryWriter(ns);
                    bw.Write(message);

                }
            }
        }

        private void ServerStopButton_Click(object sender, EventArgs e)
        {
            SendToAll("disconnect");
            server.Stop();
            Application.Exit();
        }

        private void Serwer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                SendToAll("disconnect");
                server.Stop();
            }
        }
    }
}
