using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Kdock
{
    public class RemoteClient
    {
        private TcpClient Rclient;
        private NetworkStream CStream;
        private Thread TClient;
        public string name;
        public string ClientAddress;
        public Image RmImage;

        public bool SendFrames = false;
        public bool RecieveFrames = false;
        public bool SendFile = false;
        public bool RecieveFile = false;

        private string sfile;
        private string sbincom;
        public FileStream Fs1;
        public string SourceFile = "";
        public string DestiFile = "";
        private string STMode = "R";

        private const int BufferSize = 1024;
        public string savefile = "";
        private byte[] RecData = new byte[1024];
        private int RecBytes;
        private NetworkStream netstream = null/* TODO Change to default(_) if this is not a reference type */;

        private Size imgSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        int ih;
        int iw;
        Point ImageLocation;
        public RemoteClient(string sname, TcpClient client)
        {
            name = sname;
            Rclient = client;
            ClientAddress = client.Client.RemoteEndPoint.ToString().Replace(" ", "");
            if (Rclient.Connected)
            {
                CStream = Rclient.GetStream();
                TClient = new Thread(new ThreadStart(RemoteFrameSender));
                TClient.Start();
            }
            ih = Screen.PrimaryScreen.Bounds.Height;
            iw = Screen.PrimaryScreen.Bounds.Width;
            imgSize = new Size(iw, ih);
            ImageLocation = new Point(0, 0);
        }

        private void RemoteFrameSender()
        {
            bool b = true;
            while (b)
            {
                try
                {
                    if (CStream.DataAvailable)
                    {
                        byte[] databyte = new byte[1048577];

                        int bytes;
                        bytes = CStream.Read(databyte, 0, databyte.Length);
                        Array.Resize(ref databyte, bytes);
                        // stream.Flush()
                        byte[] bttype = new byte[1];
                        Array.Copy(databyte, 0, bttype, 0, 1);
                        string rtype = System.Text.ASCIIEncoding.Default.GetString(bttype);
                        if (rtype == "R")
                        {

                            // frmRemote frmR;
                            try
                            {
                                foreach (Form frm in Application.OpenForms)
                                {
                                    //string stag = "";
                                    //if (frm.Tag != null)
                                    //{
                                    //    stag = frm.Tag.ToString();
                                    //}
                                    if (frm.Name == "R" + name)
                                    {
                                        if (((frmRemote)frm).RClient == null)
                                        {
                                            ((frmRemote)frm).RClient = this;
                                            ((frmRemote)frm).ClientStream = CStream;
                                        }
                                        bFound = true;
                                    }
                                }


                                // CStream.ReadTimeout = 200
                                if (bFound)
                                {


                                    //byte[] bt = new byte[25];
                                    int imglen = databyte.Length - 25;
                                    byte[] btimg = new byte[imglen];
                                    //Array.Copy(databyte, 0, bt, 0, 25);
                                    Array.Copy(databyte, 25, btimg, 0, imglen);

                                    //string type = System.Text.ASCIIEncoding.Default.GetString(bt);
                                    //int screenWidth = int.Parse(type.Substring(1, 4));
                                    //int screeenHeight = int.Parse(type.Substring(5, 4));
                                    //int locationX = int.Parse(type.Substring(17, 4));
                                    //int locationY = int.Parse(type.Substring(21, 4));
                                    //int imgWidth = int.Parse(type.Substring(9, 4));
                                    //int imgHeight = int.Parse(type.Substring(13, 4));
                                    Image img = Image.FromStream(new MemoryStream(btimg));

                                    RmImage = img;
                                    //if (RmImage == null)
                                    //{
                                    //    RmImage = new Bitmap(screenWidth, screeenHeight);
                                    //}
                                    //Graphics g = Graphics.FromImage(RmImage);
                                    //g.DrawImage(img, locationX, locationY, imgWidth, imgHeight);
                                }
                            }
                            catch { }
                        }
                        else if (rtype == "<")
                        {
                            string responseData = System.Text.ASCIIEncoding.Default.GetString(databyte, 0, bytes);
                            foreach (string Str in responseData.Split('\r'))
                            {
                                string St = Str.Replace("\n", "\r\n");
                                if (St.Trim() != string.Empty)
                                    RemoteResponce(St, ClientAddress, CStream);
                            }
                        }
                        else
                        {
                            MessageBox.Show("GC");
                        }

                    }

                    if (SendFrames)
                    {
                        BinaryWriter br = new BinaryWriter(CStream);
                        ImageCodecInfo jpgEncoder = KGlobal.GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, KGlobal.RmQuality);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        MemoryStream ms = new MemoryStream();

                        Bitmap img = KGlobal.GetScreenShot(imgSize, ImageLocation);
                        Rectangle srcRect = new Rectangle(ImageLocation.X, ImageLocation.Y, iw, ih);
                        img.Clone(srcRect, img.PixelFormat).Save(ms, jpgEncoder, myEncoderParameters);
                        //img.Save(ms, jpgEncoder, myEncoderParameters);
                        byte[] bt = ms.GetBuffer();
                        string info = "R" + Screen.PrimaryScreen.Bounds.Width.ToString("0000") + Screen.PrimaryScreen.Bounds.Height.ToString("0000") + iw.ToString("0000") + ih.ToString("0000") + ImageLocation.X.ToString("0000") + ImageLocation.Y.ToString("0000");
                        byte[] btinfo = System.Text.ASCIIEncoding.Default.GetBytes(info);
                        byte[] newbt = btinfo.Concat(bt).ToArray();
                        br.Write(newbt);
                        br.Flush();
                        int newx = ImageLocation.X;
                        int newy = ImageLocation.Y;
                        if (ImageLocation.X + iw <= Screen.PrimaryScreen.Bounds.Width && iw < Screen.PrimaryScreen.Bounds.Width)
                        {
                            newx = ImageLocation.X + iw;
                        }
                        else
                        {
                            newx = 0;
                            if (ImageLocation.Y + ih <= Screen.PrimaryScreen.Bounds.Height && ih < Screen.PrimaryScreen.Bounds.Height)
                                newy = ImageLocation.Y + ih;
                            else
                                newy = 0;
                        }
                        ImageLocation = new Point(newx, newy);
                    }

                    if (RecieveFrames)
                    {
                    }


                    if (SendFile)
                    {
                    }

                    if (RecieveFile)
                    {
                    }
                }
                catch (Exception ex)
                {
                    b = false;
                }
                Thread.Sleep(100);
            }
        }

        private void RemoteResponce(string key, string sIncomIP, NetworkStream ClientStream)
        {
            try
            {
                Connect frmconnect = null;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm is Connect)
                    {
                        frmconnect = (Connect)frm;
                        break;
                    }
                }
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
                    if (MessageBox.Show("Request from " + sIncom + " for remote control,you want to accept ? ", "Remote Desktop Request", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (KGlobal.WriteTextToStream(ClientStream, "</ORW/>"))
                        {
                        }
                    }
                }
                else if (key == "</Stop/>")
                    SendFrames = false;
                else if (key.Contains("</Chat/"))
                {
                }
                else if (key == "</ARW/>")
                {
                    if (KGlobal.WriteTextToStream(ClientStream, "<[" + imgSize.Width + "," + imgSize.Height + "]>"))
                        // tmrFramSender.Start()
                        // tmrControl.Start()
                        SendFrames = true;
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
                        // Client = New TcpClient(sIncom, PortString)
                        // If Client.Connected Then
                        // Dim sw As New StreamWriter(Client.GetStream())
                        // sw.Write("<(1(>")
                        // sw.Flush()
                        // sw.Close()
                        // End If

                        if (!SendFile)
                            SendFile = true;
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
                        SendFile = false;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (key == "</STransS/>")
                {
                    try
                    {
                        RecieveFile = false;
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
                        frmconnect.BeginInvoke((MethodInvoker)delegate
                            {
                                frmRemote frmRm = new frmRemote();
                                frmRm.ClientStream = ClientStream;
                                frmRm.Tag = sIncom;
                                frmRm.Name = "R" + sIncom;
                                frmRm.Show();
                            });
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
                        {
                            KGlobal.tblAccesss.Rows.Add(sResult.Split(KGlobal.smallsep)[0], System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[1]), System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[2]), System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[3]), System.Convert.ToBoolean(sResult.Split(KGlobal.smallsep)[4]));

                        }
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
                                if (frm.Tag.ToString()  == sIncom)
                                {
                                    frmconnect.BeginInvoke((MethodInvoker)delegate
                                    {
                                        ((RemoteFiles)frm).LoadFiles(key.Split('|')[2]);
                                    });
                                    b = true;
                                }
                            }
                        }
                        if (b == false)
                        {
                            frmconnect.BeginInvoke((MethodInvoker)delegate
                            {
                                RemoteFiles frm = new RemoteFiles();
                                frm.client = Rclient;
                                frm.Show();
                                frm.LoadThis(sIncom, key.Split('|')[1]);
                                ((RemoteFiles)frm).LoadFiles(key.Split('|')[2]);
                            });
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
                    SendFile = false;
                }

                else if (key.Contains("<<"))
                {

                    bool bPerm = (bool)KGlobal.tblAccesss.Select("IP = '" + sIncom + "'")[0]["CM"];
                    if (bPerm)
                    {
                        string type = key.Split('<')[2];
                        string source = key.Split('<')[3];
                        string dest = key.Split('<')[4] + @"\" + source.Split('\\').Last();
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
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        public void fileReceiving(int NoOfPackets, string sincome, string mode)
        {
            frmFileStatus fileProg = new frmFileStatus();
            fileProg.Name = "frm" + sincome.Replace(".", "a") + mode;
            fileProg.Tag = sincome;
            fileProg.MaxProgress = NoOfPackets;
            fileProg.sMode = mode;
            fileProg.Progress = 0;
            if (mode == "R")
                fileProg.DisplayText = "Copying File From : /n" + sincome + " : " + SourceFile + "/n/nTo : /nLocal PC : " + savefile;
            else if (mode == "S")
                fileProg.DisplayText = "Copying File From : /nLocal PC : " + sfile + "/n/nTo :/n" + sincome + " : " + DestiFile;

            // Panel2.Enabled = False
            // pTransfer.Visible = True
            fileProg.Show();
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

    }
}
