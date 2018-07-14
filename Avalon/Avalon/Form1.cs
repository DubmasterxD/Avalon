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
        bool[] seatsTaken;
        int round = 0;
        int inTeam;
        int tag; //0-spectator, 1-good, 2-evil
        bool assassin;
        bool lady;
        bool canAssassin;

        public Game()
        {
            InitializeComponent();
            mySeat = -1;
            stage = 0;
            info = new int[7];
            seatsTaken = new bool[10];
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
            tag = 0;
            bw.Write("nick");
            bw.Write(NickTextBox.Text);
            br = new BinaryReader(ns);
            string tmp = "";
            string cmd = "";
            mySeat = -1;
            try
            {
                while ((cmd = br.ReadString()) != "disconnect")
                {
                    switch (cmd)
                    {
                        case "seatstaken":
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
                            while ((tmp = br.ReadString()) != "end")
                            {
                                SeatTaken(Convert.ToInt16(tmp), true, br.ReadString());
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
                            if (mySeat != -1)
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
                            canAssassin = false;
                            stage = 1;
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
                        case "gameStarted":
                            stage = 2;
                            inTeam = 0;
                            round = 1;
                            ChangeLeader(Convert.ToInt16(br.ReadString()));
                            if (br.ReadString() == "true")
                            {
                                ChangeLady(Convert.ToInt16(br.ReadString()));
                            }
                            ChooseSeatToSetRole(br.ReadString(), mySeat);
                            while ((tmp = br.ReadString()) != "end")
                            {
                                HighlightCharacter(Convert.ToInt16(tmp));
                            }
                            ChooseStageToGameStage();
                            MissionHighlight(1);
                            break;
                        case "added":
                            AddToTeam(Convert.ToInt16(br.ReadString()));
                            break;
                        case "fullTeam":
                            FullTeam();
                            break;
                        case "removed":
                            RemoveFromTeam(Convert.ToInt16(br.ReadString()));
                            break;
                        case "canAdd":
                            List<int> cantAdd = new List<int>();
                            while ((tmp = br.ReadString()) != "end")
                            {
                                cantAdd.Add(Convert.ToInt16(tmp));
                            }
                            CanAddToTeam(cantAdd);
                            break;
                        case "voteTeam":
                            StartVote();
                            break;
                        case "voteTeamEnded":
                            for (int i = 0; i < 10; i++)
                            {
                                if (seatsTaken[i])
                                {
                                    ShowVote(i, Convert.ToBoolean(br.ReadString()));
                                }
                            }
                            break;
                        case "accepted":
                            TeamAccepted();
                            break;
                        case "rejected":
                            TeamRejected();
                            break;
                        case "changeLeader":
                            ChangeLeader(Convert.ToInt16(br.ReadString()));
                            break;
                        case "evilWins":
                            GameEnd(false);
                            while ((tmp = br.ReadString()) != "end")
                            {
                                ChooseSeatToSetRole(br.ReadString(), Convert.ToInt16(tmp));
                            }
                            break;
                        case "goodWins":
                            GameEnd(true);
                            while ((tmp = br.ReadString()) != "end")
                            {
                                ChooseSeatToSetRole(br.ReadString(), Convert.ToInt16(tmp));
                            }
                            break;
                        case "missionSuccess":
                            int succ = Convert.ToInt16(br.ReadString());
                            MissionEnd(succ, true);
                            while ((tmp = br.ReadString()) != "end")
                            {
                                AddNick(tmp);
                            }
                            break;
                        case "missionFailed":
                            int success = Convert.ToInt16(br.ReadString());
                            MissionEnd(success, false);
                            while ((tmp = br.ReadString()) != "end")
                            {
                                AddNick(tmp);
                            }
                            break;
                        case "leaver":
                            MessageBox.Show(br.ReadString() + " wyszedł z gry");
                            Restart();
                            break;
                        case "gamerunning":
                            if(br.ReadString()==true.ToString())
                            {
                                CantSit();
                                stage = 3;
                            }
                            break;
                        case "restarted":
                            Restart();
                            break;
                        case "kill":
                            if(assassin)
                            {
                                Kill();
                            }
                            break;
                        case "ladyTime":
                            if(lady)
                            {
                                LadyCheck();
                            }
                            break;
                        case "ladyResult":
                            if(br.ReadString()=="good")
                            {
                                LadyResult(Convert.ToInt16(br.ReadString()), true);
                            }
                            else
                            {
                                LadyResult(Convert.ToInt16(br.ReadString()), false);
                            }
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

        delegate void LadyResultDelegate(int seat, bool isGood);

        private void LadyResult(int seat, bool isGood)
        {
            if(LadyPicture1.InvokeRequired || LadyPicture2.InvokeRequired || LadyPicture3.InvokeRequired || LadyPicture4.InvokeRequired || LadyPicture5.InvokeRequired || LadyPicture6.InvokeRequired || LadyPicture7.InvokeRequired || LadyPicture8.InvokeRequired || LadyPicture9.InvokeRequired || LadyPicture10.InvokeRequired)
            {
                LadyResultDelegate f = new LadyResultDelegate(LadyResult);
                this.Invoke(f, new object[] { seat, isGood });
            }
            else
            {
                if (lady)
                {
                    switch (seat)
                    {
                        case 0:
                            if (isGood)
                            {
                                LadyPicture1.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture1.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 1:
                            if (isGood)
                            {
                                LadyPicture2.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture2.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 2:
                            if (isGood)
                            {
                                LadyPicture3.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture3.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 3:
                            if (isGood)
                            {
                                LadyPicture4.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture4.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 4:
                            if (isGood)
                            {
                                LadyPicture5.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture5.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 5:
                            if (isGood)
                            {
                                LadyPicture6.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture6.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 6:
                            if (isGood)
                            {
                                LadyPicture7.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture7.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 7:
                            if (isGood)
                            {
                                LadyPicture8.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture8.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 8:
                            if (isGood)
                            {
                                LadyPicture9.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture9.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                        case 9:
                            if (isGood)
                            {
                                LadyPicture10.Image = Avalon.Properties.Resources.LadyGood;
                            }
                            else
                            {
                                LadyPicture10.Image = Avalon.Properties.Resources.LadyBad;
                            }
                            break;
                    }
                }
                ChangeLady(seat);
            }
        }

        delegate void LadyCheckDelegate();

        private void LadyCheck()
        {
            if (LadyCheckButton1.InvokeRequired || LadyCheckButton2.InvokeRequired || LadyCheckButton3.InvokeRequired || LadyCheckButton4.InvokeRequired || LadyCheckButton5.InvokeRequired || LadyCheckButton6.InvokeRequired || LadyCheckButton7.InvokeRequired || LadyCheckButton8.InvokeRequired || LadyCheckButton9.InvokeRequired || LadyCheckButton10.InvokeRequired)
            {
                LadyCheckDelegate f = new LadyCheckDelegate(LadyCheck);
                this.Invoke(f, new object[] { });
            }
            else
            {
                if (seatsTaken[0])
                {
                    LadyCheckButton1.Visible = true;
                }
                if (seatsTaken[1])
                {
                    LadyCheckButton2.Visible = true;
                }
                if (seatsTaken[2])
                {
                    LadyCheckButton3.Visible = true;
                }
                if (seatsTaken[3])
                {
                    LadyCheckButton4.Visible = true;
                }
                if (seatsTaken[4])
                {
                    LadyCheckButton5.Visible = true;
                }
                if (seatsTaken[5])
                {
                    LadyCheckButton6.Visible = true;
                }
                if (seatsTaken[6])
                {
                    LadyCheckButton7.Visible = true;
                }
                if (seatsTaken[7])
                {
                    LadyCheckButton8.Visible = true;
                }
                if (seatsTaken[8])
                {
                    LadyCheckButton9.Visible = true;
                }
                if (seatsTaken[9])
                {
                    LadyCheckButton10.Visible = true;
                }
                switch (mySeat)
                {
                    case 0:
                        LadyCheckButton1.Visible = false;
                        break;
                    case 1:
                        LadyCheckButton2.Visible = false;
                        break;
                    case 2:
                        LadyCheckButton3.Visible = false;
                        break;
                    case 3:
                        LadyCheckButton4.Visible = false;
                        break;
                    case 4:
                        LadyCheckButton5.Visible = false;
                        break;
                    case 5:
                        LadyCheckButton6.Visible = false;
                        break;
                    case 6:
                        LadyCheckButton7.Visible = false;
                        break;
                    case 7:
                        LadyCheckButton8.Visible = false;
                        break;
                    case 8:
                        LadyCheckButton9.Visible = false;
                        break;
                    case 9:
                        LadyCheckButton10.Visible = false;
                        break;
                }
            }
        }

        delegate void KillDelegate();

        private void Kill()
        {
            if (LadyPicture1.InvokeRequired || LadyPicture2.InvokeRequired || LadyPicture3.InvokeRequired || LadyPicture4.InvokeRequired || LadyPicture5.InvokeRequired || LadyPicture6.InvokeRequired || LadyPicture7.InvokeRequired || LadyPicture8.InvokeRequired || LadyPicture9.InvokeRequired || LadyPicture10.InvokeRequired || LadyCheckButton1.InvokeRequired || LadyCheckButton2.InvokeRequired || LadyCheckButton3.InvokeRequired || LadyCheckButton4.InvokeRequired || LadyCheckButton5.InvokeRequired || LadyCheckButton6.InvokeRequired || LadyCheckButton7.InvokeRequired || LadyCheckButton8.InvokeRequired || LadyCheckButton9.InvokeRequired || LadyCheckButton10.InvokeRequired)
            {
                KillDelegate f = new KillDelegate(Kill);
                this.Invoke(f, new object[] { });
            }
            else
            {
                canAssassin = true;
                LadyPicture1.Visible = false;
                LadyPicture2.Visible = false;
                LadyPicture3.Visible = false;
                LadyPicture4.Visible = false;
                LadyPicture5.Visible = false;
                LadyPicture6.Visible = false;
                LadyPicture7.Visible = false;
                LadyPicture8.Visible = false;
                LadyPicture9.Visible = false;
                LadyPicture10.Visible = false;
                for (int i = 0; i < 10; i++)
                {
                    if (seatsTaken[i])
                    {
                        switch (i)
                        {
                            case 0:
                                if (mySeat != i && CharacterPicture1.Image == null)
                                {
                                    LadyCheckButton1.Visible = true;
                                }
                                break;
                            case 1:
                                if (mySeat != i && CharacterPicture2.Image == null)
                                {
                                    LadyCheckButton2.Visible = true;
                                }
                                break;
                            case 2:
                                if (mySeat != i && CharacterPicture3.Image == null)
                                {
                                    LadyCheckButton3.Visible = true;
                                }
                                break;
                            case 3:
                                if (mySeat != i && CharacterPicture4.Image == null)
                                {
                                    LadyCheckButton4.Visible = true;
                                }
                                break;
                            case 4:
                                if (mySeat != i && CharacterPicture5.Image == null)
                                {
                                    LadyCheckButton5.Visible = true;
                                }
                                break;
                            case 5:
                                if (mySeat != i && CharacterPicture6.Image == null)
                                {
                                    LadyCheckButton6.Visible = true;
                                }
                                break;
                            case 6:
                                if (mySeat != i && CharacterPicture7.Image == null)
                                {
                                    LadyCheckButton7.Visible = true;
                                }
                                break;
                            case 7:
                                if (mySeat != i && CharacterPicture8.Image == null)
                                {
                                    LadyCheckButton8.Visible = true;
                                }
                                break;
                            case 8:
                                if (mySeat != i && CharacterPicture9.Image == null)
                                {
                                    LadyCheckButton9.Visible = true;
                                }
                                break;
                            case 9:
                                if (mySeat != i && CharacterPicture10.Image == null)
                                {
                                    LadyCheckButton10.Visible = true;
                                }
                                break;
                        }
                    }
                }
            }
        }

        delegate void RestartDelegate();

        private void Restart()
        {
            if (GameResultImage.InvokeRequired || CharacterPicture1.InvokeRequired || CharacterPicture2.InvokeRequired || CharacterPicture3.InvokeRequired || CharacterPicture4.InvokeRequired || CharacterPicture5.InvokeRequired || CharacterPicture6.InvokeRequired || CharacterPicture7.InvokeRequired || CharacterPicture8.InvokeRequired || CharacterPicture9.InvokeRequired || CharacterPicture10.InvokeRequired || LeaderIcon1.InvokeRequired || LeaderIcon2.InvokeRequired || LeaderIcon3.InvokeRequired || LeaderIcon4.InvokeRequired || LeaderIcon5.InvokeRequired || LeaderIcon6.InvokeRequired || LeaderIcon7.InvokeRequired || LeaderIcon8.InvokeRequired || LeaderIcon9.InvokeRequired || LeaderIcon10.InvokeRequired || InTeamIcon1.InvokeRequired || InTeamIcon2.InvokeRequired || InTeamIcon3.InvokeRequired || InTeamIcon4.InvokeRequired || InTeamIcon5.InvokeRequired || InTeamIcon6.InvokeRequired || InTeamIcon7.InvokeRequired || InTeamIcon8.InvokeRequired || InTeamIcon9.InvokeRequired || InTeamIcon10.InvokeRequired || AddToTeamButton1.InvokeRequired || AddToTeamButton2.InvokeRequired || AddToTeamButton3.InvokeRequired || AddToTeamButton4.InvokeRequired || AddToTeamButton5.InvokeRequired || AddToTeamButton6.InvokeRequired || AddToTeamButton7.InvokeRequired || AddToTeamButton8.InvokeRequired || AddToTeamButton9.InvokeRequired || AddToTeamButton10.InvokeRequired || RemoveFromTeamButton1.InvokeRequired || RemoveFromTeamButton2.InvokeRequired || RemoveFromTeamButton3.InvokeRequired || RemoveFromTeamButton4.InvokeRequired || RemoveFromTeamButton5.InvokeRequired || RemoveFromTeamButton6.InvokeRequired || RemoveFromTeamButton7.InvokeRequired || RemoveFromTeamButton8.InvokeRequired || RemoveFromTeamButton9.InvokeRequired || RemoveFromTeamButton10.InvokeRequired || LadyPicture1.InvokeRequired || LadyPicture2.InvokeRequired || LadyPicture3.InvokeRequired || LadyPicture4.InvokeRequired || LadyPicture5.InvokeRequired || LadyPicture6.InvokeRequired || LadyPicture7.InvokeRequired || LadyPicture8.InvokeRequired || LadyPicture9.InvokeRequired || LadyPicture10.InvokeRequired || LadyCheckButton1.InvokeRequired || LadyCheckButton2.InvokeRequired || LadyCheckButton3.InvokeRequired || LadyCheckButton4.InvokeRequired || LadyCheckButton5.InvokeRequired || LadyCheckButton6.InvokeRequired || LadyCheckButton7.InvokeRequired || LadyCheckButton8.InvokeRequired || LadyCheckButton9.InvokeRequired || LadyCheckButton10.InvokeRequired || ChoicePicture1.InvokeRequired || ChoicePicture2.InvokeRequired || ChoicePicture3.InvokeRequired || ChoicePicture4.InvokeRequired || ChoicePicture5.InvokeRequired || ChoicePicture6.InvokeRequired || ChoicePicture7.InvokeRequired || ChoicePicture8.InvokeRequired || ChoicePicture9.InvokeRequired || ChoicePicture10.InvokeRequired || ChoiceAcceptButton1.InvokeRequired || ChoiceAcceptButton2.InvokeRequired || ChoiceAcceptButton3.InvokeRequired || ChoiceAcceptButton4.InvokeRequired || ChoiceAcceptButton5.InvokeRequired || ChoiceAcceptButton6.InvokeRequired || ChoiceAcceptButton7.InvokeRequired || ChoiceAcceptButton8.InvokeRequired || ChoiceAcceptButton9.InvokeRequired || ChoiceAcceptButton10.InvokeRequired || ChoiceAgainstButton1.InvokeRequired || ChoiceAgainstButton2.InvokeRequired || ChoiceAgainstButton3.InvokeRequired || ChoiceAgainstButton4.InvokeRequired || ChoiceAgainstButton5.InvokeRequired || ChoiceAgainstButton6.InvokeRequired || ChoiceAgainstButton7.InvokeRequired || ChoiceAgainstButton8.InvokeRequired || ChoiceAgainstButton9.InvokeRequired || ChoiceAgainstButton10.InvokeRequired || TeamMembersLeftLabel.InvokeRequired || PlayersInfoLabel.InvokeRequired || MaxSpecialEvilLabel.InvokeRequired || FailedVotes1.InvokeRequired || FailedVotes2.InvokeRequired || FailedVotes3.InvokeRequired || FailedVotes4.InvokeRequired || FailedVotes5.InvokeRequired || NumberForMission1.InvokeRequired || NumberForMission2.InvokeRequired || NumberForMission3.InvokeRequired || NumberForMission4.InvokeRequired || NumberForMission5.InvokeRequired || MissionResultPic1.InvokeRequired || MissionResultPic2.InvokeRequired || MissionResultPic3.InvokeRequired || MissionResultPic4.InvokeRequired || MissionResultPic5.InvokeRequired || Mission1Table.InvokeRequired || Mission2Table.InvokeRequired || Mission3Table.InvokeRequired || Mission4Table.InvokeRequired || Mission5Table.InvokeRequired)
            {
                RestartDelegate f = new RestartDelegate(Restart);
                this.Invoke(f, new object[] { });
            }
            else
            {
                stage = 0;
                round = 0;
                inTeam = 0;
                CharacterPicture1.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture2.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture3.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture4.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture5.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture6.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture7.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture8.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture9.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture10.BackgroundImage = Avalon.Properties.Resources.Postacbckg;
                CharacterPicture1.Image = null;
                CharacterPicture2.Image = null;
                CharacterPicture3.Image = null;
                CharacterPicture4.Image = null;
                CharacterPicture5.Image = null;
                CharacterPicture6.Image = null;
                CharacterPicture7.Image = null;
                CharacterPicture8.Image = null;
                CharacterPicture9.Image = null;
                CharacterPicture10.Image = null;
                LeaderIcon1.Visible = false;
                LeaderIcon2.Visible = false;
                LeaderIcon3.Visible = false;
                LeaderIcon4.Visible = false;
                LeaderIcon5.Visible = false;
                LeaderIcon6.Visible = false;
                LeaderIcon7.Visible = false;
                LeaderIcon8.Visible = false;
                LeaderIcon9.Visible = false;
                LeaderIcon10.Visible = false;
                InTeamIcon1.Visible = false;
                InTeamIcon2.Visible = false;
                InTeamIcon3.Visible = false;
                InTeamIcon4.Visible = false;
                InTeamIcon5.Visible = false;
                InTeamIcon6.Visible = false;
                InTeamIcon7.Visible = false;
                InTeamIcon8.Visible = false;
                InTeamIcon9.Visible = false;
                InTeamIcon10.Visible = false;
                AddToTeamButton1.Visible = false;
                AddToTeamButton2.Visible = false;
                AddToTeamButton3.Visible = false;
                AddToTeamButton4.Visible = false;
                AddToTeamButton5.Visible = false;
                AddToTeamButton6.Visible = false;
                AddToTeamButton7.Visible = false;
                AddToTeamButton8.Visible = false;
                AddToTeamButton9.Visible = false;
                AddToTeamButton10.Visible = false;
                RemoveFromTeamButton1.Visible = false;
                RemoveFromTeamButton2.Visible = false;
                RemoveFromTeamButton3.Visible = false;
                RemoveFromTeamButton4.Visible = false;
                RemoveFromTeamButton5.Visible = false;
                RemoveFromTeamButton6.Visible = false;
                RemoveFromTeamButton7.Visible = false;
                RemoveFromTeamButton8.Visible = false;
                RemoveFromTeamButton9.Visible = false;
                RemoveFromTeamButton10.Visible = false;
                LadyPicture1.Visible = true;
                LadyPicture2.Visible = true;
                LadyPicture3.Visible = true;
                LadyPicture4.Visible = true;
                LadyPicture5.Visible = true;
                LadyPicture6.Visible = true;
                LadyPicture7.Visible = true;
                LadyPicture8.Visible = true;
                LadyPicture9.Visible = true;
                LadyPicture10.Visible = true;
                LadyPicture1.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture2.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture3.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture4.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture5.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture6.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture7.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture8.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture9.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture10.BackgroundImage = Avalon.Properties.Resources.PaniJeziora;
                LadyPicture1.Image = null;
                LadyPicture2.Image = null;
                LadyPicture3.Image = null;
                LadyPicture4.Image = null;
                LadyPicture5.Image = null;
                LadyPicture6.Image = null;
                LadyPicture7.Image = null;
                LadyPicture8.Image = null;
                LadyPicture9.Image = null;
                LadyPicture10.Image = null;
                LadyPicture1.Visible = false;
                LadyPicture2.Visible = false;
                LadyPicture3.Visible = false;
                LadyPicture4.Visible = false;
                LadyPicture5.Visible = false;
                LadyPicture6.Visible = false;
                LadyPicture7.Visible = false;
                LadyPicture8.Visible = false;
                LadyPicture9.Visible = false;
                LadyPicture10.Visible = false;
                LadyCheckButton1.Visible = false;
                LadyCheckButton2.Visible = false;
                LadyCheckButton3.Visible = false;
                LadyCheckButton4.Visible = false;
                LadyCheckButton5.Visible = false;
                LadyCheckButton6.Visible = false;
                LadyCheckButton7.Visible = false;
                LadyCheckButton8.Visible = false;
                LadyCheckButton9.Visible = false;
                LadyCheckButton10.Visible = false;
                ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoiceAcceptButton1.Visible = true;
                ChoiceAcceptButton2.Visible = true;
                ChoiceAcceptButton3.Visible = true;
                ChoiceAcceptButton4.Visible = true;
                ChoiceAcceptButton5.Visible = true;
                ChoiceAcceptButton6.Visible = true;
                ChoiceAcceptButton7.Visible = true;
                ChoiceAcceptButton8.Visible = true;
                ChoiceAcceptButton9.Visible = true;
                ChoiceAcceptButton10.Visible = true;
                ChoiceAcceptButton1.Text = "Zgoda";
                ChoiceAcceptButton2.Text = "Zgoda";
                ChoiceAcceptButton3.Text = "Zgoda";
                ChoiceAcceptButton4.Text = "Zgoda";
                ChoiceAcceptButton5.Text = "Zgoda";
                ChoiceAcceptButton6.Text = "Zgoda";
                ChoiceAcceptButton7.Text = "Zgoda";
                ChoiceAcceptButton8.Text = "Zgoda";
                ChoiceAcceptButton9.Text = "Zgoda";
                ChoiceAcceptButton10.Text = "Zgoda";
                ChoiceAcceptButton1.Visible = false;
                ChoiceAcceptButton2.Visible = false;
                ChoiceAcceptButton3.Visible = false;
                ChoiceAcceptButton4.Visible = false;
                ChoiceAcceptButton5.Visible = false;
                ChoiceAcceptButton6.Visible = false;
                ChoiceAcceptButton7.Visible = false;
                ChoiceAcceptButton8.Visible = false;
                ChoiceAcceptButton9.Visible = false;
                ChoiceAcceptButton10.Visible = false;
                ChoiceAgainstButton1.Visible = true;
                ChoiceAgainstButton2.Visible = true;
                ChoiceAgainstButton3.Visible = true;
                ChoiceAgainstButton4.Visible = true;
                ChoiceAgainstButton5.Visible = true;
                ChoiceAgainstButton6.Visible = true;
                ChoiceAgainstButton7.Visible = true;
                ChoiceAgainstButton8.Visible = true;
                ChoiceAgainstButton9.Visible = true;
                ChoiceAgainstButton10.Visible = true;
                ChoiceAgainstButton1.Text = "Sprzeciw";
                ChoiceAgainstButton2.Text = "Sprzeciw";
                ChoiceAgainstButton3.Text = "Sprzeciw";
                ChoiceAgainstButton4.Text = "Sprzeciw";
                ChoiceAgainstButton5.Text = "Sprzeciw";
                ChoiceAgainstButton6.Text = "Sprzeciw";
                ChoiceAgainstButton7.Text = "Sprzeciw";
                ChoiceAgainstButton8.Text = "Sprzeciw";
                ChoiceAgainstButton9.Text = "Sprzeciw";
                ChoiceAgainstButton10.Text = "Sprzeciw";
                ChoiceAgainstButton1.Visible = false;
                ChoiceAgainstButton2.Visible = false;
                ChoiceAgainstButton3.Visible = false;
                ChoiceAgainstButton4.Visible = false;
                ChoiceAgainstButton5.Visible = false;
                ChoiceAgainstButton6.Visible = false;
                ChoiceAgainstButton7.Visible = false;
                ChoiceAgainstButton8.Visible = false;
                ChoiceAgainstButton9.Visible = false;
                ChoiceAgainstButton10.Visible = false;
                TeamMembersLeftLabel.Visible = false;
                PlayersInfoLabel.Visible = false;
                MaxSpecialEvilLabel.Visible = false;
                GameResultImage.Visible = false;
                MorganaChoiceImg.Image = null;
                LadyChoiceImg.Image = null;
                MordredChoiceImg.Image = null;
                OberonChoiceImg.Image = null;
                ParsifalChoiceImg.Image = null;
                FailedVotes1.Visible = false;
                FailedVotes2.Visible = false;
                FailedVotes3.Visible = false;
                FailedVotes4.Visible = false;
                FailedVotes5.Visible = false;
                NumberForMission1.Visible = false;
                NumberForMission2.Visible = false;
                NumberForMission3.Visible = false;
                NumberForMission4.Visible = false;
                NumberForMission5.Visible = false;
                MissionResultPic1.Visible = true;
                MissionResultPic2.Visible = true;
                MissionResultPic3.Visible = true;
                MissionResultPic4.Visible = true;
                MissionResultPic5.Visible = true;
                MissionResultPic1.BackgroundImage = Avalon.Properties.Resources.VoteBack;
                MissionResultPic2.BackgroundImage = Avalon.Properties.Resources.VoteBack;
                MissionResultPic3.BackgroundImage = Avalon.Properties.Resources.VoteBack;
                MissionResultPic4.BackgroundImage = Avalon.Properties.Resources.VoteBack;
                MissionResultPic5.BackgroundImage = Avalon.Properties.Resources.VoteBack;
                MissionResultPic1.Image = null;
                MissionResultPic2.Image = null;
                MissionResultPic3.Image = null;
                MissionResultPic4.Image = null;
                MissionResultPic5.Image = null;
                MissionResultPic1.Visible = false;
                MissionResultPic2.Visible = false;
                MissionResultPic3.Visible = false;
                MissionResultPic4.Visible = false;
                MissionResultPic5.Visible = false;
                Mission1Table.Visible = true;
                Mission2Table.Visible = true;
                Mission3Table.Visible = true;
                Mission4Table.Visible = true;
                Mission5Table.Visible = true;
                Mission1Table.Text = "";
                Mission2Table.Text = "";
                Mission3Table.Text = "";
                Mission4Table.Text = "";
                Mission5Table.Text = "";
                Mission1Table.Visible = false;
                Mission2Table.Visible = false;
                Mission3Table.Visible = false;
                Mission4Table.Visible = false;
                Mission5Table.Visible = false;
            }
        }

        private void CantSit()
        {
            Seat1Taken(true, "");
            Seat2Taken(true, "");
            Seat3Taken(true, "");
            Seat4Taken(true, "");
            Seat5Taken(true, "");
            Seat6Taken(true, "");
            Seat7Taken(true, "");
            Seat8Taken(true, "");
            Seat9Taken(true, "");
            Seat10Taken(true, "");

        }

        delegate void AddNickDelegate(string nick);

        private void AddNick(string nick)
        {
            if(Mission1Table.InvokeRequired || Mission2Table.InvokeRequired || Mission3Table.InvokeRequired || Mission4Table.InvokeRequired || Mission5Table.InvokeRequired)
            {
                AddNickDelegate f = new AddNickDelegate(AddNick);
                this.Invoke(f, new object[] { nick });
            }
            else
            {
                switch(round)
                {
                    case 2:
                        Mission1Table.Text += "\n" + nick;
                        break;
                    case 3:
                        Mission2Table.Text += "\n" + nick;
                        break;
                    case 4:
                        Mission3Table.Text += "\n" + nick;
                        break;
                    case 5:
                        Mission4Table.Text += "\n" + nick;
                        break;
                    case 6:
                        Mission5Table.Text += "\n" + nick;
                        break;
                }
            }
        }

        delegate void MissionEndDelegate(int votes, bool succeeded);

        private void MissionEnd(int votes, bool succeeded)
        {
            if(MissionResultPic1.InvokeRequired || MissionResultPic2.InvokeRequired || MissionResultPic3.InvokeRequired || MissionResultPic4.InvokeRequired || MissionResultPic5.InvokeRequired || Mission1Table.InvokeRequired || Mission2Table.InvokeRequired || Mission3Table.InvokeRequired || Mission4Table.InvokeRequired || Mission5Table.InvokeRequired)
            {
                MissionEndDelegate f = new MissionEndDelegate(MissionEnd);
                this.Invoke(f, new object[] { votes, succeeded });
            }
            else
            { 
                switch(round)
                {
                    case 1:
                        if (succeeded)
                        {
                            Mission1Table.ForeColor = Color.Lime;
                            MissionResultPic1.BackgroundImage = Avalon.Properties.Resources.VoteSukces;
                        }
                        else
                        {
                            Mission1Table.ForeColor = Color.Red;
                            MissionResultPic1.BackgroundImage = Avalon.Properties.Resources.VotePorazka;
                        }
                        MissionResultPic1.Image = null;
                        MissionResultPic2.Image = Avalon.Properties.Resources.HighlightedCard;
                        Mission1Table.Text = "Sukcesy : " + votes;
                        Mission1Table.Text += "\nPorażki : " + (info[0] - votes);
                        Mission1Table.Text += "\nSkład:";
                        break;
                    case 2:
                        if (succeeded)
                        {
                            Mission2Table.ForeColor = Color.Lime;
                            MissionResultPic2.BackgroundImage = Avalon.Properties.Resources.VoteSukces;
                        }
                        else
                        {
                            Mission2Table.ForeColor = Color.Red;
                            MissionResultPic2.BackgroundImage = Avalon.Properties.Resources.VotePorazka;
                        }
                        MissionResultPic2.Image = null;
                        MissionResultPic3.Image = Avalon.Properties.Resources.HighlightedCard;
                        Mission2Table.Text = "Sukcesy : " + votes;
                        Mission2Table.Text += "\nPorażki : " + (info[1] - votes);
                        Mission2Table.Text += "\nSkład:";
                        break;
                    case 3:
                        if (succeeded)
                        {
                            Mission3Table.ForeColor = Color.Lime;
                            MissionResultPic3.BackgroundImage = Avalon.Properties.Resources.VoteSukces;
                        }
                        else
                        {
                            Mission3Table.ForeColor = Color.Red;
                            MissionResultPic3.BackgroundImage = Avalon.Properties.Resources.VotePorazka;
                        }
                        MissionResultPic3.Image = null;
                        MissionResultPic4.Image = Avalon.Properties.Resources.HighlightedCard;
                        Mission3Table.Text = "Sukcesy : " + votes;
                        Mission3Table.Text += "\nPorażki : " + (info[2] - votes);
                        Mission3Table.Text += "\nSkład:";
                        break;
                    case 4:
                        if (succeeded)
                        {
                            Mission4Table.ForeColor = Color.Lime;
                            MissionResultPic4.BackgroundImage = Avalon.Properties.Resources.VoteSukces;
                        }
                        else
                        {
                            Mission4Table.ForeColor = Color.Red;
                            MissionResultPic4.BackgroundImage = Avalon.Properties.Resources.VotePorazka;
                        }
                        MissionResultPic4.Image = null;
                        MissionResultPic5.Image = Avalon.Properties.Resources.HighlightedCard;
                        Mission4Table.Text = "Sukcesy : " + votes;
                        Mission4Table.Text += "\nPorażki : " + (info[3] - votes);
                        Mission4Table.Text += "\nSkład:";
                        break;
                    case 5:
                        if (succeeded)
                        {
                            Mission5Table.ForeColor = Color.Lime;
                            MissionResultPic5.BackgroundImage = Avalon.Properties.Resources.VoteSukces;
                        }
                        else
                        {
                            Mission5Table.ForeColor = Color.Red;
                            MissionResultPic5.BackgroundImage = Avalon.Properties.Resources.VotePorazka;
                        }
                        MissionResultPic5.Image = null;
                        Mission5Table.Text = "Sukcesy : " + votes;
                        Mission5Table.Text += "\nPorażki : " + (info[4] - votes);
                        Mission5Table.Text += "\nSkład:";
                        break;
                }
                round++;
                inTeam = 0;
                TeamMembersLeftLabel.Text = info[round - 1].ToString();
            }
        }

        delegate void GameEndDelegate(bool goodWins);

        private void GameEnd(bool goodWins)
        {
            if (AwayButton1.InvokeRequired || AwayButton2.InvokeRequired || AwayButton3.InvokeRequired || AwayButton4.InvokeRequired || AwayButton5.InvokeRequired || AwayButton6.InvokeRequired || AwayButton7.InvokeRequired || AwayButton8.InvokeRequired || AwayButton9.InvokeRequired || AwayButton10.InvokeRequired || SitButton1.InvokeRequired || SitButton2.InvokeRequired || SitButton3.InvokeRequired || SitButton4.InvokeRequired || SitButton5.InvokeRequired || SitButton6.InvokeRequired || SitButton7.InvokeRequired || SitButton8.InvokeRequired || SitButton9.InvokeRequired || SitButton10.InvokeRequired || StartGameButton.InvokeRequired || GameResultImage.InvokeRequired)
            {
                GameEndDelegate f = new GameEndDelegate(GameEnd);
                this.Invoke(f, new object[] { goodWins });
            }
            else
            {
                if (mySeat == 0)
                {
                    StartGameButton.Text = "Restart";
                    stage = 3;
                    int k = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        if (seatsTaken[i])
                        {
                            k++;
                        }
                    }
                    if (k > 4)
                    {
                        StartGameButton.Visible = true;
                    }
                }
                GameResultImage.Visible = true;
                if (goodWins)
                {
                    GameResultImage.Image = Avalon.Properties.Resources.Loss;
                    if (tag == 1)
                    {
                        GameResultImage.Image = Avalon.Properties.Resources.Victory;
                    }
                }
                else
                {
                    GameResultImage.Image = Avalon.Properties.Resources.Victory;
                    if (tag == 1)
                    {
                        GameResultImage.Image = Avalon.Properties.Resources.Loss;
                    }
                }
                if (mySeat != -1)
                {
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
                    switch (mySeat)
                    {
                        case 0:
                            AwayButton1.Visible = true;
                            break;
                        case 1:
                            AwayButton2.Visible = true;
                            break;
                        case 2:
                            AwayButton3.Visible = true;
                            break;
                        case 3:
                            AwayButton4.Visible = true;
                            break;
                        case 4:
                            AwayButton5.Visible = true;
                            break;
                        case 5:
                            AwayButton6.Visible = true;
                            break;
                        case 6:
                            AwayButton7.Visible = true;
                            break;
                        case 7:
                            AwayButton8.Visible = true;
                            break;
                        case 8:
                            AwayButton9.Visible = true;
                            break;
                        case 9:
                            AwayButton10.Visible = true;
                            break;
                    }
                }
            }
        }

        delegate void TeamAcceptedDelegate();

        private void TeamAccepted()
        {
            if (ChoicePicture1.InvokeRequired || ChoicePicture2.InvokeRequired || ChoicePicture3.InvokeRequired || ChoicePicture4.InvokeRequired || ChoicePicture5.InvokeRequired || ChoicePicture6.InvokeRequired || ChoicePicture7.InvokeRequired || ChoicePicture8.InvokeRequired || ChoicePicture9.InvokeRequired || ChoicePicture10.InvokeRequired || FailedVotes1.InvokeRequired || FailedVotes2.InvokeRequired || FailedVotes3.InvokeRequired || FailedVotes4.InvokeRequired || FailedVotes5.InvokeRequired || ChoiceAcceptButton1.InvokeRequired || ChoiceAcceptButton2.InvokeRequired || ChoiceAcceptButton3.InvokeRequired || ChoiceAcceptButton4.InvokeRequired || ChoiceAcceptButton5.InvokeRequired || ChoiceAcceptButton6.InvokeRequired || ChoiceAcceptButton7.InvokeRequired || ChoiceAcceptButton8.InvokeRequired || ChoiceAcceptButton9.InvokeRequired || ChoiceAcceptButton10.InvokeRequired || ChoiceAgainstButton1.InvokeRequired || ChoiceAgainstButton2.InvokeRequired || ChoiceAgainstButton3.InvokeRequired || ChoiceAgainstButton4.InvokeRequired || ChoiceAgainstButton5.InvokeRequired || ChoiceAgainstButton6.InvokeRequired || ChoiceAgainstButton7.InvokeRequired || ChoiceAgainstButton8.InvokeRequired || ChoiceAgainstButton9.InvokeRequired || ChoiceAgainstButton10.InvokeRequired)
            {
                TeamAcceptedDelegate f = new TeamAcceptedDelegate(TeamAccepted);
                this.Invoke(f, new object[] { });
            }
            else
            {
                FailedVotes1.Visible = false;
                FailedVotes2.Visible = false;
                FailedVotes3.Visible = false;
                FailedVotes4.Visible = false;
                FailedVotes5.Visible = false;
                ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                switch (mySeat)
                {
                    case 0:
                        if (InTeamIcon1.Visible)
                        {
                            ChoiceAcceptButton1.Visible = true;
                            ChoiceAcceptButton1.Text = "Sukces";
                            ChoiceAgainstButton1.Visible = true;
                            ChoiceAgainstButton1.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton1.Visible = false;
                                ChoiceAgainstButton1.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 1:
                        if (InTeamIcon2.Visible)
                        {
                            ChoiceAcceptButton2.Visible = true;
                            ChoiceAcceptButton2.Text = "Sukces";
                            ChoiceAgainstButton2.Visible = true;
                            ChoiceAgainstButton2.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton2.Visible = false;
                                ChoiceAgainstButton2.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 2:
                        if (InTeamIcon3.Visible)
                        {
                            ChoiceAcceptButton3.Visible = true;
                            ChoiceAcceptButton3.Text = "Sukces";
                            ChoiceAgainstButton3.Visible = true;
                            ChoiceAgainstButton3.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton3.Visible = false;
                                ChoiceAgainstButton3.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 3:
                        if (InTeamIcon4.Visible)
                        {
                            ChoiceAcceptButton4.Visible = true;
                            ChoiceAcceptButton4.Text = "Sukces";
                            ChoiceAgainstButton4.Visible = true;
                            ChoiceAgainstButton4.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton4.Visible = false;
                                ChoiceAgainstButton4.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 4:
                        if (InTeamIcon5.Visible)
                        {
                            ChoiceAcceptButton5.Visible = true;
                            ChoiceAcceptButton5.Text = "Sukces";
                            ChoiceAgainstButton5.Visible = true;
                            ChoiceAgainstButton5.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton5.Visible = false;
                                ChoiceAgainstButton5.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 5:
                        if (InTeamIcon6.Visible)
                        {
                            ChoiceAcceptButton6.Visible = true;
                            ChoiceAcceptButton6.Text = "Sukces";
                            ChoiceAgainstButton6.Visible = true;
                            ChoiceAgainstButton6.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton6.Visible = false;
                                ChoiceAgainstButton6.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 6:
                        if (InTeamIcon7.Visible)
                        {
                            ChoiceAcceptButton7.Visible = true;
                            ChoiceAcceptButton7.Text = "Sukces";
                            ChoiceAgainstButton7.Visible = true;
                            ChoiceAgainstButton7.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton7.Visible = false;
                                ChoiceAgainstButton7.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 7:
                        if (InTeamIcon8.Visible)
                        {
                            ChoiceAcceptButton8.Visible = true;
                            ChoiceAcceptButton8.Text = "Sukces";
                            ChoiceAgainstButton8.Visible = true;
                            ChoiceAgainstButton8.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton8.Visible = false;
                                ChoiceAgainstButton8.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 8:
                        if (InTeamIcon9.Visible)
                        {
                            ChoiceAcceptButton9.Visible = true;
                            ChoiceAcceptButton9.Text = "Sukces";
                            ChoiceAgainstButton9.Visible = true;
                            ChoiceAgainstButton9.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton9.Visible = false;
                                ChoiceAgainstButton9.Text = "Sprzeciw";
                            }
                        }
                        break;
                    case 9:
                        if (InTeamIcon10.Visible)
                        {
                            ChoiceAcceptButton10.Visible = true;
                            ChoiceAcceptButton10.Text = "Sukces";
                            ChoiceAgainstButton10.Visible = true;
                            ChoiceAgainstButton10.Text = "Porażka";
                            if (tag == 1)
                            {
                                ChoiceAgainstButton10.Visible = false;
                                ChoiceAgainstButton10.Text = "Sprzeciw";
                            }
                        }
                        break;
                }
            }
        }

        delegate void TeamRejectedDelegate();

        private void TeamRejected()
        {
            if (FailedVotes1.InvokeRequired || FailedVotes2.InvokeRequired || FailedVotes3.InvokeRequired || FailedVotes4.InvokeRequired || FailedVotes5.InvokeRequired || ChoicePicture1.InvokeRequired || ChoicePicture2.InvokeRequired || ChoicePicture3.InvokeRequired || ChoicePicture4.InvokeRequired || ChoicePicture5.InvokeRequired || ChoicePicture6.InvokeRequired || ChoicePicture7.InvokeRequired || ChoicePicture8.InvokeRequired || ChoicePicture9.InvokeRequired || ChoicePicture10.InvokeRequired)
            {
                TeamRejectedDelegate f = new TeamRejectedDelegate(TeamRejected);
                this.Invoke(f, new object[] { });
            }
            else
            {
                if (FailedVotes4.Visible)
                {
                    FailedVotes5.Visible = true;
                }
                else if (FailedVotes3.Visible)
                {
                    FailedVotes4.Visible = true;
                }
                else if (FailedVotes2.Visible)
                {
                    FailedVotes3.Visible = true;
                }
                else if (FailedVotes1.Visible)
                {
                    FailedVotes2.Visible = true;
                }
                else
                {
                    FailedVotes1.Visible = true;
                }
                ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteTeam;
                inTeam = 0;
                TeamMembersLeftLabel.Text = info[round-1].ToString();
            }
        }

        delegate void ShowVoteDelegate(int seat, bool accepted);

        private void ShowVote(int seat, bool accepted)
        {
            if (ChoicePicture1.InvokeRequired || ChoicePicture2.InvokeRequired || ChoicePicture3.InvokeRequired || ChoicePicture4.InvokeRequired || ChoicePicture5.InvokeRequired || ChoicePicture6.InvokeRequired || ChoicePicture7.InvokeRequired || ChoicePicture8.InvokeRequired || ChoicePicture9.InvokeRequired || ChoicePicture10.InvokeRequired)
            {
                ShowVoteDelegate f = new ShowVoteDelegate(ShowVote);
                this.Invoke(f, new object[] { seat, accepted });
            }
            else
            {
                switch (seat)
                {
                    case 0:
                        if (accepted)
                        {
                            ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 1:
                        if (accepted)
                        {
                            ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 2:
                        if (accepted)
                        {
                            ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 3:
                        if (accepted)
                        {
                            ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 4:
                        if (accepted)
                        {
                            ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 5:
                        if (accepted)
                        {
                            ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 6:
                        if (accepted)
                        {
                            ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 7:
                        if (accepted)
                        {
                            ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 8:
                        if (accepted)
                        {
                            ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                    case 9:
                        if (accepted)
                        {
                            ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
                        }
                        else
                        {
                            ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
                        }
                        break;
                }
            }
        }

        delegate void StartVoteDelegate();

        private void StartVote()
        {
            if (ChoiceAcceptButton1.InvokeRequired || ChoiceAcceptButton2.InvokeRequired || ChoiceAcceptButton3.InvokeRequired || ChoiceAcceptButton4.InvokeRequired || ChoiceAcceptButton5.InvokeRequired || ChoiceAcceptButton6.InvokeRequired || ChoiceAcceptButton7.InvokeRequired || ChoiceAcceptButton8.InvokeRequired || ChoiceAcceptButton9.InvokeRequired || ChoiceAcceptButton10.InvokeRequired || ChoiceAgainstButton1.InvokeRequired || ChoiceAgainstButton2.InvokeRequired || ChoiceAgainstButton3.InvokeRequired || ChoiceAgainstButton4.InvokeRequired || ChoiceAgainstButton5.InvokeRequired || ChoiceAgainstButton6.InvokeRequired || ChoiceAgainstButton7.InvokeRequired || ChoiceAgainstButton8.InvokeRequired || ChoiceAgainstButton9.InvokeRequired || ChoiceAgainstButton10.InvokeRequired)
            {
                StartVoteDelegate f = new StartVoteDelegate(StartVote);
                this.Invoke(f, new object[] { });
            }
            else
            {
                switch (mySeat)
                {
                    case 0:
                        ChoiceAcceptButton1.Visible = true;
                        ChoiceAgainstButton1.Visible = true;
                        break;
                    case 1:
                        ChoiceAcceptButton2.Visible = true;
                        ChoiceAgainstButton2.Visible = true;
                        break;
                    case 2:
                        ChoiceAcceptButton3.Visible = true;
                        ChoiceAgainstButton3.Visible = true;
                        break;
                    case 3:
                        ChoiceAcceptButton4.Visible = true;
                        ChoiceAgainstButton4.Visible = true;
                        break;
                    case 4:
                        ChoiceAcceptButton5.Visible = true;
                        ChoiceAgainstButton5.Visible = true;
                        break;
                    case 5:
                        ChoiceAcceptButton6.Visible = true;
                        ChoiceAgainstButton6.Visible = true;
                        break;
                    case 6:
                        ChoiceAcceptButton7.Visible = true;
                        ChoiceAgainstButton7.Visible = true;
                        break;
                    case 7:
                        ChoiceAcceptButton8.Visible = true;
                        ChoiceAgainstButton8.Visible = true;
                        break;
                    case 8:
                        ChoiceAcceptButton9.Visible = true;
                        ChoiceAgainstButton9.Visible = true;
                        break;
                    case 9:
                        ChoiceAcceptButton10.Visible = true;
                        ChoiceAgainstButton10.Visible = true;
                        break;
                }
            }
        }

        delegate void CanAddToTeamDelegate(List<int> cantAdd);

        private void CanAddToTeam(List<int> cantAdd)
        {
            if (StartGameButton.InvokeRequired || AddToTeamButton1.InvokeRequired || AddToTeamButton2.InvokeRequired || AddToTeamButton3.InvokeRequired || AddToTeamButton4.InvokeRequired || AddToTeamButton5.InvokeRequired || AddToTeamButton6.InvokeRequired || AddToTeamButton7.InvokeRequired || AddToTeamButton8.InvokeRequired || AddToTeamButton9.InvokeRequired || AddToTeamButton10.InvokeRequired)
            {
                CanAddToTeamDelegate f = new CanAddToTeamDelegate(CanAddToTeam);
                this.Invoke(f, new object[] { cantAdd });
            }
            else
            {
                StartGameButton.Visible = false;
                for (int i = 0; i < 10; i++)
                {
                    if (seatsTaken[i] && !cantAdd.Contains(i))
                    {
                        switch (i)
                        {
                            case 0:
                                AddToTeamButton1.Visible = true;
                                break;
                            case 1:
                                AddToTeamButton2.Visible = true;
                                break;
                            case 2:
                                AddToTeamButton3.Visible = true;
                                break;
                            case 3:
                                AddToTeamButton4.Visible = true;
                                break;
                            case 4:
                                AddToTeamButton5.Visible = true;
                                break;
                            case 5:
                                AddToTeamButton6.Visible = true;
                                break;
                            case 6:
                                AddToTeamButton7.Visible = true;
                                break;
                            case 7:
                                AddToTeamButton8.Visible = true;
                                break;
                            case 8:
                                AddToTeamButton9.Visible = true;
                                break;
                            case 9:
                                AddToTeamButton10.Visible = true;
                                break;
                        }
                    }
                }
            }
        }

        delegate void RemoveFromTeamDelegate(int seat);

        private void RemoveFromTeam(int seat)
        {
            if (TeamMembersLeftLabel.InvokeRequired || InTeamIcon1.InvokeRequired || InTeamIcon2.InvokeRequired || InTeamIcon3.InvokeRequired || InTeamIcon4.InvokeRequired || InTeamIcon5.InvokeRequired || InTeamIcon6.InvokeRequired || InTeamIcon7.InvokeRequired || InTeamIcon8.InvokeRequired || InTeamIcon9.InvokeRequired || InTeamIcon10.InvokeRequired)
            {
                RemoveFromTeamDelegate f = new RemoveFromTeamDelegate(RemoveFromTeam);
                this.Invoke(f, new object[] { seat });
            }
            else
            {
                inTeam--;
                TeamMembersLeftLabel.Text = (info[round - 1] - inTeam).ToString();
                switch (seat)
                {
                    case 0:
                        InTeamIcon1.Visible = false;
                        break;
                    case 1:
                        InTeamIcon2.Visible = false;
                        break;
                    case 2:
                        InTeamIcon3.Visible = false;
                        break;
                    case 3:
                        InTeamIcon4.Visible = false;
                        break;
                    case 4:
                        InTeamIcon5.Visible = false;
                        break;
                    case 5:
                        InTeamIcon6.Visible = false;
                        break;
                    case 6:
                        InTeamIcon7.Visible = false;
                        break;
                    case 7:
                        InTeamIcon8.Visible = false;
                        break;
                    case 8:
                        InTeamIcon9.Visible = false;
                        break;
                    case 9:
                        InTeamIcon10.Visible = false;
                        break;
                }
            }
        }

        delegate void FullTeamDelegate();

        private void FullTeam()
        {
            if (StartGameButton.InvokeRequired || AddToTeamButton1.InvokeRequired || AddToTeamButton2.InvokeRequired || AddToTeamButton3.InvokeRequired || AddToTeamButton4.InvokeRequired || AddToTeamButton5.InvokeRequired || AddToTeamButton6.InvokeRequired || AddToTeamButton7.InvokeRequired || AddToTeamButton8.InvokeRequired || AddToTeamButton9.InvokeRequired || AddToTeamButton10.InvokeRequired)
            {
                FullTeamDelegate f = new FullTeamDelegate(FullTeam);
                this.Invoke(f, new object[] { });
            }
            else
            {
                StartGameButton.Visible = true;
                AddToTeamButton1.Visible = false;
                AddToTeamButton2.Visible = false;
                AddToTeamButton3.Visible = false;
                AddToTeamButton4.Visible = false;
                AddToTeamButton5.Visible = false;
                AddToTeamButton6.Visible = false;
                AddToTeamButton7.Visible = false;
                AddToTeamButton8.Visible = false;
                AddToTeamButton9.Visible = false;
                AddToTeamButton10.Visible = false;
            }
        }

        delegate void AddToTeamDelegate(int seat);

        private void AddToTeam(int seat)
        {
            if (TeamMembersLeftLabel.InvokeRequired || InTeamIcon1.InvokeRequired || InTeamIcon2.InvokeRequired || InTeamIcon3.InvokeRequired || InTeamIcon4.InvokeRequired || InTeamIcon5.InvokeRequired || InTeamIcon6.InvokeRequired || InTeamIcon7.InvokeRequired || InTeamIcon8.InvokeRequired || InTeamIcon9.InvokeRequired || InTeamIcon10.InvokeRequired)
            {
                AddToTeamDelegate f = new AddToTeamDelegate(AddToTeam);
                this.Invoke(f, new object[] { seat });
            }
            else
            {
                inTeam++;
                TeamMembersLeftLabel.Text = (info[round - 1] - inTeam).ToString();
                switch (seat)
                {
                    case 0:
                        InTeamIcon1.Visible = true;
                        break;
                    case 1:
                        InTeamIcon2.Visible = true;
                        break;
                    case 2:
                        InTeamIcon3.Visible = true;
                        break;
                    case 3:
                        InTeamIcon4.Visible = true;
                        break;
                    case 4:
                        InTeamIcon5.Visible = true;
                        break;
                    case 5:
                        InTeamIcon6.Visible = true;
                        break;
                    case 6:
                        InTeamIcon7.Visible = true;
                        break;
                    case 7:
                        InTeamIcon8.Visible = true;
                        break;
                    case 8:
                        InTeamIcon9.Visible = true;
                        break;
                    case 9:
                        InTeamIcon10.Visible = true;
                        break;
                }
            }
        }

        delegate void MissionHighlightDelegate(int number);

        private void MissionHighlight(int number)
        {
            if (TeamMembersLeftLabel.InvokeRequired || MissionResultPic1.InvokeRequired || MissionResultPic2.InvokeRequired || MissionResultPic3.InvokeRequired || MissionResultPic4.InvokeRequired || MissionResultPic5.InvokeRequired)
            {
                MissionHighlightDelegate f = new MissionHighlightDelegate(MissionHighlight);
                this.Invoke(f, new object[] { number });
            }
            else
            {
                MissionResultPic1.Image = null;
                MissionResultPic2.Image = null;
                MissionResultPic3.Image = null;
                MissionResultPic4.Image = null;
                MissionResultPic5.Image = null;
                switch (number)
                {
                    case 1:
                        MissionResultPic1.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 2:
                        MissionResultPic2.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 3:
                        MissionResultPic3.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 4:
                        MissionResultPic4.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 5:
                        MissionResultPic5.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                }
                TeamMembersLeftLabel.Visible = true;
                TeamMembersLeftLabel.Text = info[0].ToString();
            }
        }

        delegate void ChooseStageToGameStageDelegate();

        private void ChooseStageToGameStage()
        {
            if (StartGameButton.InvokeRequired || LadyChoiceImg.InvokeRequired || MordredChoiceImg.InvokeRequired || MorganaChoiceImg.InvokeRequired || OberonChoiceImg.InvokeRequired || ParsifalChoiceImg.InvokeRequired || MaxSpecialEvilLabel.InvokeRequired || NumberForMission1.InvokeRequired || NumberForMission2.InvokeRequired || NumberForMission3.InvokeRequired || NumberForMission4.InvokeRequired || NumberForMission5.InvokeRequired || Mission1Table.InvokeRequired || Mission2Table.InvokeRequired || Mission3Table.InvokeRequired || Mission4Table.InvokeRequired || Mission5Table.InvokeRequired || MissionResultPic1.InvokeRequired || MissionResultPic2.InvokeRequired || MissionResultPic3.InvokeRequired || MissionResultPic4.InvokeRequired || MissionResultPic5.InvokeRequired)
            {
                ChooseStageToGameStageDelegate f = new ChooseStageToGameStageDelegate(ChooseStageToGameStage);
                this.Invoke(f, new object[] { });
            }
            else
            {
                LadyChoiceImg.Visible = false;
                MordredChoiceImg.Visible = false;
                MorganaChoiceImg.Visible = false;
                OberonChoiceImg.Visible = false;
                ParsifalChoiceImg.Visible = false;
                MaxSpecialEvilLabel.Visible = false;
                NumberForMission1.Visible = true;
                NumberForMission1.Text = "Skład : " + info[0];
                NumberForMission2.Visible = true;
                NumberForMission2.Text = "Skład : " + info[1];
                NumberForMission3.Visible = true;
                NumberForMission3.Text = "Skład : " + info[2];
                NumberForMission4.Visible = true;
                NumberForMission4.Text = "Skład : " + info[3];
                NumberForMission5.Visible = true;
                NumberForMission5.Text = "Skład : " + info[4];
                Mission1Table.Visible = true;
                Mission1Table.Text = "";
                Mission2Table.Visible = true;
                Mission2Table.Text = "";
                Mission3Table.Visible = true;
                Mission3Table.Text = "";
                Mission4Table.Visible = true;
                Mission4Table.Text = "";
                Mission5Table.Visible = true;
                Mission5Table.Text = "";
                MissionResultPic1.Visible = true;
                MissionResultPic2.Visible = true;
                MissionResultPic3.Visible = true;
                MissionResultPic4.Visible = true;
                MissionResultPic5.Visible = true;
                StartGameButton.Text = "Accept";
            }
        }

        delegate void ChangeLadyDelegate(int seat);

        private void ChangeLady(int seat)
        {
            if (LadyPicture1.InvokeRequired || LadyPicture2.InvokeRequired || LadyPicture3.InvokeRequired || LadyPicture4.InvokeRequired || LadyPicture5.InvokeRequired || LadyPicture6.InvokeRequired || LadyPicture7.InvokeRequired || LadyPicture8.InvokeRequired || LadyPicture9.InvokeRequired || LadyPicture10.InvokeRequired || LadyCheckButton1.InvokeRequired || LadyCheckButton2.InvokeRequired || LadyCheckButton3.InvokeRequired || LadyCheckButton4.InvokeRequired || LadyCheckButton5.InvokeRequired || LadyCheckButton6.InvokeRequired || LadyCheckButton7.InvokeRequired || LadyCheckButton8.InvokeRequired || LadyCheckButton9.InvokeRequired || LadyCheckButton10.InvokeRequired)
            {
                ChangeLadyDelegate f = new ChangeLadyDelegate(ChangeLady);
                this.Invoke(f, new object[] { seat });
            }
            else
            {
                LadyPicture1.Visible = false;
                LadyPicture2.Visible = false;
                LadyPicture3.Visible = false;
                LadyPicture4.Visible = false;
                LadyPicture5.Visible = false;
                LadyPicture6.Visible = false;
                LadyPicture7.Visible = false;
                LadyPicture8.Visible = false;
                LadyPicture9.Visible = false;
                LadyPicture10.Visible = false;
                LadyCheckButton1.Visible = false;
                LadyCheckButton2.Visible = false;
                LadyCheckButton3.Visible = false;
                LadyCheckButton4.Visible = false;
                LadyCheckButton5.Visible = false;
                LadyCheckButton6.Visible = false;
                LadyCheckButton7.Visible = false;
                LadyCheckButton8.Visible = false;
                LadyCheckButton9.Visible = false;
                LadyCheckButton10.Visible = false;
                if(seat==mySeat)
                {
                    lady = true;
                }
                else
                {
                    lady = false;
                }
                switch (seat)
                {
                    case 0:
                        LadyPicture1.Visible = true;
                        break;
                    case 1:
                        LadyPicture2.Visible = true;
                        break;
                    case 2:
                        LadyPicture3.Visible = true;
                        break;
                    case 3:
                        LadyPicture4.Visible = true;
                        break;
                    case 4:
                        LadyPicture5.Visible = true;
                        break;
                    case 5:
                        LadyPicture6.Visible = true;
                        break;
                    case 6:
                        LadyPicture7.Visible = true;
                        break;
                    case 7:
                        LadyPicture8.Visible = true;
                        break;
                    case 8:
                        LadyPicture9.Visible = true;
                        break;
                    case 9:
                        LadyPicture10.Visible = true;
                        break;
                }
            }
        }

        delegate void ChangeLeaderDelegate(int seat);

        private void ChangeLeader(int seat)
        {
            if (InTeamIcon1.InvokeRequired || InTeamIcon2.InvokeRequired || InTeamIcon3.InvokeRequired || InTeamIcon4.InvokeRequired || InTeamIcon5.InvokeRequired || InTeamIcon6.InvokeRequired || InTeamIcon7.InvokeRequired || InTeamIcon8.InvokeRequired || InTeamIcon9.InvokeRequired || InTeamIcon10.InvokeRequired || LeaderIcon1.InvokeRequired || LeaderIcon2.InvokeRequired || LeaderIcon3.InvokeRequired || LeaderIcon4.InvokeRequired || LeaderIcon5.InvokeRequired || LeaderIcon6.InvokeRequired || LeaderIcon7.InvokeRequired || LeaderIcon8.InvokeRequired || LeaderIcon9.InvokeRequired || LeaderIcon10.InvokeRequired)
            {
                ChangeLeaderDelegate f = new ChangeLeaderDelegate(ChangeLeader);
                this.Invoke(f, new object[] { seat });
            }
            else
            {
                InTeamIcon1.Visible = false;
                InTeamIcon2.Visible = false;
                InTeamIcon3.Visible = false;
                InTeamIcon4.Visible = false;
                InTeamIcon5.Visible = false;
                InTeamIcon6.Visible = false;
                InTeamIcon7.Visible = false;
                InTeamIcon8.Visible = false;
                InTeamIcon9.Visible = false;
                InTeamIcon10.Visible = false;
                LeaderIcon1.Visible = false;
                LeaderIcon2.Visible = false;
                LeaderIcon3.Visible = false;
                LeaderIcon4.Visible = false;
                LeaderIcon5.Visible = false;
                LeaderIcon6.Visible = false;
                LeaderIcon7.Visible = false;
                LeaderIcon8.Visible = false;
                LeaderIcon9.Visible = false;
                LeaderIcon10.Visible = false;
                switch (seat)
                {
                    case 0:
                        LeaderIcon1.Visible = true;
                        break;
                    case 1:
                        LeaderIcon2.Visible = true;
                        break;
                    case 2:
                        LeaderIcon3.Visible = true;
                        break;
                    case 3:
                        LeaderIcon4.Visible = true;
                        break;
                    case 4:
                        LeaderIcon5.Visible = true;
                        break;
                    case 5:
                        LeaderIcon6.Visible = true;
                        break;
                    case 6:
                        LeaderIcon7.Visible = true;
                        break;
                    case 7:
                        LeaderIcon8.Visible = true;
                        break;
                    case 8:
                        LeaderIcon9.Visible = true;
                        break;
                    case 9:
                        LeaderIcon10.Visible = true;
                        break;
                }
                if (seat == mySeat)
                {
                    if (seatsTaken[0])
                    {
                        AddToTeamButton1.Visible = true;
                    }
                    if (seatsTaken[1])
                    {
                        AddToTeamButton2.Visible = true;
                    }
                    if (seatsTaken[2])
                    {
                        AddToTeamButton3.Visible = true;
                    }
                    if (seatsTaken[3])
                    {
                        AddToTeamButton4.Visible = true;
                    }
                    if (seatsTaken[4])
                    {
                        AddToTeamButton5.Visible = true;
                    }
                    if (seatsTaken[5])
                    {
                        AddToTeamButton6.Visible = true;
                    }
                    if (seatsTaken[6])
                    {
                        AddToTeamButton7.Visible = true;
                    }
                    if (seatsTaken[7])
                    {
                        AddToTeamButton8.Visible = true;
                    }
                    if (seatsTaken[8])
                    {
                        AddToTeamButton9.Visible = true;
                    }
                    if (seatsTaken[9])
                    {
                        AddToTeamButton10.Visible = true;
                    }
                }
            }
        }

        delegate void HighlightCharacterDelegate(int seat);

        private void HighlightCharacter(int seat)
        {
            if (CharacterPicture1.InvokeRequired || CharacterPicture2.InvokeRequired || CharacterPicture3.InvokeRequired || CharacterPicture4.InvokeRequired || CharacterPicture5.InvokeRequired || CharacterPicture6.InvokeRequired || CharacterPicture7.InvokeRequired || CharacterPicture8.InvokeRequired || CharacterPicture9.InvokeRequired || CharacterPicture10.InvokeRequired)
            {
                HighlightCharacterDelegate f = new HighlightCharacterDelegate(HighlightCharacter);
                this.Invoke(f, new object[] { seat });
            }
            else
            {
                switch (seat)
                {
                    case 0:
                        CharacterPicture1.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 1:
                        CharacterPicture2.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 2:
                        CharacterPicture3.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 3:
                        CharacterPicture4.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 4:
                        CharacterPicture5.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 5:
                        CharacterPicture6.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 6:
                        CharacterPicture7.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 7:
                        CharacterPicture8.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 8:
                        CharacterPicture9.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                    case 9:
                        CharacterPicture10.Image = Avalon.Properties.Resources.HighlightedCard;
                        break;
                }
            }
        }

        delegate void SetRoleDelegate(string name, int seat);

        private void ChooseSeatToSetRole(string name, int seat)
        {
            if (CharacterPicture1.InvokeRequired || CharacterPicture2.InvokeRequired || CharacterPicture3.InvokeRequired || CharacterPicture4.InvokeRequired || CharacterPicture5.InvokeRequired || CharacterPicture6.InvokeRequired || CharacterPicture7.InvokeRequired || CharacterPicture8.InvokeRequired || CharacterPicture9.InvokeRequired || CharacterPicture10.InvokeRequired)
            {
                SetRoleDelegate f = new SetRoleDelegate(ChooseSeatToSetRole);
                this.Invoke(f, new object[] { name, seat });
            }
            else
            {
                switch (seat)
                {
                    case 0:
                        CharacterPicture1.BackgroundImage = SetRole(name);
                        break;
                    case 1:
                        CharacterPicture2.BackgroundImage = SetRole(name);
                        break;
                    case 2:
                        CharacterPicture3.BackgroundImage = SetRole(name);
                        break;
                    case 3:
                        CharacterPicture4.BackgroundImage = SetRole(name);
                        break;
                    case 4:
                        CharacterPicture5.BackgroundImage = SetRole(name);
                        break;
                    case 5:
                        CharacterPicture6.BackgroundImage = SetRole(name);
                        break;
                    case 6:
                        CharacterPicture7.BackgroundImage = SetRole(name);
                        break;
                    case 7:
                        CharacterPicture8.BackgroundImage = SetRole(name);
                        break;
                    case 8:
                        CharacterPicture9.BackgroundImage = SetRole(name);
                        break;
                    case 9:
                        CharacterPicture10.BackgroundImage = SetRole(name);
                        break;
                }
            }
        }

        private Image SetRole(string name)
        {
            Image role = null;
            assassin = false;
            switch (name)
            {
                case "Merlin":
                    role = Avalon.Properties.Resources.Merlin;
                    tag = 1;
                    break;
                case "Persifal":
                    role = Avalon.Properties.Resources.Parsifal;
                    tag = 1;
                    break;
                case "Morgana":
                    role = Avalon.Properties.Resources.Morgana;
                    tag = 2;
                    break;
                case "Oberon":
                    role = Avalon.Properties.Resources.Oberon;
                    tag = 2;
                    break;
                case "Skrytobójca":
                    role = Avalon.Properties.Resources.Skrytobojca;
                    tag = 2;
                    assassin = true;
                    break;
                case "Evil":
                    role = Avalon.Properties.Resources.PoplecznikMordreda3;
                    tag = 2;
                    break;
                case "Good":
                    role = Avalon.Properties.Resources.PoddanyArtura5;
                    tag = 1;
                    break;
                case "Mordred":
                    role = Avalon.Properties.Resources.Mordred;
                    tag = 2;
                    break;
            }
            return role;
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
            if (StartGameButton.InvokeRequired)
            {
                CanStartGameDelegate f = new CanStartGameDelegate(CanStartGame);
                this.Invoke(f, new object[] { canStart });
            }
            else
            {
                if (canStart)
                {
                    StartGameButton.Visible = true;
                }
                else
                {
                    StartGameButton.Visible = false;
                }
            }
        }

        delegate void MySeatChangedDelegate(int number);

        private void MySeatChanged(int number)
        {
            if (AwayButton1.InvokeRequired || AwayButton2.InvokeRequired || AwayButton3.InvokeRequired || AwayButton4.InvokeRequired || AwayButton5.InvokeRequired || AwayButton6.InvokeRequired || AwayButton7.InvokeRequired || AwayButton8.InvokeRequired || AwayButton9.InvokeRequired || AwayButton10.InvokeRequired || SitButton1.InvokeRequired || SitButton2.InvokeRequired || SitButton3.InvokeRequired || SitButton4.InvokeRequired || SitButton5.InvokeRequired || SitButton6.InvokeRequired || SitButton7.InvokeRequired || SitButton8.InvokeRequired || SitButton9.InvokeRequired || SitButton10.InvokeRequired)
            {
                MySeatChangedDelegate f = new MySeatChangedDelegate(MySeatChanged);
                this.Invoke(f, new object[] { number });
            }
            else
            {
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
                mySeat = number;
                switch (mySeat)
                {
                    case 0:
                        AwayButton1.Visible = true;
                        break;
                    case 1:
                        AwayButton2.Visible = true;
                        break;
                    case 2:
                        AwayButton3.Visible = true;
                        break;
                    case 3:
                        AwayButton4.Visible = true;
                        break;
                    case 4:
                        AwayButton5.Visible = true;
                        break;
                    case 5:
                        AwayButton6.Visible = true;
                        break;
                    case 6:
                        AwayButton7.Visible = true;
                        break;
                    case 7:
                        AwayButton8.Visible = true;
                        break;
                    case 8:
                        AwayButton9.Visible = true;
                        break;
                    case 9:
                        AwayButton10.Visible = true;
                        break;
                }
            }
        }

        private void SeatTaken(int number, bool taken, string nick)
        {
            switch (number)
            {
                case 0:
                    Seat1Taken(taken, nick);
                    break;
                case 1:
                    Seat2Taken(taken, nick);
                    break;
                case 2:
                    Seat3Taken(taken, nick);
                    break;
                case 3:
                    Seat4Taken(taken, nick);
                    break;
                case 4:
                    Seat5Taken(taken, nick);
                    break;
                case 5:
                    Seat6Taken(taken, nick);
                    break;
                case 6:
                    Seat7Taken(taken, nick);
                    break;
                case 7:
                    Seat8Taken(taken, nick);
                    break;
                case 8:
                    Seat9Taken(taken, nick);
                    break;
                case 9:
                    Seat10Taken(taken, nick);
                    break;
            }
        }

        delegate void SeatTakenDelegate(bool taken, string nick);

        private void Seat1Taken(bool taken, string nick)
        {
            if (RemoveFromTeamButton1.InvokeRequired || ChoiceAgainstButton1.InvokeRequired || ChoiceAcceptButton1.InvokeRequired || CharacterPicture1.InvokeRequired || LadyCheckButton1.InvokeRequired || AddToTeamButton1.InvokeRequired || ChoicePicture1.InvokeRequired || NicknameLabel1.InvokeRequired || LadyPicture1.InvokeRequired || LeaderIcon1.InvokeRequired || AwayButton1.InvokeRequired || InTeamIcon1.InvokeRequired || SitButton1.InvokeRequired)
            {
                SeatTakenDelegate f = new SeatTakenDelegate(Seat1Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel1.Text = nick;
                seatsTaken[0] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat2Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel2.Text = nick;
                seatsTaken[1] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat3Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel3.Text = nick;
                seatsTaken[2] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat4Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel4.Text = nick;
                seatsTaken[3] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat5Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel5.Text = nick;
                seatsTaken[4] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat6Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel6.Text = nick;
                seatsTaken[5] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat7Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel7.Text = nick;
                seatsTaken[6] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat8Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel8.Text = nick;
                seatsTaken[7] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat9Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel9.Text = nick;
                seatsTaken[8] = taken;
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
                SeatTakenDelegate f = new SeatTakenDelegate(Seat10Taken);
                this.Invoke(f, new object[] { taken, nick });
            }
            else
            {
                NicknameLabel10.Text = nick;
                seatsTaken[9] = taken;
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
            if (server != null && server.Connected)
            {
                bw.Write("leaving");
                bw.Write("stand");
                bw.Write("disconnect");
                server.Close();
            }
            Application.Exit();
        }

        private void ExitButton_MouseEnter(object sender, EventArgs e)
        {
            ExitButton.Image = Avalon.Properties.Resources.xHighlight;
        }

        private void ExitButton_MouseLeave(object sender, EventArgs e)
        {
            ExitButton.Image = null;
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void MinimizeButton_MouseEnter(object sender, EventArgs e)
        {
            MinimizeButton.Image = Avalon.Properties.Resources.minimizeHighlight;
        }

        private void MinimizeButton_MouseLeave(object sender, EventArgs e)
        {
            MinimizeButton.Image = null;
        }

        private void SitButton1_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("0");
        }

        private void SitButton2_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("1");
        }

        private void SitButton3_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("2");
        }

        private void SitButton4_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("3");
        }

        private void SitButton5_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("4");
        }

        private void SitButton6_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("5");
        }

        private void SitButton7_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("6");
        }

        private void SitButton8_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("7");
        }

        private void SitButton9_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("8");
        }

        private void SitButton10_Click(object sender, EventArgs e)
        {
            bw.Write("takeseat");
            bw.Write("9");
        }

        private void AwayButtons(object sender, EventArgs e)
        {
            bw.Write("stand");
            mySeat = -1;
            tag = 0;
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            switch (stage)
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
                    bw.Write("tryTeam");
                    StartGameButton.Visible = false;
                    RemoveFromTeamButton1.Visible = false;
                    RemoveFromTeamButton2.Visible = false;
                    RemoveFromTeamButton3.Visible = false;
                    RemoveFromTeamButton4.Visible = false;
                    RemoveFromTeamButton5.Visible = false;
                    RemoveFromTeamButton6.Visible = false;
                    RemoveFromTeamButton7.Visible = false;
                    RemoveFromTeamButton8.Visible = false;
                    RemoveFromTeamButton9.Visible = false;
                    RemoveFromTeamButton10.Visible = false;
                    break;
                case 3:
                    bw.Write("restart");
                    StartGameButton.Text = "Start";
                    break;
            }
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

        private void AddToTeamButton1_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("0");
            AddToTeamButton1.Visible = false;
            RemoveFromTeamButton1.Visible = true;
        }

        private void AddToTeamButton2_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("1");
            AddToTeamButton2.Visible = false;
            RemoveFromTeamButton2.Visible = true;
        }

        private void AddToTeamButton3_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("2");
            AddToTeamButton3.Visible = false;
            RemoveFromTeamButton3.Visible = true;
        }

        private void AddToTeamButton4_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("3");
            AddToTeamButton4.Visible = false;
            RemoveFromTeamButton4.Visible = true;
        }

        private void AddToTeamButton5_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("4");
            AddToTeamButton5.Visible = false;
            RemoveFromTeamButton5.Visible = true;
        }

        private void AddToTeamButton6_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("5");
            AddToTeamButton6.Visible = false;
            RemoveFromTeamButton6.Visible = true;
        }

        private void AddToTeamButton7_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("6");
            AddToTeamButton7.Visible = false;
            RemoveFromTeamButton7.Visible = true;
        }

        private void AddToTeamButton8_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("7");
            AddToTeamButton8.Visible = false;
            RemoveFromTeamButton8.Visible = true;
        }

        private void AddToTeamButton9_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("8");
            AddToTeamButton9.Visible = false;
            RemoveFromTeamButton9.Visible = true;
        }

        private void AddToTeamButton10_Click(object sender, EventArgs e)
        {
            bw.Write("addToTeam");
            bw.Write("9");
            AddToTeamButton10.Visible = false;
            RemoveFromTeamButton10.Visible = true;
        }

        private void RemoveFromTeamButton1_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("0");
            AddToTeamButton1.Visible = true;
            RemoveFromTeamButton1.Visible = false;
        }

        private void RemoveFromTeamButton2_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("1");
            AddToTeamButton2.Visible = true;
            RemoveFromTeamButton2.Visible = false;
        }

        private void RemoveFromTeamButton3_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("2");
            AddToTeamButton3.Visible = true;
            RemoveFromTeamButton3.Visible = false;
        }

        private void RemoveFromTeamButton4_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("3");
            AddToTeamButton4.Visible = true;
            RemoveFromTeamButton4.Visible = false;
        }

        private void RemoveFromTeamButton5_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("4");
            AddToTeamButton5.Visible = true;
            RemoveFromTeamButton5.Visible = false;
        }

        private void RemoveFromTeamButton6_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("5");
            AddToTeamButton6.Visible = true;
            RemoveFromTeamButton6.Visible = false;
        }

        private void RemoveFromTeamButton7_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("6");
            AddToTeamButton7.Visible = true;
            RemoveFromTeamButton7.Visible = false;
        }

        private void RemoveFromTeamButton8_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("7");
            AddToTeamButton8.Visible = true;
            RemoveFromTeamButton8.Visible = false;
        }

        private void RemoveFromTeamButton9_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("8");
            AddToTeamButton9.Visible = true;
            RemoveFromTeamButton9.Visible = false;
        }

        private void RemoveFromTeamButton10_Click(object sender, EventArgs e)
        {
            bw.Write("removeFromTeam");
            bw.Write("9");
            AddToTeamButton10.Visible = true;
            RemoveFromTeamButton10.Visible = false;
        }

        private void ChoiceAcceptButton1_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton1.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton1.Visible = false;
                ChoiceAgainstButton1.Visible = false;
                ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton1.Visible = false;
                ChoiceAgainstButton1.Visible = false;
                ChoiceAcceptButton1.Text = "Zgoda";
                ChoiceAgainstButton1.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton2_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton2.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton2.Visible = false;
                ChoiceAgainstButton2.Visible = false;
                ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton2.Visible = false;
                ChoiceAgainstButton2.Visible = false;
                ChoiceAcceptButton2.Text = "Zgoda";
                ChoiceAgainstButton2.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton3_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton3.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton3.Visible = false;
                ChoiceAgainstButton3.Visible = false;
                ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton3.Visible = false;
                ChoiceAgainstButton3.Visible = false;
                ChoiceAcceptButton3.Text = "Zgoda";
                ChoiceAgainstButton3.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton4_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton4.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton4.Visible = false;
                ChoiceAgainstButton4.Visible = false;
                ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton4.Visible = false;
                ChoiceAgainstButton4.Visible = false;
                ChoiceAcceptButton4.Text = "Zgoda";
                ChoiceAgainstButton4.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton5_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton5.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton5.Visible = false;
                ChoiceAgainstButton5.Visible = false;
                ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton5.Visible = false;
                ChoiceAgainstButton5.Visible = false;
                ChoiceAcceptButton5.Text = "Zgoda";
                ChoiceAgainstButton5.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton6_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton6.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton6.Visible = false;
                ChoiceAgainstButton6.Visible = false;
                ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton6.Visible = false;
                ChoiceAgainstButton6.Visible = false;
                ChoiceAcceptButton6.Text = "Zgoda";
                ChoiceAgainstButton6.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton7_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton7.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton7.Visible = false;
                ChoiceAgainstButton7.Visible = false;
                ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton7.Visible = false;
                ChoiceAgainstButton7.Visible = false;
                ChoiceAcceptButton7.Text = "Zgoda";
                ChoiceAgainstButton7.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton8_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton8.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton8.Visible = false;
                ChoiceAgainstButton8.Visible = false;
                ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton8.Visible = false;
                ChoiceAgainstButton8.Visible = false;
                ChoiceAcceptButton8.Text = "Zgoda";
                ChoiceAgainstButton8.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton9_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton9.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton9.Visible = false;
                ChoiceAgainstButton9.Visible = false;
                ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton9.Visible = false;
                ChoiceAgainstButton9.Visible = false;
                ChoiceAcceptButton9.Text = "Zgoda";
                ChoiceAgainstButton9.Text = "Sprzeciw";
            }
        }

        private void ChoiceAcceptButton10_Click(object sender, EventArgs e)
        {
            if (ChoiceAcceptButton10.Text == "Zgoda")
            {
                bw.Write("accept");
                ChoiceAcceptButton10.Visible = false;
                ChoiceAgainstButton10.Visible = false;
                ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteZgoda;
            }
            else
            {
                bw.Write("success");
                ChoiceAcceptButton10.Visible = false;
                ChoiceAgainstButton10.Visible = false;
                ChoiceAcceptButton10.Text = "Zgoda";
                ChoiceAgainstButton10.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton1_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton1.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton1.Visible = false;
                ChoiceAgainstButton1.Visible = false;
                ChoicePicture1.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton1.Visible = false;
                ChoiceAgainstButton1.Visible = false;
                ChoiceAcceptButton1.Text = "Zgoda";
                ChoiceAgainstButton1.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton2_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton2.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton2.Visible = false;
                ChoiceAgainstButton2.Visible = false;
                ChoicePicture2.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton2.Visible = false;
                ChoiceAgainstButton2.Visible = false;
                ChoiceAcceptButton2.Text = "Zgoda";
                ChoiceAgainstButton2.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton3_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton3.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton3.Visible = false;
                ChoiceAgainstButton3.Visible = false;
                ChoicePicture3.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton3.Visible = false;
                ChoiceAgainstButton3.Visible = false;
                ChoiceAcceptButton3.Text = "Zgoda";
                ChoiceAgainstButton3.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton4_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton4.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton4.Visible = false;
                ChoiceAgainstButton4.Visible = false;
                ChoicePicture4.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton4.Visible = false;
                ChoiceAgainstButton4.Visible = false;
                ChoiceAcceptButton4.Text = "Zgoda";
                ChoiceAgainstButton4.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton5_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton5.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton5.Visible = false;
                ChoiceAgainstButton5.Visible = false;
                ChoicePicture5.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton5.Visible = false;
                ChoiceAgainstButton5.Visible = false;
                ChoiceAcceptButton5.Text = "Zgoda";
                ChoiceAgainstButton5.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton6_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton6.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton6.Visible = false;
                ChoiceAgainstButton6.Visible = false;
                ChoicePicture6.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton6.Visible = false;
                ChoiceAgainstButton6.Visible = false;
                ChoiceAcceptButton6.Text = "Zgoda";
                ChoiceAgainstButton6.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton7_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton7.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton7.Visible = false;
                ChoiceAgainstButton7.Visible = false;
                ChoicePicture7.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton7.Visible = false;
                ChoiceAgainstButton7.Visible = false;
                ChoiceAcceptButton7.Text = "Zgoda";
                ChoiceAgainstButton7.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton8_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton8.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton8.Visible = false;
                ChoiceAgainstButton8.Visible = false;
                ChoicePicture8.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton8.Visible = false;
                ChoiceAgainstButton8.Visible = false;
                ChoiceAcceptButton8.Text = "Zgoda";
                ChoiceAgainstButton8.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton9_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton9.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton9.Visible = false;
                ChoiceAgainstButton9.Visible = false;
                ChoicePicture9.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton9.Visible = false;
                ChoiceAgainstButton9.Visible = false;
                ChoiceAcceptButton9.Text = "Zgoda";
                ChoiceAgainstButton9.Text = "Sprzeciw";
            }
        }

        private void ChoiceAgainstButton10_Click(object sender, EventArgs e)
        {
            if (ChoiceAgainstButton10.Text == "Sprzeciw")
            {
                bw.Write("against");
                ChoiceAcceptButton10.Visible = false;
                ChoiceAgainstButton10.Visible = false;
                ChoicePicture10.BackgroundImage = Avalon.Properties.Resources.VoteSprzeciw;
            }
            else
            {
                bw.Write("fail");
                ChoiceAcceptButton10.Visible = false;
                ChoiceAgainstButton10.Visible = false;
                ChoiceAcceptButton10.Text = "Zgoda";
                ChoiceAgainstButton10.Text = "Sprzeciw";
            }
        }

        private void LadyCheckButton1_Click(object sender, EventArgs e)
        {
            if(canAssassin)
            {
                bw.Write("kill");
                bw.Write("0");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("0");
            }
        }

        private void LadyCheckButton2_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("1");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("1");
            }
        }

        private void LadyCheckButton3_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("2");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("2");
            }
        }

        private void LadyCheckButton4_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("3");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("3");
            }
        }

        private void LadyCheckButton5_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("4");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("4");
            }
        }

        private void LadyCheckButton6_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("5");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("5");
            }
        }

        private void LadyCheckButton7_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("6");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("6");
            }
        }

        private void LadyCheckButton8_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("7");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("7");
            }
        }

        private void LadyCheckButton9_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("8");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("8");
            }
        }

        private void LadyCheckButton10_Click(object sender, EventArgs e)
        {
            if (canAssassin)
            {
                bw.Write("kill");
                bw.Write("9");
            }
            else
            {
                bw.Write("checkRole");
                bw.Write("9");
            }
        }
    }
}
