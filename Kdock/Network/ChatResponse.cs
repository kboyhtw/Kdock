using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kdock
{
    internal class ChatResponse
    {
        internal NetworkPackage PerformAction(string clientAddress, NetworkPackage received)
        {
            string[] parts = received.Instruction.Split(':');
            string senderName = parts.Length > 1 ? parts[1] : "Unknown";
            string incomingText = received.Data != null ? Encoding.UTF8.GetString(received.Data) : "";

            bool formFound = false;

            foreach (Form frm in Application.OpenForms)
            {
                if (frm is frmChat)
                {
                    string targetName1 = "C" + clientAddress.Split(':')[0];
                    string targetName2 = "C" + senderName;

                    if (frm.Name == targetName1 || frm.Name == targetName2)
                    {
                        ((frmChat)frm).GotMsg(incomingText);
                        formFound = true;
                        break;
                    }
                }
            }

            if (!formFound)
            {
                string line = clientAddress.Split(':')[0] + KGlobal.smallsep + senderName + KGlobal.smallsep + incomingText + KGlobal.mainsep;
                File.AppendAllText(Path.Combine(KGlobal.path, "InMsg.k"), line);
            }

            return null;
        }
    }


    internal class FilesResponse
    {
        internal LanSession Session;

        internal NetworkPackage PerformAction(TcpClient client, string clientAddress, NetworkPackage received)
        {
            string instruction = received.Instruction.ToLower();
            string senderIP = clientAddress.Split(':')[0];
            string payload = received.Data != null ? Encoding.UTF8.GetString(received.Data) : "";

            if (instruction.Contains("<__>"))
            {
                if (KGlobal.access == null || KGlobal.access.IsDisposed)
                    KGlobal.access = new frmAccess();

                string result = KGlobal.access.ShowMessege(senderIP, "Request from " + senderIP + " for File Explorer access. Accept?");
                if (result != null)
                {
                    string[] parts = result.Split(KGlobal.smallsep);
                    bool bDelete = parts[1] == "1";
                    bool bCM = parts[2] == "1";
                    bool bGet = parts[3] == "1";
                    bool bGive = parts[4] == "1";

                    DataRow[] rows = KGlobal.tblAccesss.Select("IP = '" + senderIP + "'");
                    if (rows.Length > 0)
                    {
                        rows[0]["Delete"] = bDelete;
                        rows[0]["CM"] = bCM;
                        rows[0]["Get"] = bGet;
                        rows[0]["Give"] = bGive;
                    }
                    else
                    {
                        KGlobal.tblAccesss.Rows.Add(parts[0], bDelete, bCM, bGet, bGive);
                    }

                    byte[] driveData = Encoding.UTF8.GetBytes(GetDrives());
                    return new NetworkPackage(PackageType.String, "files:drives:" + Environment.MachineName, driveData);
                }
                return null;
            }

            if (instruction.Contains("getdrives"))
            {
                string data = string.IsNullOrEmpty(payload) ? GetDrives() : GetFilesFolders(payload);
                return new NetworkPackage(PackageType.String, "files:drives:" + Environment.MachineName, Encoding.UTF8.GetBytes(data));
            }

            if (instruction.Contains("drives"))
            {
                if (string.IsNullOrEmpty(payload))
                {
                    MessageBox.Show("Sorry, this folder can't be accessed.");
                    return null;
                }

                bool shown = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm is RemoteFiles)
                    {
                        if (frm.Tag != null && frm.Tag.ToString() == senderIP)
                        {
                            RemoteFiles open = (RemoteFiles)frm;
                            if (Session != null) open.Session = Session;
                            open.LoadFiles(payload);
                            shown = true;
                        }
                    }
                }

                if (!shown)
                {
                    RemoteFiles rf = new RemoteFiles();
                    rf.client = client;
                    rf.Session = Session;
                    rf.Show();
                    rf.LoadThis(senderIP, received.Instruction.Split(':')[2]);
                    rf.LoadFiles(payload);
                }

                return null;
            }

            if (instruction.Contains("delete"))
            {
                DataRow[] accessRows = KGlobal.tblAccesss.Select("IP = '" + senderIP + "'");
                bool isAllowed = accessRows.Length > 0 && (bool)accessRows[0]["Delete"];

                if (!isAllowed)
                    return new NetworkPackage(PackageType.String, "files:access:" + Environment.MachineName, Encoding.UTF8.GetBytes("</Denied/>"));

                string type = received.Instruction.Split(':')[1];
                if (type == "deletedirectory" && Directory.Exists(payload))
                    Directory.Delete(payload);
                else if (type == "deletefile" && File.Exists(payload))
                    File.Delete(payload);

                string parent = GetParentPath(payload);
                return new NetworkPackage(PackageType.String, "files:drives:" + Environment.MachineName, Encoding.UTF8.GetBytes(GetFilesFolders(parent)));
            }

            if (instruction.Contains("move") || instruction.Contains("copy"))
            {
                DataRow[] accessRows = KGlobal.tblAccesss.Select("IP = '" + senderIP + "'");
                bool isAllowed = accessRows.Length > 0 && (bool)accessRows[0]["CM"];

                if (!isAllowed)
                    return new NetworkPackage(PackageType.String, "files:access:" + Environment.MachineName, Encoding.UTF8.GetBytes("</Denied/>"));

                string[] parts = received.Instruction.Split(':');
                string type = parts[1];
                string[] paths = payload.Split(KGlobal.mainsep);
                string source = paths[0];
                string destDir = paths.Length > 1 ? paths[1] : "";
                payload = destDir;
                string dest = Path.Combine(payload, Path.GetFileName(source));

                if (type == "movefile" && File.Exists(source)) File.Move(source, dest);
                else if (type == "movedirectory" && Directory.Exists(source)) Directory.Move(source, dest);
                else if (type == "copyfile" && File.Exists(source)) File.Copy(source, dest);
                else if (type == "copydirectory" && Directory.Exists(source)) KGlobal.CopyDirectory(source, dest);

                return new NetworkPackage(PackageType.String, "files:drives:" + Environment.MachineName, Encoding.UTF8.GetBytes(GetFilesFolders(payload)));
            }

            if (instruction.Contains("access"))
            {
                MessageBox.Show("Access denied.");
                return null;
            }

            return null;
        }

        private string GetParentPath(string input)
        {
            try
            {
                string basePath = input.Substring(0, input.LastIndexOf("\\"));
                if (!basePath.EndsWith("\\")) basePath += "\\";
                return basePath;
            }
            catch { return ""; }
        }

        private void LaunchUI(object param)
        {
            RemoteFiles form = param as RemoteFiles;
            if (form != null)
            {
                Application.Run(form);
            }
        }

        private string GetDrives()
        {
            string fixedDrive = "", cdDrive = "", removableDrive = "";

            foreach (DriveInfo di in DriveInfo.GetDrives())
            {
                string name = di.Name + KGlobal.smallsep;
                if (di.DriveType == DriveType.Fixed) fixedDrive += name;
                else if (di.DriveType == DriveType.CDRom) cdDrive += name;
                else if (di.DriveType == DriveType.Removable) removableDrive += name;
            }

            return fixedDrive + KGlobal.mainsep + cdDrive + KGlobal.mainsep + removableDrive + KGlobal.mainsep + "" + KGlobal.mainsep + "";
        }

        private string GetFilesFolders(string path)
        {
            try
            {
                string files = "";
                string folders = "";

                foreach (string f in Directory.GetFiles(path))
                {
                    try
                    {
                        FileInfo info = new FileInfo(f);
                        files += f + "\t" + info.Length + "\t" + info.LastWriteTimeUtc.Ticks + KGlobal.smallsep;
                    }
                    catch
                    {
                        files += f + KGlobal.smallsep;
                    }
                }
                foreach (string d in Directory.GetDirectories(path))
                {
                    try
                    {
                        DirectoryInfo info = new DirectoryInfo(d);
                        folders += d + "\t0\t" + info.LastWriteTimeUtc.Ticks + KGlobal.smallsep;
                    }
                    catch
                    {
                        folders += d + KGlobal.smallsep;
                    }
                }

                return "" + KGlobal.mainsep + "" + KGlobal.mainsep + "" + KGlobal.mainsep + folders + KGlobal.mainsep + files;
            }
            catch { return ""; }
        }
    }

}
