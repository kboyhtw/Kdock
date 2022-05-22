using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Kdock
{
    class FileReciever
    {
        public int ID;
        public string Path;
        TcpClient Client;
        public int FileSize;

        bool bStop = false;
        FileStream Fs = null;
        bool IsOpen = false;
        public FileReciever(int _ID, string _Diretory, TcpClient _Client)
        {
            ID = _ID;
            Path = _Diretory;
            Client = _Client;
            Fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write);
            IsOpen = true;
        }


        public void Writedata(NetworkPackage np)
        {
            try
            {
                byte[] data = (byte[])np.Data;
                int len = data.Length;
                Fs.Write(data, 0, len);   
            }
            catch (Exception ex)
            {
            }
        }

        public void Stop()
        {
            try
            {
               Fs.Close();
            }
            catch (Exception ex)
            {
            }
        }


        public void Finish()
        {
            try
            {
                Fs.Close();
            }
            catch (Exception ex)
            {
            }
        }



        public void Recieve(NetworkPackage np)
        {
          
            //try
            //{
            //    // Invoke(Sub() timerSpeed.Start())
            //    NetworkStream netstream = null;
            //    //Client = KGlobal.NetworkListener.AcceptTcpClient();
            //    //string sIncom = Client.Client.RemoteEndPoint.ToString().Replace(" ", "").Split(':')[0];
            //    netstream = Client.GetStream();

            //    int totalrecbytes = 0;
            //    FileStream Fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write);
            //    int RecBytes = 0;
            //    byte[] RecData = new byte[8000];
            //    while ((InlineAssignHelper(ref RecBytes, netstream.Read(RecData, 0, RecData.Length))) > 0 & bStop == false)
            //    {
            //        NetworkPackage np = new NetworkPackage(RecData);
            //        Fs.Write(RecData, 0, RecBytes);
            //        totalrecbytes += RecBytes;
            //        try
            //        {
            //            if (((frmFileStatus)Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"]).Progress < ((frmFileStatus)Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"]).MaxProgress)
            //            {
            //                BeginInvoke((MethodInvoker)delegate { ((frmFileStatus)Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"]).Progress += 1; });
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //        }
            //    }
            //    Fs.Close();
            //    netstream.Close();
            //    // client.Close()
            //    // Invoke(Sub() timerSpeed.Stop())
            //    try
            //    {
            //        // Dim mode As String
            //        // Invoke(Sub() mode = CType(Application.OpenForms("frm" & sIncom.Replace(".", "a") & "R"), frmFileStatus).sMode)
            //        // If mode = "R" Then
            //        BeginInvoke((MethodInvoker)delegate { Application.OpenForms["frm" + sIncom.Replace(".", "a") + "R"].Close(); });
            //    }
            //    // End If
            //    catch (Exception ex)
            //    {
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
        }

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }

    }
}
