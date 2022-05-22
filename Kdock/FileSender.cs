using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Kdock
{
    class FileSender
    {
        FileObject File = null;
        public TcpClient Client;
        // int BufferSize = 1024 * 100;

        bool bStop = false;
        public Int64 TotalLength = 0;
        public FileSender(FileObject _File, TcpClient _client)
        {
            File = _File;
            Client = _client;
        }
        public void Send()
        {
            try
            {
                byte[] SendingBuffer = null;
                NetworkStream netstream = null;
                try
                {
                    FileStream Fs1 = new FileStream(File.Directory + File.FileName, FileMode.Open, FileAccess.Read);
                    int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs1.Length) / Convert.ToDouble(KGlobal.FileBufferSize)));

                    netstream = Client.GetStream();
                    TotalLength = System.Convert.ToInt64(Fs1.Length);
                    int CurrentPacketLength;

                    for (int i = 0; i <= NoOfPackets - 1; i++)
                    {
                        if (bStop == false)
                        {
                            if (TotalLength > KGlobal.FileBufferSize)
                            {
                                CurrentPacketLength = KGlobal.FileBufferSize;
                                TotalLength = TotalLength - CurrentPacketLength;
                            }
                            else
                                CurrentPacketLength = Convert.ToInt32(TotalLength);
                            SendingBuffer = new byte[CurrentPacketLength];
                            Fs1.Read(SendingBuffer, 0, CurrentPacketLength);
                            NetworkPackage np = new NetworkPackage("File:" + File.ID.ToString() + ":" + File.FileName + ":" + Fs1.Length, SendingBuffer);
                            byte[] package = np.ToByte();
                            netstream.Write(package, 0, System.Convert.ToInt32(package.Length));

                        }
                        else
                            i = NoOfPackets - 1;

                    }


                    Fs1.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //BeginInvoke((MethodInvoker)delegate { Application.OpenForms["frm" + sbincom.Replace(".", "a") + "S"].Close(); });
                }
                finally
                {
                    // netstream.Close();
                }
            }
            catch (Exception ex)
            {
                //Client = new TcpClient(sbincom, KGlobal.PortString);
                //if (Client.Connected)
                //{
                //    StreamWriter sw = new StreamWriter(Client.GetStream());
                //    sw.Write("</Failed/>");
                //    sw.Flush();
                //    sw.Close();
                //}
            }
        }
    }
}
