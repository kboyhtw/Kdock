using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data;

namespace Kdock
{
    public partial class Connect : Form
    {
        public Connect()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


        private Size imgSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        private Point ImageLocation;

        private void btnOk_Click(object sender, EventArgs e)
        {
            TcpClient Client;
            if (txtIp.Text.Trim() != "")
            {
                KGlobal.HostName = txtIp.Text;
                KGlobal.port = 8000;
                KGlobal.PortString = 7777;
                try
                {
                    Client = new TcpClient(txtIp.Text.Trim(), KGlobal.PortString);
                    if (Client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(Client.GetStream());
                        sw.Write(RC);
                        sw.Flush();
                        sw.Close();
                    }
                    pConnect.Visible = false;
                }
                catch (Exception ex)
                {
                    pConnect.Visible = true;
                }
            }
            else
                MessageBox.Show("Please Enter ID");
        }
        private void btnChat_Click(object sender, EventArgs e)
        {
            // Listener.Stop()
            RC = "</Chat/>";
            pConnect.Visible = true;
        }
        private string RC = "";
        private void BtnRM_Click(object sender, EventArgs e)
        {
            // Listener.Stop()
            RC = "</Remote/>";
            pConnect.Visible = true;
        }
        public string strHostName;
        public string strIPAddress;
        int ih;
        int iw;
        private void Connect_Load(object sender, EventArgs e)
        {
            ih = Screen.PrimaryScreen.Bounds.Height / 20;
            iw = Screen.PrimaryScreen.Bounds.Width / 20;
            imgSize = new Size(iw, ih);
            ImageLocation = new Point(0, 0);
            strHostName = System.Net.Dns.GetHostName();
            strIPAddress = GetIPv4Address();
            KGlobal.myAddress = strIPAddress + " : " + 7777;
            lblLocalIp.Text = "Local ID : " + strIPAddress + " : " + KGlobal.myAddress.Split(':').Last();
            KGlobal.ListenerString = new TcpListener(IPAddress.Any, 7777);
            KGlobal.ListenerString.Start();
            KGlobal.Listener.Start();
            KGlobal.NetworkListener.Start();
            KGlobal.ListenerFile.Start();
            KGlobal.kdockmain.Label15.Text = " " + strIPAddress + " ";
        }

