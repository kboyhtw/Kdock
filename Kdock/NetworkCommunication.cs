using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Kdock
{
    class NetworkCommunication
    {
        string name;
        TcpClient Rclient;
        string ClientAddress;
        Thread TClient;
        Thread TProcess;
        NetworkStream CStream;
        public NetworkCommunication(string sname, TcpClient client)
        {
            name = sname;
            Rclient = client;
            ClientAddress = client.Client.RemoteEndPoint.ToString().Replace(" ", "");
            if (Rclient.Connected)
            {
                CStream = Rclient.GetStream();
                //Process();
                TClient = new Thread(new ThreadStart(Process));
                TClient.Start();

                TProcess = new Thread(new ThreadStart(TProcessData));
                TProcess.Start();
            }
            //ih = Screen.PrimaryScreen.Bounds.Height;
            //iw = Screen.PrimaryScreen.Bounds.Width;
            //imgSize = new Size(iw, ih);
            //ImageLocation = new Point(0, 0);
        }

        List<FileObject> RFiles = new List<FileObject>();
        List<NetworkPackage> incomData = new List<NetworkPackage>();
        List<byte[]> IncomBytes = new List<byte[]>();
        int istep = 0;

        int BytesRecieved = 0;

        bool StartProcess = true;
        private void Process()
        {

            while (StartProcess)
            {
                try
                {
                    if (!Rclient.Connected)
                    {
                        StartProcess = false;
                    }

                    if (CStream.DataAvailable)
                    {
                        //istep = 0;
                        //byte[] databyte = GetStream(CStream);
                        //BytesRecieved += databyte.Length;
                        //IncomBytes.Add(databyte);


                        byte[] databyte = new byte[KGlobal.FileBufferSize *2];
                        int bytes = 0;
                        // while ((InlineAssignHelper(ref RecBytes, netstream.Read(RecData, 0, RecData.Length))) > 0 & bStop == false)
                        while ((bytes = CStream.Read(databyte, 0, databyte.Length)) > 0)
                        {
                            Array.Resize(ref databyte, bytes);
                            IncomBytes.Add((byte[])databyte.Clone());
                            databyte = new byte[KGlobal.FileBufferSize *2];
                        }



                        //while (databyte.Length > 0)
                        //{
                        //    int DataLen = databyte.Length;
                        //    NetworkPackage np = new NetworkPackage(ref databyte);
                        //    if (databyte.Length < DataLen)
                        //    {
                        //        incomData.Add(np);
                        //    }
                        //    else
                        //    {
                        //        databyte = GetStream(CStream, databyte);
                        //    }
                        //}
                        //istep = 3;
                        //while (incomData.Count > 0)
                        //{
                        //    ProcessData(incomData[0]);
                        //    incomData.RemoveAt(0);
                        //}
                        //istep = 4;
                        // Thread.Sleep(100);

                    }
                    else
                    {
                        // BytesRecieved = 0;
                        Thread.Sleep(500); //  b = false;
                    }
                }
                catch (Exception ex)
                {
                    StartProcess = false;
                }
            }
        }

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }

        public void Stop()
        {
            StartProcess = false;
            Thread.Sleep(500);
            try
            {
                TClient.Abort();
            }
            catch (Exception)
            {

            }
            try
            {
                TProcess.Abort();
            }
            catch (Exception)
            {

            }
        }


        private byte[] GetStream(NetworkStream CStream, byte[] PreviousData = null)
        {
            byte[] databyte = new byte[999999999];
            int bytes = CStream.Read(databyte, 0, databyte.Length);
            Array.Resize(ref databyte, bytes);
            if (PreviousData != null)
            {
                databyte = KGlobal.Combine(PreviousData, databyte);
            }
            return databyte;
        }

        private List<byte[]> SplitData(byte[] Data)
        {
            List<byte[]> recdatalst = new List<byte[]>();
            int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Data.Length) / Convert.ToDouble(KGlobal.FileBufferSize)));
            int TotalLength = System.Convert.ToInt32(Data.Length);
            int CurrentPacketLength;
            int CurrStartIndex = 0;
            for (int i = 0; i <= NoOfPackets - 1; i++)
            {

                if (TotalLength > KGlobal.FileBufferSize)
                {
                    CurrentPacketLength = KGlobal.FileBufferSize;
                    TotalLength = TotalLength - CurrentPacketLength;
                }
                else
                    CurrentPacketLength = TotalLength;
                byte[] SendingBuffer = new byte[CurrentPacketLength];

                Buffer.BlockCopy(Data, CurrStartIndex, SendingBuffer, 0, CurrentPacketLength);
                CurrStartIndex += CurrentPacketLength;

                recdatalst.Add(SendingBuffer);
            }

            return recdatalst;
        }

        private void TProcessData()
        {

            while (StartProcess)
            {
                if (IncomBytes.Count > 0)
                {
                    NetworkPackage np = new NetworkPackage();
                    while (!np.IsDataCompleted)
                    {
                        if (IncomBytes.Count > 0)
                        {
                            IncomBytes[0] = np.AddBytes(IncomBytes[0]);
                            if (!np.IsDataCompleted)
                            {

                            }
                            if (IncomBytes[0].Length == 0)
                            {
                                IncomBytes.RemoveAt(0);
                            }
                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }
                    ProcessData(np);
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }


        private void ProcessData(NetworkPackage np)
        {

            if (np.Type == "S")
            {
                bool bIsOpen = false;
                frmChat frmC;
                string[] strinst = np.DataInstruction.ToString().Split(':');
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.Name == "C" + ClientAddress.Split(':')[0] | frm.Name == "C" + strinst[1])
                    {
                        bIsOpen = true;
                        frmC = (frmChat)frm;
                        frmC.GotMsg((string)np.Data);
                    }
                }
                if (bIsOpen == false)
                    // frmC = New frmLanChat
                    // frmC.Tag = sIncom
                    // frmC.Name = "C" & sIncom
                    // frmC.Text = sIncom
                    // frmC.Show()
                    File.AppendAllText(KGlobal.path + "InMsg.k", ClientAddress.Split(':')[0] + KGlobal.smallsep + strinst[1] + KGlobal.smallsep + (string)np.Data + KGlobal.mainsep);

            }
            else if (np.Type == "B")
            {
                if (np.DataInstruction.ToString().StartsWith("File:"))
                {
                    FileObject fobj = new FileObject(np);
                    bool Found = false;
                    for (int i = 0; i < RFiles.Count; i++)
                    {
                        FileObject item = RFiles[i];
                        if (item.ID == fobj.ID)
                        {
                            item.AppendFile(fobj);
                            File.WriteAllText("InFile.k", ClientAddress.Split(':')[0] + KGlobal.smallsep + item.RecivedData.ToString() + "/" + item.TotalLength.ToString());
                            Found = true;
                            if (item.IsCompleted)
                            {
                                RFiles.RemoveAt(i);

                            }
                            break;
                        }
                    }
                    if (!Found)
                    {
                        RFiles.Add(fobj);
                        fobj.StartWrite();
                        frmChat frmC;
                        bool bIsOpen = false;
                        foreach (Form frm in Application.OpenForms)
                        {
                            if (frm.Name == "C" + ClientAddress.Split(':')[0])
                            {
                                bIsOpen = true;
                                frmC = (frmChat)frm;
                                frmC.GotMsg(fobj.FileName, fobj);
                            }
                        }

                        if (bIsOpen == false)
                            // frmC = New frmLanChat
                            // frmC.Tag = sIncom
                            // frmC.Name = "C" & sIncom
                            // frmC.Text = sIncom
                            // frmC.Show()
                            File.WriteAllText("InFile.k", ClientAddress.Split(':')[0] + KGlobal.smallsep + fobj.RecivedData.ToString() + "/" + fobj.TotalLength.ToString());

                    }

                }
            }
            else if (np.Type == "I")
            {

            }

        }
    }
}
