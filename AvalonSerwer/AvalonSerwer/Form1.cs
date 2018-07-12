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
        TcpClient[] players;
        string[] nicks;
        int numberOfPlayers;
        bool withLady;
        bool withPersifal;
        bool withMordred;
        bool withMorgana;
        bool withOberon;
        int numberOfEvilAdds;
        int[] info;
        bool gameRunning;
        int[] seatsTaken;
        Random rnd = new Random();
        Random rnd2 = new Random();
        int leader;
        string[] roles;
        int lady;
        int currRound;
        List<int> team;

        public Serwer()
        {
            InitializeComponent();
            threadList = new ArrayList();
            clientList = new ArrayList();
            players = new TcpClient[10];
            nicks = new string[10];
            numberOfPlayers = 0;
            gameRunning = false;
            numberOfEvilAdds = 0;
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
                while (true)
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
            int seat = -1;
            string cmd = "";
            try
            {
                while ((cmd = br.ReadString()) != "disconnect")
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
                                    bw.Write((i).ToString());
                                    bw.Write(nicks[i]);
                                }
                            }
                            bw.Write("end");
                            break;
                        case "takeseat":
                            int askingSeat = Convert.ToInt16(br.ReadString());
                            if (nicks[askingSeat] == "" || nicks[askingSeat] == null)
                            {
                                nicks[askingSeat] = nick;
                                players[askingSeat] = Player;
                                numberOfPlayers++;
                                SendToAll("seattaken");
                                SendToAll(askingSeat.ToString());
                                SendToAll(nicks[askingSeat]);
                                bw.Write("seataccepted");
                                bw.Write(askingSeat.ToString());
                                seat = askingSeat;
                                if (numberOfPlayers >= 5)
                                {

                                    SendToPlayer("ableToStart", 0);
                                }
                            }
                            break;
                        case "stand":
                            if (seat != -1)
                            {
                                SendToAll("seatfree");
                                SendToAll(seat.ToString());
                                players[seat] = null;
                                nicks[seat] = null;
                                numberOfPlayers--;
                            }
                            if (seat == 0)
                            {
                                bw.Write("unableToStart");
                            }
                            seat = -1;
                            bw.Write("seatstaken");
                            for (int i = 0; i < nicks.Length; i++)
                            {
                                if (nicks[i] != "" && nicks[i] != null)
                                {
                                    bw.Write((i).ToString());
                                    bw.Write(nicks[i]);
                                }
                            }
                            bw.Write("end");
                            if (numberOfPlayers < 5)
                            {
                                SendToPlayer("unableToStart", 0);
                            }
                            break;
                        case "startChoose":
                            rnd2 = new Random();
                            SendToAll("chooseStarted");
                            switch (numberOfPlayers)
                            {
                                case 5:
                                    info = new int[] { 2, 3, 2, 3, 3, 3, 2 };
                                    break;
                                case 6:
                                    info = new int[] { 2, 3, 4, 3, 4, 4, 2 };
                                    break;
                                case 7:
                                    info = new int[] { 2, 3, 3, 4, 4, 4, 3 };
                                    break;
                                case 8:
                                    info = new int[] { 3, 4, 4, 5, 5, 5, 3 };
                                    break;
                                case 9:
                                    info = new int[] { 3, 4, 4, 5, 5, 6, 3 };
                                    break;
                                case 10:
                                    info = new int[] { 3, 4, 4, 5, 5, 6, 4 };
                                    break;
                            }
                            SendToAll(info[0].ToString());
                            SendToAll(info[1].ToString());
                            SendToAll(info[2].ToString());
                            SendToAll(info[3].ToString());
                            SendToAll(info[4].ToString());
                            SendToAll(info[5].ToString());
                            SendToAll(info[6].ToString());
                            gameRunning = true;
                            break;
                        case "ladyChosen":
                            if (seat == 0)
                            {
                                if (withLady)
                                {
                                    withLady = false;
                                    SendToAll("addChosen");
                                    SendToAll("lady");
                                    SendToAll("false");
                                }
                                else
                                {
                                    withLady = true;
                                    SendToAll("addChosen");
                                    SendToAll("lady");
                                    SendToAll("true");
                                }
                            }
                            break;
                        case "persifalChosen":
                            if (seat == 0)
                            {
                                if (withPersifal)
                                {
                                    withPersifal = false;
                                    SendToAll("addChosen");
                                    SendToAll("persifal");
                                    SendToAll("false");
                                }
                                else
                                {
                                    withPersifal = true;
                                    SendToAll("addChosen");
                                    SendToAll("persifal");
                                    SendToAll("true");
                                }
                            }
                            break;
                        case "mordredChosen":
                            if (seat == 0)
                            {
                                if (withMordred)
                                {
                                    withMordred = false;
                                    numberOfEvilAdds--;
                                    SendToAll("addChosen");
                                    SendToAll("mordred");
                                    SendToAll("false");
                                }
                                else
                                {
                                    if (numberOfEvilAdds < (info[6] - 1))
                                    {
                                        withMordred = true;
                                        numberOfEvilAdds++;
                                        SendToAll("addChosen");
                                        SendToAll("mordred");
                                        SendToAll("true");
                                    }
                                }
                            }
                            break;
                        case "morganaChosen":
                            if (seat == 0)
                            {
                                if (withMorgana)
                                {
                                    withMorgana = false;
                                    numberOfEvilAdds--;
                                    SendToAll("addChosen");
                                    SendToAll("morgana");
                                    SendToAll("false");
                                }
                                else
                                {
                                    if (numberOfEvilAdds < (info[6] - 1))
                                    {
                                        withMorgana = true;
                                        numberOfEvilAdds++;
                                        SendToAll("addChosen");
                                        SendToAll("morgana");
                                        SendToAll("true");
                                    }
                                }
                            }
                            break;
                        case "oberonChosen":
                            if (seat == 0)
                            {
                                if (withOberon)
                                {
                                    withOberon = false;
                                    numberOfEvilAdds--;
                                    SendToAll("addChosen");
                                    SendToAll("oberon");
                                    SendToAll("false");
                                }
                                else
                                {
                                    if (numberOfEvilAdds < (info[6] - 1))
                                    {
                                        withOberon = true;
                                        numberOfEvilAdds++;
                                        SendToAll("addChosen");
                                        SendToAll("oberon");
                                        SendToAll("true");
                                    }
                                }
                            }
                            break;
                        case "startGame":
                            currRound = 1;
                            SendToAll("gameStarted");
                            seatsTaken = new int[numberOfPlayers];
                            int h = 0;
                            for (int i = 0; i < nicks.Length; i++)
                            {
                                if (nicks[i] != null && nicks[i] != "")
                                {
                                    seatsTaken[h] = i;
                                    h++;
                                }
                            }
                            leader = rnd.Next(numberOfPlayers);
                            leader = 0;
                            SendToAll(seatsTaken[leader].ToString());
                            if (withLady)
                            {
                                SendToAll("true");
                                lady = leader - 1;
                                if (lady == -1)
                                {
                                    lady = numberOfPlayers - 1;
                                }
                                SendToAll(seatsTaken[lady].ToString());
                            }
                            else
                            {
                                SendToAll("false");
                            }
                            roles = new string[numberOfPlayers];
                            for (int i = 0; i < numberOfPlayers; i++)
                            {
                                roles[i] = "";
                            }
                            string[] freeRoles = new string[numberOfPlayers];
                            freeRoles[0] = "Merlin";
                            freeRoles[1] = "Skrytobójca";
                            if (withPersifal)
                            {
                                freeRoles[2] = "Persifal";
                            }
                            else
                            {
                                freeRoles[2] = "Good";
                            }
                            for (int i = 3; i <= info[5]; i++)
                            {
                                freeRoles[i] = "Good";
                            }
                            for (int i = numberOfPlayers - 1; i > numberOfPlayers - info[6]; i--)
                            {
                                if (withMordred)
                                {
                                    freeRoles[i] = "Mordred";
                                    withMordred = false;
                                }
                                else if (withMorgana)
                                {
                                    freeRoles[i] = "Morgana";
                                    withMorgana = false;
                                }
                                else if (withOberon)
                                {
                                    freeRoles[i] = "Oberon";
                                    withOberon = false;
                                }
                                else
                                {
                                    freeRoles[i] = "Evil";
                                }
                            }
                            int k = 0;
                            int l;
                            while (k < numberOfPlayers)
                            {
                                l = rnd2.Next(numberOfPlayers);
                                if (roles[l] == "")
                                {
                                    roles[l] = freeRoles[k];
                                    k++;
                                }
                            }
                            for (int i = 0; i < numberOfPlayers; i++)
                            {
                                SendToPlayer(roles[i], seatsTaken[i]);
                                if (roles[i] == "Merlin")
                                {
                                    for (int j = 0; j < numberOfPlayers; j++)
                                    {
                                        if (roles[j] == "Skrytobójca" || roles[j] == "Oberon" || roles[j] == "Morgana" || roles[j] == "Evil")
                                        {
                                            SendToPlayer(seatsTaken[j].ToString(), seatsTaken[i]);
                                        }
                                    }
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                                else if (roles[i] == "Persifal")
                                {
                                    for (int j = 0; j < numberOfPlayers; j++)
                                    {
                                        if (roles[j] == "Merlin" || roles[j] == "Morgana")
                                        {
                                            SendToPlayer(seatsTaken[j].ToString(), seatsTaken[i]);
                                        }
                                    }
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                                else if (roles[i] == "Skrytobójca")
                                {
                                    for (int j = 0; j < numberOfPlayers; j++)
                                    {
                                        if (roles[j] == "Mordred" || roles[j] == "Morgana" || roles[j] == "Evil")
                                        {
                                            SendToPlayer(seatsTaken[j].ToString(), seatsTaken[i]);
                                        }
                                    }
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                                else if (roles[i] == "Mordred")
                                {
                                    for (int j = 0; j < numberOfPlayers; j++)
                                    {
                                        if (roles[j] == "Skrytobójca" || roles[j] == "Morgana" || roles[j] == "Evil")
                                        {
                                            SendToPlayer(seatsTaken[j].ToString(), seatsTaken[i]);
                                        }
                                    }
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                                else if (roles[i] == "Morgana")
                                {
                                    for (int j = 0; j < numberOfPlayers; j++)
                                    {
                                        if (roles[j] == "Skrytobójca" || roles[j] == "Mordred" || roles[j] == "Evil")
                                        {
                                            SendToPlayer(seatsTaken[j].ToString(), seatsTaken[i]);
                                        }
                                    }
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                                else if (roles[i] == "Evil")
                                {
                                    for (int j = 0; j < numberOfPlayers; j++)
                                    {
                                        if (roles[j] == "Skrytobójca" || roles[j] == "Mordred" || roles[j] == "Morgana" || roles[j] == "Evil")
                                        {
                                            if (i != j)
                                            {
                                                SendToPlayer(seatsTaken[j].ToString(), seatsTaken[i]);
                                            }
                                        }
                                    }
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                                else
                                {
                                    SendToPlayer("end", seatsTaken[i]);
                                }
                            }
                            team = new List<int>();
                            break;
                        case "addToTeam":
                            team.Add(Convert.ToInt16(br.ReadString()));
                            SendToAll("added");
                            SendToAll(team.Last().ToString());
                            if(team.Count()==info[currRound-1])
                            {
                                bw.Write("fullTeam");
                            }
                            break;
                        case "removeFromTeam":
                            int removed = Convert.ToInt16(br.ReadString());
                            team.Remove(removed);
                            SendToAll("removed");
                            SendToAll(removed.ToString());
                            if(team.Count==info[currRound-1]-1)
                            {
                                bw.Write("canAdd");
                                foreach(int seattaken in team)
                                {
                                    bw.Write(seattaken.ToString());
                                }
                                bw.Write("end");
                            }
                            break;
                        case "tryTeam":
                            SendToAll("voteTeam");
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

        private void SendToPlayer(string message, int seat)
        {
            if (players[seat] != null && players[seat].Connected)
            {
                NetworkStream ns = players[seat].GetStream();
                BinaryWriter bw = new BinaryWriter(ns);
                bw.Write(message);
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