        private string GetIPv4Address()
        {
            string _GetIPv4Address = string.Empty;
            string strHostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry iphe = System.Net.Dns.GetHostEntry(strHostName);

            foreach (System.Net.IPAddress ipheal in iphe.AddressList)
            {
                if (ipheal.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    _GetIPv4Address = ipheal.ToString();
                    break;
                }
            }
            return _GetIPv4Address;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            TcpClient Client;
            try
            {
                foreach (string itm in hostlist.Items)
                {
                    Client = new TcpClient(itm, KGlobal.port);
                    if (Client.Connected)
                    {
                        //BinaryWriter br = new BinaryWriter(Client.GetStream());
                        //ImageCodecInfo jpgEncoder = KGlobal.GetEncoder(ImageFormat.Jpeg);
                        //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        //System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                        //EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, KGlobal.RmQuality);
                        //myEncoderParameters.Param[0] = myEncoderParameter;
                        //MemoryStream ms = new MemoryStream();
                        //KGlobal.GetScreenShot(imgSize).Save(ms, jpgEncoder, myEncoderParameters);
                        //byte[] bt = ms.GetBuffer();

                        //br.Write(bt);
                        //br.Flush();
                        //br.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void RemoteFrameSender(NetworkStream CStream, string sincom)
        {
            bool b = true;
            while (b)
            {
                try
                {
                    if (CStream.DataAvailable)
                    {
                        byte[] databyte = new byte[1025];
                        Int32 bytes = CStream.Read(databyte, 0, databyte.Length);
                        // stream.Flush()
                        try
                        {
                            string responseData = System.Text.Encoding.ASCII.GetString(databyte, 0, bytes);
                            RemoteResponce(responseData, sincom, CStream);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    ImageCodecInfo jpgEncoder = KGlobal.GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, KGlobal.RmQuality);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    MemoryStream ms = new MemoryStream();
                    // KGlobal.GetScreenShot(imgSize).Save(ms, jpgEncoder, myEncoderParameters);
                    byte[] bt = ms.GetBuffer();
                    string info = "R|" + iw.ToString() + "|" + ih.ToString() + "|" + ImageLocation.X.ToString() + "|" + ImageLocation.Y.ToString();
                    byte[] btinfo = System.Text.ASCIIEncoding.Default.GetBytes(info);
                    CStream.Write(bt, 0, bt.Length);
                }
                catch (Exception ex)
                {
                    b = false;
                }
                Thread.Sleep(100);
            }
        }


        private List<Thread> tlist = new List<Thread>();

        // Private Function TRConatins(ByVal tname As String) As Boolean
        // Dim b As Boolean = False
        // For Each tr As Thread In tlist
        // If tr.Name = tname Then
        // b = True
        // Exit For
        // End If
        // Next
        // Return b
        // End Function

        private void TRRemove(string tname)
        {
            foreach (Thread tr in tlist)
            {
                if (tr.Name == tname)
                {
                    try
                    {
                        tr.Abort();
                    }
                    catch (Exception ex)
                    {
                    }
                    tlist.Remove(tr);
                    break;
                }
            }
        }

        private void TRStart(string tname)
        {
            foreach (Thread tr in tlist)
            {
                if (tr.Name == tname)
                {
                    try
                    {
                        tr.Start();
                    }
                    catch (Exception ex)
                    {
                    }
                    break;
                }
            }
        }

        private void RemoteResponce(string key, string sIncomIP, NetworkStream ClientStream)
        {
            try
            {
                string sIncom = sIncomIP.Split(':')[0];
                if (key == ">??")
                    KGlobal.WriteTextToStream(ClientStream, "<??" + Environment.MachineName);
                else if (key.Contains("<??"))
                {
                    if (KGlobal.LANd.Visible)
                    {
                        if (KGlobal.LANd.papps.Controls.ContainsKey(key.Substring(3, key.Length - 3)))
                        {
                            for (int i = 0; i <= KGlobal.LANd.Controls.Find(key.Substring(3, key.Length - 3), true).Length - 1; i++)
                                ((LanPC)KGlobal.LANd.Controls.Find(key.Substring(3, key.Length - 3), true)[0]).getOnline(sIncom);
                        }
                        // If LANd.papps.Controls.ContainsKey(KEY.Substring(3, KEY.Length - 3)) Then
                        // CType(LANd.Controls.Find(KEY.Substring(3, KEY.Length - 3), True)(0), LanPC).getOnline(sIncom)
                        // End If
                        if (KGlobal.LANd.papps.Controls.ContainsKey(sIncom))
                            ((LanPC)KGlobal.LANd.Controls.Find(sIncom, true)[0]).getOnline(sIncom);
                    }
                    if (KGlobal.oLAN.Visible)
                    {
                        if (KGlobal.oLAN.flpPC.Controls.ContainsKey(sIncom))
                            ((LanPC)KGlobal.oLAN.Controls.Find(sIncom, true)[0]).getOnline(sIncom);
                        if (KGlobal.oLAN.Controls.Find(key.Substring(3, key.Length - 3), true).Length > 0)
                        {
                            for (int i = 0; i <= KGlobal.oLAN.Controls.Find(key.Substring(3, key.Length - 3), true).Length - 1; i++)
                                ((LanPC)KGlobal.oLAN.Controls.Find(key.Substring(3, key.Length - 3), true)[i]).getOnline(sIncom);
                        }
                    }
                }
                else if (key.Contains("</") & key.Contains("/>||"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                    mouse_event(0x8, 0, 0, 0, 0);
                }
                else if (key.Contains("</") & key.Contains("/>??"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                    mouse_event(0x10, 0, 0, 0, 0);
                }
                else if (key.Contains("</") & key.Contains("/>@@"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                    mouse_event(0x20, 0, 0, 0, 0);
                }
                else if (key.Contains("</") & key.Contains("/>##"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                    mouse_event(0x40, 0, 0, 0, 0);
                }
                else if (key.Contains("</") & key.Contains("/>--"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                    mouse_event(0x2, 0, 0, 0, 0);
                }
                else if (key.Contains("</") & key.Contains("/>++"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                    mouse_event(0x4, 0, 0, 0, 0);
                }
                else if (key.Contains("</") & key.Contains("/>**"))
                {
                    string[] @int = key.Substring(2, key.Length - 6).Split(',');
                    Cursor.Position = new Point(int.Parse(@int[0]), int.Parse(@int[1]));
                }
                else if (key == "</Remote/>")
                {
                    if (MessageBox.Show(this, "Request from " + sIncom + " for remote control,you want to accept ? ", "Remote Desktop Request", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (KGlobal.WriteTextToStream(ClientStream, "</ORW/>"))
                        {
                            if (!hostlist.Items.Contains(sIncom))
                                hostlist.Items.Add(sIncom);
                            if (!KGlobal.TRConatins(sIncomIP))
                            {
                                Thread t1 = new Thread(() => RemoteFrameSender(ClientStream, sIncomIP));
                                t1.Name = sIncomIP;
                            }
                        }
                    }
                }
                else if (key == "</Stop/>")
                {
                    if (hostlist.Items.Contains(sIncom))
                        hostlist.Items.Remove(sIncom);
                    TRRemove(sIncomIP);
                }
                else if (key.Contains("</Chat/"))
                {
                }
                else if (key == "</ARW/>")
                {
                    if (KGlobal.WriteTextToStream(ClientStream, "<[" + imgSize.Width + "," + imgSize.Height + "]>"))
                        // tmrFramSender.Start()
                        // tmrControl.Start()
                        TRStart(sIncomIP);
                }
                else if (key.Contains("<#"))
                {
                    bool bPerm = (bool)KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0]["Give"];
                    if (bPerm)
                    {
                        savefile = key.Split('#')[2] + @"\" + key.Split('#')[1].Split('\\').Last();
                        STMode = "R";
                        sbincom = sIncom;
                        KGlobal.WriteTextToStream(ClientStream, "<@" + key.Split('#')[1] + "@S@>");
                    }
                    else
                        KGlobal.WriteTextToStream(ClientStream, "</Denied/>");
                }
                else if (key.Contains("<@"))
                {
                    sfile = key.Split('@')[1];
                    STMode = key.Split('@')[2];
                    bool bPerm;
                    if (STMode == "S")
                        bPerm = true;
                    else
                        bPerm = (bool)KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0]["Get"];
                    if (bPerm)
                    {
                        sbincom = sIncom;
                        // Client = New TcpClient(sIncom,KGlobal.PortString)
                        // If Client.Connected Then
                        // Dim sw As New StreamWriter(Client.GetStream())
                        // sw.Write("<(1(>")
                        // sw.Flush()
                        // sw.Close()
                        // End If

                        if (!bwSender.IsBusy)
                            bwSender.RunWorkerAsync();
                        else if (STMode == "S")
                            MessageBox.Show("One flle copying is going on please try again latter");
                        else
                            KGlobal.WriteTextToStream(ClientStream, "</Busy/>");
                    }
                    else
                        KGlobal.WriteTextToStream(ClientStream, "</Denied/>");
                }
                else if (key == "</Busy/>")
                    MessageBox.Show("One flle copying is going on please try again latter");
                else if (key == "</Failed/>")
                    // For Each frm As Form In Application.OpenForms
                    // If TypeOf frm Is RemoteFiles Then
                    // If frm.Name = "frm" & sIncom Then
                    // CType(frm, RemoteFiles).FileReceived()
                    // End If
                    // End If
                    // Next
                    MessageBox.Show("flle copying failed");
                else if (key == "</STransR/>")
                {
                    try
                    {
                        bwSender.CancelAsync();
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (key == "</STransS/>")
                {
                    try
                    {
                        bwReceiver.CancelAsync();
                        if (File.Exists(savefile))
                            File.Delete(savefile);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (key.Contains("<("))
                    fileReceiving(int.Parse(key.Split('(')[1]), sIncom, key.Split('(')[2]);
                else if (key.Contains("<)"))
                {
                }
                else if (key.Contains("<["))
                {
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.Name == "R" + sIncom)
                            ((frmRemote)frm).clientImgSize = new Size(int.Parse(key.Substring(2, key.Length - 4).Split(',')[0]), int.Parse(key.Substring(2, key.Length - 4).Split(',')[1]));
                    }
                }
                else if (key == "</ORW/>")
                {
                    bool b = false;
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.Name == "R" + sIncom)
                            b = true;
                    }
                    if (b == false)
                    {
                        frmRemote frmRm = new frmRemote();
                        frmRm.ClientStream = ClientStream;
                        frmRm.Tag = sIncom;
                        frmRm.Name = "R" + sIncom;
                        frmRm.Show();
                        this.WindowState = FormWindowState.Minimized;
                    }
                    else
                        MessageBox.Show("This Connection is already active");
                }
                else if (key.Contains("::") & key.Split(':').Length == 6)
                {
                    KGlobal.HostName = key.Split(':')[2].Trim();
                    KGlobal.port = 8000;
                    KGlobal.PortString = 7777;
                }
                else if (key.Contains("<%"))
                    KGlobal.RmQuality = int.Parse(key.Substring(2, key.Length - 4));
                else if (key.Contains("<*>"))
                {
                    bool b = false;
                    frmLanChat frmC;
                    string[] str = key.Split('<');
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.Name == "C" + sIncom | frm.Name == "C" + str[1].Substring(2, str[1].Length - 2))
                        {
                            b = true;
                            frmC = (frmLanChat)frm;
                            frmC.GotMsg(str[2].Substring(2, str[2].Length - 2));
                        }
                    }
                    if (b == false)
                        // frmC = New frmLanChat
                        // frmC.Tag = sIncom
                        // frmC.Name = "C" & sIncom
                        // frmC.Text = sIncom
                        // frmC.Show()
                        File.AppendAllText(KGlobal.path + "InMsg.k", sIncom + KGlobal.smallsep + str[1].Substring(2, str[1].Length - 2) + KGlobal.smallsep + str[2].Substring(2, str[2].Length - 2) + KGlobal.mainsep);
                }
                else if (key == "<__>")
                {
                    if (!KGlobal.access.IsDisposed)
                        KGlobal.access = new frmAccess();
                    string sResult = KGlobal.access.ShowMessege(sIncom, "Request from " + sIncom + " for FileExplorer access,You want to accept ?");
                    if (sResult != null)
                    {
                        if (KGlobal.tblAccesss.Select("IP = '" + sIncom + "'").Count() > 0)
                        {
                            DataRow dr = KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0];
                            dr["Delete"] = System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[1]);
                            dr["CM"] = System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[2]);
                            dr["Get"] = System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[3]);
                            dr["Give"] = System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[4]);
                        }
                        else
                            KGlobal.tblAccesss.Rows.Add(sResult.Split(KGlobal.smallsep)[0], System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[1]), System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[2]), System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[3]), System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[4]));
                        KGlobal.WriteTextToStream(ClientStream, "<|" + Environment.MachineName + "|" + getDrives() + "|>");
                    }
                }
                else if (key.Contains("<^"))
                {
                    if (key == "<^^>")
                        KGlobal.WriteTextToStream(ClientStream, "<|" + Environment.MachineName + "|" + getDrives() + "|>");
                    else
                        KGlobal.WriteTextToStream(ClientStream, "<|" + Environment.MachineName + "|" + GetFilesFolders(key.Substring(2, key.Length - 4)) + "|>");
                }
                else if (key.Contains("<|"))
                {
                    bool b = false;
                    if (key.Split('|')[2] == "")
                        MessageBox.Show("Sorry this folder can't be accessed");
                    else
                    {
                        foreach (Form frm in Application.OpenForms)
                        {
                            if (frm is RemoteFiles)
                            {
                                if (frm.Tag == sIncom)
                                {
                                    ((RemoteFiles)frm).LoadFiles(key.Split('|')[2]);
                                    b = true;
                                }
                            }
                        }
                        if (b == false)
                        {
                            RemoteFiles frm = new RemoteFiles();
                            frm.Show();
                            frm.LoadThis(sIncom, key.Split('|')[1]);
                            ((RemoteFiles)frm).LoadFiles(key.Split('|')[2]);
                        }
                    }
                }
                else if (key.Contains("<-"))
                {
                    bool bPerm = (bool)KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0]["Delete"];
                    if (bPerm)
                    {
                        string type = key.Split('-')[1];
                        if (type == "D")
                            Directory.Delete(key.Split('-')[2]);
                        else if (type == "F")
                            File.Delete(key.Split('-')[2]);


                        string inPath = key.Split('-')[2];

                        string @base = inPath.Substring(0, inPath.LastIndexOf(@"\"));
                        if (!@base.Contains(@"\") & @base != "")
                            @base += @"\";
                        KGlobal.WriteTextToStream(ClientStream, "<|" + Environment.MachineName + "|" + GetFilesFolders(@base) + "|>");
                    }
                    else
                        KGlobal.WriteTextToStream(ClientStream, "</Denied/>");
                }
                else if (key == "</Denied/>")
                {
                    MessageBox.Show("Access denied");
                    if (bwSender.IsBusy)
                        bwSender.CancelAsync();
                }
                else if (key.Contains("<<"))
                {
                    bool bPerm = (bool)KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0]["CM"];
                    if (bPerm)
                    {
                        string type = key.Split('<')[2];
                        string source = key.Split('<')[3];
                        string dest = key.Split('<')[4] + "\\" + source.Split('\\').Last();
                        if (type == "F")
                            File.Move(source, dest);
                        else if (type == "D")
                            Directory.Move(source, dest);
                        KGlobal.WriteTextToStream(ClientStream, "<|" + Environment.MachineName + "|" + GetFilesFolders(key.Split('<')[4]) + "|>");
                    }
                    else
                        KGlobal.WriteTextToStream(ClientStream, "</Denied/>");
                }
                else if (key.Contains("<+"))
                {
                    bool bPerm = (bool)KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0]["CM"];
                    if (bPerm)
                    {
                        string type = key.Split('+')[1];
                        string source = key.Split('+')[2];

                        string dest = key.Split('+')[3] + @"\" + source.Split('\\').Last();
                        if (type == "F")
                            File.Copy(source, dest);
                        else if (type == "D")

                            KGlobal.CopyDirectory(source, dest);
                        KGlobal.WriteTextToStream(ClientStream, "<|" + Environment.MachineName + "|" + GetFilesFolders(key.Split('+')[3]) + "|>");
                    }
                    else
                        KGlobal.WriteTextToStream(ClientStream, "</Denied/>");
                }
                else if (!key.Contains("<^") & !key.Contains("<$") & !key.Contains("<[") & !key.Contains("<%") & !key.Contains(">??") & !key.Contains("<??") & !key.Contains("</") & !key.Contains("<*>") & !key.Contains("/>") & !key.Contains("::") & key != null & key != "")
                    SendKeys.Send(key);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private ListBox hostlist = new ListBox();
        private void tmrControl_Tick(object sender, EventArgs e)
        {
            TcpClient Client;
            if (KGlobal.ListenerString.Pending() == true)
            {
                Client = KGlobal.ListenerString.AcceptTcpClient();
                byte[] databyte = new byte[1025];
                NetworkStream cstream = Client.GetStream();
                int i = cstream.Read(databyte, 0, databyte.Length);
                string KEY = System.Text.Encoding.ASCII.GetString(databyte, 0, i);
                string sIncom = Client.Client.RemoteEndPoint.ToString().Replace(" ", "");

                // RemoteResponce(KEY, sIncom, cstream)
                if (KEY.Contains("</start/>"))
                {
                    if (!KGlobal.TRConatins(sIncom))
                    {
                        RemoteClient rc = new RemoteClient(sIncom, Client);
                        KGlobal.RemoteClients.Add(rc);
                    }
                    if (KEY.Contains("</Remote/>"))
                    {
                        if (MessageBox.Show(this, "Request from " + sIncom + " for remote control,you want to accept ? ", "Remote Desktop Request", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            KGlobal.WriteTextToStream(cstream, "</ORW/>");
                    }
                    if (KEY.Contains("<__>"))
                    {
                        if (!KGlobal.access.IsDisposed)
                            KGlobal.access = new frmAccess();
                        string sResult = KGlobal.access.ShowMessege(sIncom, "Request from " + sIncom + " for FileExplorer access,You want to accept ?");
                        if (sResult != null)
                        {
                            bool bdelete = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[1])); ;
                            bool bCM = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[2])); ;
                            bool bget = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[3])); ;
                            bool bgive = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[4])); ;
                            if (KGlobal.tblAccesss.Select("IP = '" + sIncom + "'").Count() > 0)
                            {
                                DataRow dr = KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0];
                                dr["Delete"] = bdelete;
                                dr["CM"] = bCM;
                                dr["Get"] = bget;
                                dr["Give"] = bgive;
                            }
                            else
                                KGlobal.tblAccesss.Rows.Add(sResult.Split(KGlobal.smallsep)[0], bdelete, bCM, bget, bgive);
                            KGlobal.WriteTextToStream(cstream, "<|" + Environment.MachineName + "|" + getDrives() + "|>");
                        }
                    }
                }
                else
                    Client.Close();
            }

            try
            {
                if (KGlobal.ListenerFile.Pending())
                {
                    if (!bwReceiver.IsBusy)
                        bwReceiver.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
        private bool bFound = true;
        private string GetFilesFolders(string spath)
        {
            string FilesFolders;
            try
            {
                string files = "";
                string folders = "";
                foreach (string st in Directory.GetFiles(spath))
                    files += st + KGlobal.smallsep;
                foreach (string st in Directory.GetDirectories(spath))
                    folders += st + KGlobal.smallsep;
                FilesFolders = "" + KGlobal.mainsep + "" + KGlobal.mainsep + "" + KGlobal.mainsep + folders + KGlobal.mainsep + files;
            }
            catch (Exception ex)
            {
                FilesFolders = "";
            }
            return FilesFolders;
        }

        private string getDrives()
        {
            string drive = "";
            string cd = "";
            string remov = "";
            foreach (DriveInfo di in DriveInfo.GetDrives())
            {
                if (di.DriveType == DriveType.CDRom)
                    cd += di.Name + KGlobal.smallsep;
                else if (di.DriveType == DriveType.Removable)
                    remov += di.Name + KGlobal.smallsep;
                else if (di.DriveType == DriveType.Fixed)
                    drive += di.Name + KGlobal.smallsep;
            }
            return drive + KGlobal.mainsep + cd + KGlobal.mainsep + remov + KGlobal.mainsep + "" + KGlobal.mainsep + "";
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            TcpClient Client;
            if (KGlobal.Listener.Pending() == true)
            {
                try
                {
                    frmRemote frmR = null;
                    Client = KGlobal.Listener.AcceptTcpClient();
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.Tag == Client.Client.RemoteEndPoint.ToString().Replace(" ", "").Split(':')[0])
                        {
                            frmR = (frmRemote)frm;
                            bFound = true;
                        }
                    }

                    if (bFound)
                    {
                        frmR.setSize();
                        frmR.PicDisplay.Image = Image.FromStream(Client.GetStream());
                        frmR.MinimumSize = new Size(frmR.PicDisplay.Width + 16, frmR.PicDisplay.Height + 39);
                        frmR.TextBox1.Text = Client.Client.RemoteEndPoint.ToString();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        public void fileReceiving(int NoOfPackets, string sincome, string mode)
        {
            frmFileStatus fileProg = new frmFileStatus();
            fileProg.Name = "frm" + sincome.Replace(".", "a") + mode;
            fileProg.Tag = sincome;
            fileProg.MaxProgress = NoOfPackets;
            fileProg.sMode = mode;
            fileProg.Progress = 0;
            if (mode == "R")
                fileProg.DisplayText = "Copying File From :/n" + sincome + " : " + SourceFile + "/n/nTo : /n Local PC : " + savefile;
            else if (mode == "S")
                fileProg.DisplayText = "Copying File From :/n" + "Local PC : " + sfile + "/n/n To :/n" + sincome + " : " + DestiFile;

            // Panel2.Enabled = False
            // pTransfer.Visible = True
            fileProg.Show();
        }
        private void Connect_FormClosing(object sender, FormClosingEventArgs e)
        {
            KGlobal.ListenerString.Stop();
            KGlobal.Listener.Stop();
            KGlobal.kdockmain.Label15.Text = " " + Environment.OSVersion.ToString() + " ";
        }
        private string sfile;
        private string sbincom;
        public FileStream Fs1;
        public string SourceFile = "";
        public string DestiFile = "";
        private string STMode = "R";
        private void bwSender_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            TcpClient Client;
            try
            {

                // Client = New TcpClient(sbincom, portFile)
                // If Client.Connected Then
                // Dim br As New BinaryWriter(Client.GetStream())
                // Dim Fs As New FileStream(sfile, FileMode.Open, FileAccess.Read)
                // Dim bt(Fs.Length - 1) As Byte
                // Fs.Read(bt, 0, Fs.Length)
                // br.Write(bt)
                // br.Flush()
                // br.Close()
                // End If   
                byte[] SendingBuffer = null;
                TcpClient client1 = null;
                TcpClient clientString = null;
                // lblStatus.Text = ""
                NetworkStream netstream = null;
                try
                {
                    Fs1 = new FileStream(sfile, FileMode.Open, FileAccess.Read);
                    int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs1.Length) / Convert.ToDouble(BufferSize)));
                    // progressBar1.Maximum = NoOfPackets
                    if (STMode == "S")
                        BeginInvoke((MethodInvoker)delegate { fileReceiving(NoOfPackets, sbincom, STMode); });
                    else if (STMode == "R")
                    {
                        clientString = new TcpClient(sbincom, KGlobal.PortString);
                        if (clientString.Connected)
                        {
                            StreamWriter sw = new StreamWriter(clientString.GetStream());
                            sw.Write("<(" + NoOfPackets + "(" + STMode + "(>");
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    client1 = new TcpClient(sbincom, KGlobal.portFile);
                    // lblStatus.Text = "Connected to the Server..." & vbLf
                    netstream = client1.GetStream();
                    int TotalLength = System.Convert.ToInt32(Fs1.Length);
                    int CurrentPacketLength;
                    int counter = 0;
                    for (int i = 0; i <= NoOfPackets - 1; i++)
                    {
                        if (bwSender.CancellationPending == false)
                        {
                            if (TotalLength > BufferSize)
                            {
                                CurrentPacketLength = BufferSize;
                                TotalLength = TotalLength - CurrentPacketLength;
                            }
                            else
                                CurrentPacketLength = TotalLength;
                            SendingBuffer = new byte[CurrentPacketLength - 1 + 1];
                            Fs1.Read(SendingBuffer, 0, CurrentPacketLength);
                            netstream.Write(SendingBuffer, 0, System.Convert.ToInt32(SendingBuffer.Length));


                            // If progressBar1.Value >= progressBar1.Maximum Then
                            // progressBar1.Value = progressBar1.Minimum
                            // End If
                            // progressBar1.PerformStep()
                            try
                            {
                                if (((frmFileStatus)Application.OpenForms["frm" + sbincom.Replace(".", "a") + "S"]).Progress < ((frmFileStatus)Application.OpenForms["frm" + sbincom.Replace(".", "a") + "S"]).MaxProgress)
                                {
                                    BeginInvoke((MethodInvoker)delegate { ((frmFileStatus)Application.OpenForms["frm" + sbincom.Replace(".", "a") + "S"]).Progress += 1; });
                                }


                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        else
                            i = NoOfPackets - 1;
                    }

                    // lblStatus.Text = lblStatus.Text + "Sent " + Fs.Length.ToString() + " bytes to the server"
                    Fs1.Close();
                    try
                    {
                        BeginInvoke((MethodInvoker)delegate { Application.OpenForms["frm" + sbincom.Replace(".", "a") + "S"].Close(); });
                    }
                    catch (Exception ex)
                    {
                    }
                    try
                    {
                        BeginInvoke((MethodInvoker)delegate { ((RemoteFiles)Application.OpenForms["frm" + sbincom]).RefreshDirectory(); });
                    }
                    catch (Exception ex)
                    {
                    }
                    clientString = new TcpClient(sbincom, KGlobal.PortString);
                    if (clientString.Connected)
                    {
                        StreamWriter sw = new StreamWriter(clientString.GetStream());
                        sw.Write("<))>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    BeginInvoke((MethodInvoker)delegate { Application.OpenForms["frm" + sbincom.Replace(".", "a") + "S"].Close(); });
                }
                finally
                {
                    netstream.Close();
                }
            }
            catch (Exception ex)
            {
                Client = new TcpClient(sbincom, KGlobal.PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("</Failed/>");
                    sw.Flush();
                    sw.Close();
                }
            }
        }


        public void SendTCP(string M, string IPA, Int32 PortN)
        {
        }
        private const int BufferSize = 1024;
        public string savefile = "";
        private byte[] RecData = new byte[1024];
        private int RecBytes;
        private NetworkStream netstream = null;
        public FileStream Fs;

        private void bwReceiver_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            TcpClient Client;
            try
            {
                // Invoke(Sub() timerSpeed.Start())
                netstream = null;
                Client = KGlobal.ListenerFile.AcceptTcpClient();
                string sIncom = Client.Client.RemoteEndPoint.ToString().Replace(" ", "").Split(':')[0];
                netstream = Client.GetStream();

                int totalrecbytes = 0;
                Fs = new FileStream(savefile, FileMode.OpenOrCreate, FileAccess.Write);
                while ((InlineAssignHelper(ref RecBytes, netstream.Read(RecData, 0, RecData.Length))) > 0 & bwReceiver.CancellationPending == false)
                {
                    Fs.Write(RecData, 0, RecBytes);
                    totalrecbytes += RecBytes;
                    try
                    {
                        if (((frmFileStatus)Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"]).Progress < ((frmFileStatus)Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"]).MaxProgress)
                        {
                            BeginInvoke((MethodInvoker)delegate { ((frmFileStatus)Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"]).Progress += 1; });
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                Fs.Close();
                netstream.Close();
                // client.Close()
                // Invoke(Sub() timerSpeed.Stop())
                try
                {
                    // Dim mode As String
                    // Invoke(Sub() mode = CType(Application.OpenForms("frm" & sIncom.Replace(".", "a") & "R"), frmFileStatus).sMode)
                    // If mode = "R" Then
                    BeginInvoke((MethodInvoker)delegate { Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"].Close(); });
                }
                // End If
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }

        private void tmrNetworkListener_Tick(object sender, EventArgs e)
        {
            TcpClient Client;
            if (KGlobal.NetworkListener.Pending() == true)
            {
                Client = KGlobal.NetworkListener.AcceptTcpClient();
                byte[] databyte = new byte[1025];
                NetworkStream cstream = Client.GetStream();
                string sIncom = Client.Client.RemoteEndPoint.ToString().Replace(" ", "");
                int i = cstream.Read(databyte, 0, databyte.Length);
                NetworkPackage np = new NetworkPackage(ref databyte);
                if (np.DataInstruction.ToString().Contains("start"))
                {
                    if (!KGlobal.TRConatins(sIncom))
                    {
                        NetworkCommunication rc = new NetworkCommunication(sIncom, Client);
                        //KGlobal.RemoteClients.Add(rc);
                    }
                    if (np.DataInstruction.ToString().Contains("</Remote/>"))
                    {
                        if (MessageBox.Show(this, "Request from " + sIncom + " for remote control,you want to accept ? ", "Remote Desktop Request", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            KGlobal.WriteTextToStream(cstream, "</ORW/>");
                    }
                    if (np.DataInstruction.ToString().Contains("<__>"))
                    {
                        if (!KGlobal.access.IsDisposed)
                            KGlobal.access = new frmAccess();
                        string sResult = KGlobal.access.ShowMessege(sIncom, "Request from " + sIncom + " for FileExplorer access,You want to accept ?");
                        if (sResult != null)
                        {
                            bool bdelete = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[1])); ;
                            bool bCM = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[2])); ;
                            bool bget = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[3])); ;
                            bool bgive = Convert.ToBoolean(int.Parse(sResult.Split(KGlobal.smallsep)[4])); ;
                            if (KGlobal.tblAccesss.Select("IP = '" + sIncom + "'").Count() > 0)
                            {
                                DataRow dr = KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0];
                                dr["Delete"] = bdelete;
                                dr["CM"] = bCM;
                                dr["Get"] = bget;
                                dr["Give"] = bgive;
                            }
                            else
                                KGlobal.tblAccesss.Rows.Add(sResult.Split(KGlobal.smallsep)[0], bdelete, bCM, bget, bgive);
                            KGlobal.WriteTextToStream(cstream, "<|" + Environment.MachineName + "|" + getDrives() + "|>");
                        }
                    }
                }
                else
                    Client.Close();
            }
        }
    }
}
