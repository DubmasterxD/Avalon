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
        TcpClient server;
        string ipAddress;
        int port;
        NetworkStream ns;
        BinaryWriter bw;
        BinaryReader br;
        int mySeat;
        int stage;
        int[] info;

        public Game()
        {
            InitializeComponent();
            mySeat = 0;
            stage = 0;
            info = new int[7];
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (NickTextBox.Text != "")
            {
                try
                {
                    ipAddress = IPTextBox.Text;
                    port = Convert.ToInt32(PortTextBox.Text);
                    server = new TcpClient(ipAddress, port);
                    ns = server.GetStream();
                    bw = new BinaryWriter(ns);
                    Thread serverConnection = new Thread(ServerConnection);
                    serverConnection.Start();
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
                    MessageBox.Show("Nie znaleziono serwera.");
                }
            }
            else
            {
                NoNickLabel.Visible = true;
            }
        }

        private void ServerConnection()
        {
            bw.Write("nick");
            bw.Write(NickTextBox.Text);
            br = new BinaryReader(ns);
            string cmd = "";
            try
            {
                while((cmd=br.ReadString())!="disconnect")
                {
                    switch(cmd)
                    {
                        case "seatstaken":
                            mySeat = 0;
                            Seat1Taken(false, "");
                            Seat2Taken(false, "");
                            Seat3Taken(false, "");
                            Seat4Taken(false, "");
                            Seat5Taken(false, "");
                            Seat6Taken(false, "");
                            Seat7Taken(false, "");
                            Seat8Taken(false, "");
                            Seat9Taken(false, "");
                            Seat10Taken(false, "");
                            string numbers = "";
                            while((numbers = br.ReadString()) != "end")
                            {
                                SeatTaken(Convert.ToInt16(numbers), true, br.ReadString());
                            }
                            break;
                        case "seattaken":
                            int number = Convert.ToInt16(br.ReadString());
                            SeatTaken(number, true, br.ReadString());
                            break;
                        case "seataccepted":
                            MySeatChanged(Convert.ToInt16(br.ReadString()));
                            break;
                        case "seatfree":
                            int freedSeat = Convert.ToInt16(br.ReadString());
                            SeatTaken(freedSeat, false, "");
                            if(mySeat!=0)
                            {
                                MySeatChanged(mySeat);
                            }
                            break;
                        case "ableToStart":
                            CanStartGame(true);
                            break;
                        case "unableToStart":
                            CanStartGame(false);
                            break;
                        case "chooseStarted":
                            info[0] = Convert.ToInt16(br.ReadString());
                            info[1] = Convert.ToInt16(br.ReadString());
                            info[2] = Convert.ToInt16(br.ReadString());
                            info[3] = Convert.ToInt16(br.ReadString());
                            info[4] = Convert.ToInt16(br.ReadString());
                            info[5] = Convert.ToInt16(br.ReadString());
                            info[6] = Convert.ToInt16(br.ReadString());
                            ChooseStarted();
                            break;
                        case "addChosen":
                            AddSelected(br.ReadString(), br.ReadString());
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
                if (server != null && server.Connected)
                {
                    bw.Write("stand");
                    bw.Write("disconnect");
                    server.Close();
                }
                if (cmd == "disconnect")
                {
                    MessageBox.Show("Serwer został wyłączony");
                }
                Application.Exit();
            }
        }

        delegate void AddSelectedDelegate(string name, string chosen);

        private void AddSelected(string name, string chosen)
        {
            if (LadyChoiceImg.InvokeRequired || MordredChoiceImg.InvokeRequired || MorganaChoiceImg.InvokeRequired || OberonChoiceImg.InvokeRequired || ParsifalChoiceImg.InvokeRequired)
            {
                AddSelectedDelegate f = new AddSelectedDelegate(AddSelected);
                this.Invoke(f, new object[] { name, chosen });
            }
            else
            {
                switch (name)
                {
                    case "lady":
                        if (chosen == "true")
                        {
                            LadyChoiceImg.Image = Avalon.Properties.Resources.HighlightedCard;
                        }
                        else
                        {
                            LadyChoiceImg.Image = null;
                        }
                        break;
                    case "persifal":
                        if (chosen == "true")
                        {
                            ParsifalChoiceImg.Image = Avalon.Properties.Resources.HighlightedCard;
                        }
                        else
                        {
                            ParsifalChoiceImg.Image = null;
                        }
                        break;
                    case "mordred":
                        if (chosen == "true")
                        {
                            MordredChoiceImg.Image = Avalon.Properties.Resources.HighlightedCard;
                        }
                        else
                        {
                            MordredChoiceImg.Image = null;
                        }
                        break;
                    case "morgana":
                        if (chosen == "true")
                        {
                            MorganaChoiceImg.Image = Avalon.Properties.Resources.HighlightedCard;
                        }
                        else
                        {
                            MorganaChoiceImg.Image = null;
                        }
                        break;
                    case "oberon":
                        if (chosen == "true")
                        {
                            OberonChoiceImg.Image = Avalon.Properties.Resources.HighlightedCard;
                        }
                        else
                        {
                            OberonChoiceImg.Image = null;
                        }
                        break;
                }
            }
        }

        delegate void ChooseStartedDelegate();

        private void ChooseStarted()
        {
            if (AwayButton1.InvokeRequired || AwayButton2.InvokeRequired || AwayButton3.InvokeRequired || AwayButton4.InvokeRequired || AwayButton5.InvokeRequired || AwayButton6.InvokeRequired || AwayButton7.InvokeRequired || AwayButton8.InvokeRequired || AwayButton9.InvokeRequired || AwayButton10.InvokeRequired || SitButton1.InvokeRequired || SitButton2.InvokeRequired || SitButton3.InvokeRequired || SitButton4.InvokeRequired || SitButton5.InvokeRequired || SitButton6.InvokeRequired || SitButton7.InvokeRequired || SitButton8.InvokeRequired || SitButton9.InvokeRequired || SitButton10.InvokeRequired || NumberForMission1.InvokeRequired || NumberForMission2.InvokeRequired || NumberForMission3.InvokeRequired || NumberForMission4.InvokeRequired || NumberForMission5.InvokeRequired || PlayersInfoLabel.InvokeRequired || MaxSpecialEvilLabel.InvokeRequired || LadyChoiceImg.InvokeRequired || MordredChoiceImg.InvokeRequired || MorganaChoiceImg.InvokeRequired || OberonChoiceImg.InvokeRequired || ParsifalChoiceImg.InvokeRequired)
            {
                ChooseStartedDelegate f = new ChooseStartedDelegate(ChooseStarted);
                this.Invoke(f, new object[] { });
            }
            else
            {
                NumberForMission1.Text = "Skład : " + info[0];
                NumberForMission2.Text = "Skład : " + info[1];
                NumberForMission3.Text = "Skład : " + info[2];
                NumberForMission4.Text = "Skład : " + info[3];
                NumberForMission5.Text = "Skład : " + info[4];
                PlayersInfoLabel.Text = "Graczy : " + (info[5] + info[6]) + "\nDobrzy : " + info[5] + "\nŹli : " + info[6];
                PlayersInfoLabel.Visible = true;
                MaxSpecialEvilLabel.Text = "Max : " + (info[6] - 1);
                MaxSpecialEvilLabel.Visible = true;
                LadyChoiceImg.Visible = true;
                MordredChoiceImg.Visible = true;
                MorganaChoiceImg.Visible = true;
                OberonChoiceImg.Visible = true;
                ParsifalChoiceImg.Visible = true;
                AwayButton1.Visible = false;
                AwayButton2.Visible = false;
                AwayButton3.Visible = false;
                AwayButton4.Visible = false;
                AwayButton5.Visible = false;
                AwayButton6.Visible = false;
                AwayButton7.Visible = false;
                AwayButton8.Visible = false;
                AwayButton9.Visible = false;
                AwayButton10.Visible = false;
                SitButton1.Visible = false;
                SitButton2.Visible = false;
                SitButton3.Visible = false;
                SitButton4.Visible = false;
                SitButton5.Visible = false;
                SitButton6.Visible = false;
                SitButton7.Visible = false;
                SitButton8.Visible = false;
                SitButton9.Visible = false;
                SitButton10.Visible = false;
            }
        }

        delegate void CanStartGameDelegate(bool canStart);

        private void CanStartGame(bool canStart)
        {
            if(StartGameButton.InvokeRequired)
            {
                CanStartGameDelegate f = new CanStartGameDelegate(CanStartGame);
                this.Invoke(f, new object[] { canStart });
            }
            else
            {
                if(canStart)
                {
                    StartGameButton.Visible = true;
                }
                else
                {
                    StartGameButton.Visible = false;
                }
            }
        }

        delegate void SeatChangedDelegate(int number);

        private void MySeatChanged(int number)
        {
            if (AwayButton1.InvokeRequired || AwayButton2.InvokeRequired || AwayButton3.InvokeRequired || AwayButton4.InvokeRequired || AwayButton5.InvokeRequired || AwayButton6.InvokeRequired || AwayButton7.InvokeRequired || AwayButton8.InvokeRequired || AwayButton9.InvokeRequired || AwayButton10.InvokeRequired || SitButton1.InvokeRequired || SitButton2.InvokeRequired || SitButton3.InvokeRequired || SitButton4.InvokeRequired || SitButton5.InvokeRequired || SitButton6.InvokeRequired || SitButton7.InvokeRequired || SitButton8.InvokeRequired || SitButton9.InvokeRequired || SitButton10.InvokeRequired)
            {
                SeatChangedDelegate f = new SeatChangedDelegate(MySeatChanged);
                this.Invoke(f, new object[] { number});
            }
            else
            {
                mySeat = number;
                switch (mySeat)
                {
                    case 1:
                        AwayButton1.Visible = true;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 2:
                        AwayButton2.Visible = true;
                        SitButton1.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 3:
                        AwayButton3.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 4:
                        AwayButton4.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 5:
                        AwayButton5.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 6:
                        AwayButton6.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 7:
                        AwayButton7.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 8:
                        AwayButton8.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton9.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 9:
                        AwayButton9.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton10.Visible = false;
                        break;
                    case 10:
                        AwayButton10.Visible = true;
                        SitButton1.Visible = false;
                        SitButton2.Visible = false;
                        SitButton3.Visible = false;
                        SitButton4.Visible = false;
                        SitButton5.Visible = false;
                        SitButton6.Visible = false;
                        SitButton7.Visible = false;
                        SitButton8.Visible = false;
                        SitButton9.Visible = false;
                        break;
                }
            }
        }

        private void SeatTaken(int number, bool taken, string nick)
        {
            switch(number)
            {
                case 1:
                    Seat1Taken(taken, nick);
                    break;
                case 2:
                    Seat2Taken(taken, nick);
                    break;
                case 3:
                    Seat3Taken(taken, nick);
                    break;
                case 4:
                    Seat4Taken(taken, nick);
                    break;
                case 5:
                    Seat5Taken(taken, nick);
                    break;
                case 6:
                    Seat6Taken(taken, nick);
                    break;
                case 7:
                    Seat7Taken(taken, nick);
                    break;
                case 8:
                    Seat8Taken(taken, nick);
                    break;
                case 9:
                    Seat9Taken(taken, nick);
                    break;
                case 10:
                    Seat10Taken(taken, nick);
                    break;
            }
        }

        delegate void SitTakenDelegate(bool taken, string nick);

        private void Seat1Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton1.InvokeRequired || ChoiceAgainstButton1.InvokeRequired || ChoiceAcceptButton1.InvokeRequired || CharacterPicture1.InvokeRequired || LadyCheckButton1.InvokeRequired || AddToTeamButton1.InvokeRequired || ChoicePicture1.InvokeRequired || NicknameLabel1.InvokeRequired || LadyPicture1.InvokeRequired || LeaderIcon1.InvokeRequired || AwayButton1.InvokeRequired || InTeamIcon1.InvokeRequired || SitButton1.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat1Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel1.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton1.Visible = false;
                    ChoiceAgainstButton1.Visible = false;
                    ChoiceAcceptButton1.Visible = false;
                    CharacterPicture1.Visible = true;
                    LadyCheckButton1.Visible = false;
                    AddToTeamButton1.Visible = false;
                    ChoicePicture1.Visible = true;
                    NicknameLabel1.Visible = true;
                    LadyPicture1.Visible = false;
                    LeaderIcon1.Visible = false;
                    AwayButton1.Visible = false;
                    InTeamIcon1.Visible = false;
                    SitButton1.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton1.Visible = false;
                    ChoiceAgainstButton1.Visible = false;
                    ChoiceAcceptButton1.Visible = false;
                    CharacterPicture1.Visible = false;
                    LadyCheckButton1.Visible = false;
                    AddToTeamButton1.Visible = false;
                    ChoicePicture1.Visible = false;
                    NicknameLabel1.Visible = false;
                    LadyPicture1.Visible = false;
                    LeaderIcon1.Visible = false;
                    AwayButton1.Visible = false;
                    InTeamIcon1.Visible = false;
                    SitButton1.Visible = true;
                }
            }
        }

        private void Seat2Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton2.InvokeRequired || ChoiceAgainstButton2.InvokeRequired || ChoiceAcceptButton2.InvokeRequired || CharacterPicture2.InvokeRequired || LadyCheckButton2.InvokeRequired || AddToTeamButton2.InvokeRequired || ChoicePicture2.InvokeRequired || NicknameLabel2.InvokeRequired || LadyPicture2.InvokeRequired || LeaderIcon2.InvokeRequired || AwayButton2.InvokeRequired || InTeamIcon2.InvokeRequired || SitButton2.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat2Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel2.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton2.Visible = false;
                    ChoiceAgainstButton2.Visible = false;
                    ChoiceAcceptButton2.Visible = false;
                    CharacterPicture2.Visible = true;
                    LadyCheckButton2.Visible = false;
                    AddToTeamButton2.Visible = false;
                    ChoicePicture2.Visible = true;
                    NicknameLabel2.Visible = true;
                    LadyPicture2.Visible = false;
                    LeaderIcon2.Visible = false;
                    AwayButton2.Visible = false;
                    InTeamIcon2.Visible = false;
                    SitButton2.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton2.Visible = false;
                    ChoiceAgainstButton2.Visible = false;
                    ChoiceAcceptButton2.Visible = false;
                    CharacterPicture2.Visible = false;
                    LadyCheckButton2.Visible = false;
                    AddToTeamButton2.Visible = false;
                    ChoicePicture2.Visible = false;
                    NicknameLabel2.Visible = false;
                    LadyPicture2.Visible = false;
                    LeaderIcon2.Visible = false;
                    AwayButton2.Visible = false;
                    InTeamIcon2.Visible = false;
                    SitButton2.Visible = true;
                }
            }
        }

        private void Seat3Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton3.InvokeRequired || ChoiceAgainstButton3.InvokeRequired || ChoiceAcceptButton3.InvokeRequired || CharacterPicture3.InvokeRequired || LadyCheckButton3.InvokeRequired || AddToTeamButton3.InvokeRequired || ChoicePicture3.InvokeRequired || NicknameLabel3.InvokeRequired || LadyPicture3.InvokeRequired || LeaderIcon3.InvokeRequired || AwayButton3.InvokeRequired || InTeamIcon3.InvokeRequired || SitButton3.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat3Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel3.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton3.Visible = false;
                    ChoiceAgainstButton3.Visible = false;
                    ChoiceAcceptButton3.Visible = false;
                    CharacterPicture3.Visible = true;
                    LadyCheckButton3.Visible = false;
                    AddToTeamButton3.Visible = false;
                    ChoicePicture3.Visible = true;
                    NicknameLabel3.Visible = true;
                    LadyPicture3.Visible = false;
                    LeaderIcon3.Visible = false;
                    AwayButton3.Visible = false;
                    InTeamIcon3.Visible = false;
                    SitButton3.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton3.Visible = false;
                    ChoiceAgainstButton3.Visible = false;
                    ChoiceAcceptButton3.Visible = false;
                    CharacterPicture3.Visible = false;
                    LadyCheckButton3.Visible = false;
                    AddToTeamButton3.Visible = false;
                    ChoicePicture3.Visible = false;
                    NicknameLabel3.Visible = false;
                    LadyPicture3.Visible = false;
                    LeaderIcon3.Visible = false;
                    AwayButton3.Visible = false;
                    InTeamIcon3.Visible = false;
                    SitButton3.Visible = true;
                }
            }
        }

        private void Seat4Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton4.InvokeRequired || ChoiceAgainstButton4.InvokeRequired || ChoiceAcceptButton4.InvokeRequired || CharacterPicture4.InvokeRequired || LadyCheckButton4.InvokeRequired || AddToTeamButton4.InvokeRequired || ChoicePicture4.InvokeRequired || NicknameLabel4.InvokeRequired || LadyPicture4.InvokeRequired || LeaderIcon4.InvokeRequired || AwayButton4.InvokeRequired || InTeamIcon4.InvokeRequired || SitButton4.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat4Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel4.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton4.Visible = false;
                    ChoiceAgainstButton4.Visible = false;
                    ChoiceAcceptButton4.Visible = false;
                    CharacterPicture4.Visible = true;
                    LadyCheckButton4.Visible = false;
                    AddToTeamButton4.Visible = false;
                    ChoicePicture4.Visible = true;
                    NicknameLabel4.Visible = true;
                    LadyPicture4.Visible = false;
                    LeaderIcon4.Visible = false;
                    AwayButton4.Visible = false;
                    InTeamIcon4.Visible = false;
                    SitButton4.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton4.Visible = false;
                    ChoiceAgainstButton4.Visible = false;
                    ChoiceAcceptButton4.Visible = false;
                    CharacterPicture4.Visible = false;
                    LadyCheckButton4.Visible = false;
                    AddToTeamButton4.Visible = false;
                    ChoicePicture4.Visible = false;
                    NicknameLabel4.Visible = false;
                    LadyPicture4.Visible = false;
                    LeaderIcon4.Visible = false;
                    AwayButton4.Visible = false;
                    InTeamIcon4.Visible = false;
                    SitButton4.Visible = true;
                }
            }
        }

        private void Seat5Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton5.InvokeRequired || ChoiceAgainstButton5.InvokeRequired || ChoiceAcceptButton5.InvokeRequired || CharacterPicture5.InvokeRequired || LadyCheckButton5.InvokeRequired || AddToTeamButton5.InvokeRequired || ChoicePicture5.InvokeRequired || NicknameLabel5.InvokeRequired || LadyPicture5.InvokeRequired || LeaderIcon5.InvokeRequired || AwayButton5.InvokeRequired || InTeamIcon5.InvokeRequired || SitButton5.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat5Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel5.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton5.Visible = false;
                    ChoiceAgainstButton5.Visible = false;
                    ChoiceAcceptButton5.Visible = false;
                    CharacterPicture5.Visible = true;
                    LadyCheckButton5.Visible = false;
                    AddToTeamButton5.Visible = false;
                    ChoicePicture5.Visible = true;
                    NicknameLabel5.Visible = true;
                    LadyPicture5.Visible = false;
                    LeaderIcon5.Visible = false;
                    AwayButton5.Visible = false;
                    InTeamIcon5.Visible = false;
                    SitButton5.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton5.Visible = false;
                    ChoiceAgainstButton5.Visible = false;
                    ChoiceAcceptButton5.Visible = false;
                    CharacterPicture5.Visible = false;
                    LadyCheckButton5.Visible = false;
                    AddToTeamButton5.Visible = false;
                    ChoicePicture5.Visible = false;
                    NicknameLabel5.Visible = false;
                    LadyPicture5.Visible = false;
                    LeaderIcon5.Visible = false;
                    AwayButton5.Visible = false;
                    InTeamIcon5.Visible = false;
                    SitButton5.Visible = true;
                }
            }
        }

        private void Seat6Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton6.InvokeRequired || ChoiceAgainstButton6.InvokeRequired || ChoiceAcceptButton6.InvokeRequired || CharacterPicture6.InvokeRequired || LadyCheckButton6.InvokeRequired || AddToTeamButton6.InvokeRequired || ChoicePicture6.InvokeRequired || NicknameLabel6.InvokeRequired || LadyPicture6.InvokeRequired || LeaderIcon6.InvokeRequired || AwayButton6.InvokeRequired || InTeamIcon6.InvokeRequired || SitButton6.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat6Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel6.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton6.Visible = false;
                    ChoiceAgainstButton6.Visible = false;
                    ChoiceAcceptButton6.Visible = false;
                    CharacterPicture6.Visible = true;
                    LadyCheckButton6.Visible = false;
                    AddToTeamButton6.Visible = false;
                    ChoicePicture6.Visible = true;
                    NicknameLabel6.Visible = true;
                    LadyPicture6.Visible = false;
                    LeaderIcon6.Visible = false;
                    AwayButton6.Visible = false;
                    InTeamIcon6.Visible = false;
                    SitButton6.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton6.Visible = false;
                    ChoiceAgainstButton6.Visible = false;
                    ChoiceAcceptButton6.Visible = false;
                    CharacterPicture6.Visible = false;
                    LadyCheckButton6.Visible = false;
                    AddToTeamButton6.Visible = false;
                    ChoicePicture6.Visible = false;
                    NicknameLabel6.Visible = false;
                    LadyPicture6.Visible = false;
                    LeaderIcon6.Visible = false;
                    AwayButton6.Visible = false;
                    InTeamIcon6.Visible = false;
                    SitButton6.Visible = true;
                }
            }
        }

        private void Seat7Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton7.InvokeRequired || ChoiceAgainstButton7.InvokeRequired || ChoiceAcceptButton7.InvokeRequired || CharacterPicture7.InvokeRequired || LadyCheckButton7.InvokeRequired || AddToTeamButton7.InvokeRequired || ChoicePicture7.InvokeRequired || NicknameLabel7.InvokeRequired || LadyPicture7.InvokeRequired || LeaderIcon7.InvokeRequired || AwayButton7.InvokeRequired || InTeamIcon7.InvokeRequired || SitButton7.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat7Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel7.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton7.Visible = false;
                    ChoiceAgainstButton7.Visible = false;
                    ChoiceAcceptButton7.Visible = false;
                    CharacterPicture7.Visible = true;
                    LadyCheckButton7.Visible = false;
                    AddToTeamButton7.Visible = false;
                    ChoicePicture7.Visible = true;
                    NicknameLabel7.Visible = true;
                    LadyPicture7.Visible = false;
                    LeaderIcon7.Visible = false;
                    AwayButton7.Visible = false;
                    InTeamIcon7.Visible = false;
                    SitButton7.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton7.Visible = false;
                    ChoiceAgainstButton7.Visible = false;
                    ChoiceAcceptButton7.Visible = false;
                    CharacterPicture7.Visible = false;
                    LadyCheckButton7.Visible = false;
                    AddToTeamButton7.Visible = false;
                    ChoicePicture7.Visible = false;
                    NicknameLabel7.Visible = false;
                    LadyPicture7.Visible = false;
                    LeaderIcon7.Visible = false;
                    AwayButton7.Visible = false;
                    InTeamIcon7.Visible = false;
                    SitButton7.Visible = true;
                }
            }
        }

        private void Seat8Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton8.InvokeRequired || ChoiceAgainstButton8.InvokeRequired || ChoiceAcceptButton8.InvokeRequired || CharacterPicture8.InvokeRequired || LadyCheckButton8.InvokeRequired || AddToTeamButton8.InvokeRequired || ChoicePicture8.InvokeRequired || NicknameLabel8.InvokeRequired || LadyPicture8.InvokeRequired || LeaderIcon8.InvokeRequired || AwayButton8.InvokeRequired || InTeamIcon8.InvokeRequired || SitButton8.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat8Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel8.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton8.Visible = false;
                    ChoiceAgainstButton8.Visible = false;
                    ChoiceAcceptButton8.Visible = false;
                    CharacterPicture8.Visible = true;
                    LadyCheckButton8.Visible = false;
                    AddToTeamButton8.Visible = false;
                    ChoicePicture8.Visible = true;
                    NicknameLabel8.Visible = true;
                    LadyPicture8.Visible = false;
                    LeaderIcon8.Visible = false;
                    AwayButton8.Visible = false;
                    InTeamIcon8.Visible = false;
                    SitButton8.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton8.Visible = false;
                    ChoiceAgainstButton8.Visible = false;
                    ChoiceAcceptButton8.Visible = false;
                    CharacterPicture8.Visible = false;
                    LadyCheckButton8.Visible = false;
                    AddToTeamButton8.Visible = false;
                    ChoicePicture8.Visible = false;
                    NicknameLabel8.Visible = false;
                    LadyPicture8.Visible = false;
                    LeaderIcon8.Visible = false;
                    AwayButton8.Visible = false;
                    InTeamIcon8.Visible = false;
                    SitButton8.Visible = true;
                }
            }
        }

        private void Seat9Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton9.InvokeRequired || ChoiceAgainstButton9.InvokeRequired || ChoiceAcceptButton9.InvokeRequired || CharacterPicture9.InvokeRequired || LadyCheckButton9.InvokeRequired || AddToTeamButton9.InvokeRequired || ChoicePicture9.InvokeRequired || NicknameLabel9.InvokeRequired || LadyPicture9.InvokeRequired || LeaderIcon9.InvokeRequired || AwayButton9.InvokeRequired || InTeamIcon9.InvokeRequired || SitButton9.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat9Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel9.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton9.Visible = false;
                    ChoiceAgainstButton9.Visible = false;
                    ChoiceAcceptButton9.Visible = false;
                    CharacterPicture9.Visible = true;
                    LadyCheckButton9.Visible = false;
                    AddToTeamButton9.Visible = false;
                    ChoicePicture9.Visible = true;
                    NicknameLabel9.Visible = true;
                    LadyPicture9.Visible = false;
                    LeaderIcon9.Visible = false;
                    AwayButton9.Visible = false;
                    InTeamIcon9.Visible = false;
                    SitButton9.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton9.Visible = false;
                    ChoiceAgainstButton9.Visible = false;
                    ChoiceAcceptButton9.Visible = false;
                    CharacterPicture9.Visible = false;
                    LadyCheckButton9.Visible = false;
                    AddToTeamButton9.Visible = false;
                    ChoicePicture9.Visible = false;
                    NicknameLabel9.Visible = false;
                    LadyPicture9.Visible = false;
                    LeaderIcon9.Visible = false;
                    AwayButton9.Visible = false;
                    InTeamIcon9.Visible = false;
                    SitButton9.Visible = true;
                }
            }
        }

        private void Seat10Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton10.InvokeRequired || ChoiceAgainstButton10.InvokeRequired || ChoiceAcceptButton10.InvokeRequired || CharacterPicture10.InvokeRequired || LadyCheckButton10.InvokeRequired || AddToTeamButton10.InvokeRequired || ChoicePicture10.InvokeRequired || NicknameLabel10.InvokeRequired || LadyPicture10.InvokeRequired || LeaderIcon10.InvokeRequired || AwayButton10.InvokeRequired || InTeamIcon10.InvokeRequired || SitButton10.InvokeRequired)
            {
                SitTakenDelegate f = new SitTakenDelegate(Seat10Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel10.Text = nick;
                if (taken)
                {
                    RemoveFromTeamButton10.Visible = false;
                    ChoiceAgainstButton10.Visible = false;
                    ChoiceAcceptButton10.Visible = false;
                    CharacterPicture10.Visible = true;
                    LadyCheckButton10.Visible = false;
                    AddToTeamButton10.Visible = false;
                    ChoicePicture10.Visible = true;
                    NicknameLabel10.Visible = true;
                    LadyPicture10.Visible = false;
                    LeaderIcon10.Visible = false;
                    AwayButton10.Visible = false;
                    InTeamIcon10.Visible = false;
                    SitButton10.Visible = false;
                }
                else
                {
                    RemoveFromTeamButton10.Visible = false;
                    ChoiceAgainstButton10.Visible = false;
                    ChoiceAcceptButton10.Visible = false;
                    CharacterPicture10.Visible = false;
                    LadyCheckButton10.Visible = false;
                    AddToTeamButton10.Visible = false;
                    ChoicePicture10.Visible = false;
                    NicknameLabel10.Visible = false;
                    LadyPicture10.Visible = false;
                    LeaderIcon10.Visible = false;
                    AwayButton10.Visible = false;
                    InTeamIcon10.Visible = false;
                    SitButton10.Visible = true;
                }
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (server!=null && server.Connected)
            {
                bw.Write("stand");
                bw.Write("disconnect");
                server.Close();
            }
            Application.Exit();
        }

        private void ExitButton_MouseEnter(object sender, EventArgs e)
        {
            ExitButton.BackgroundImage = Avalon.Properties.Resources.xHighlight;
        }

        private void ExitButton_MouseLeave(object sender, EventArgs e)
        {
            ExitButton.BackgroundImage = null;
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void MinimizeButton_MouseEnter(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = Avalon.Properties.Resources.minimizeHighlight;
        }

        private void MinimizeButton_MouseLeave(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = null;
        }

        private void SitButton1_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("1");
        }

        private void SitButton2_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("2");
        }

        private void SitButton3_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("3");
        }

        private void SitButton4_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("4");
        }

        private void SitButton5_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("5");
        }

        private void SitButton6_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("6");
        }

        private void SitButton7_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("7");
        }

        private void SitButton8_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("8");
        }

        private void SitButton9_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("9");
        }

        private void SitButton10_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("10");
        }

        private void AwayButtons(object sender, EventArgs e)
        {
            bw.Write("stand");
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            switch(stage)
            {
                case 0:
                    bw.Write("startChoose");
                    StartGameButton.Text = "Next";
                    break;
                case 1:
                    bw.Write("startGame");
                    StartGameButton.Visible = false;
                    break;
                case 2:

                    break;
            }
            stage++;
        }

        private void MordredChoiceImg_Click(object sender, EventArgs e)
        {
            bw.Write("mordredChosen");
        }

        private void MorganaChoiceImg_Click(object sender, EventArgs e)
        {
            bw.Write("morganaChosen");
        }

        private void OberonChoiceImg_Click(object sender, EventArgs e)
        {
            bw.Write("oberonChosen");
        }

        private void LadyChoiceImg_Click(object sender, EventArgs e)
        {
            bw.Write("ladyChosen");
        }

        private void ParsifalChoiceImg_Click(object sender, EventArgs e)
        {
            bw.Write("persifalChosen");
        }
    }
}
