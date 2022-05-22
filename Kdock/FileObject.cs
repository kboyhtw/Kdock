using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Kdock
{
    public class FileObject
    {
        public string FileName = "";
        public int ID = 0;
        public string Directory = "";
        public Int64 TotalLength = 0;
        public byte[] Data = null;
        public bool IsCompleted = false;


        public FileObject(int _ID, string _FileName, string _Directory)
        {
            ID = _ID;
            FileName = _FileName;
            Directory = _Directory;

        }
        FileStream Fs = null;

        public Int64 RecivedData = 0;
        public FileObject(NetworkPackage NP)
        {
            string[] fdata = NP.DataInstruction.ToString ().Split(':');
            ID = int.Parse(fdata[1]);
            FileName = fdata[2];
            TotalLength = Int64.Parse(fdata[3]);
            Data = (byte[])NP.Data;

        }

        public void AppendFile(FileObject _FileObject)
        {
            if (ID == _FileObject.ID)
            {
                try
                {
                    //Data = Data.Concat(_FileObject.Data).ToArray();
                    //Data = KGlobal.Combine(Data, _FileObject.Data);
                    RecivedData += _FileObject.Data.Length;
                    Fs.Write(_FileObject.Data, 0, _FileObject.Data.Length);
                    if (RecivedData >= TotalLength)
                    {
                        Fs.Close();
                        // File.WriteAllBytes(Directory + FileName, Data);
                        IsCompleted = true;
                        TimerSpeed.Stop();
                    }
                }
                catch (Exception ex)
                {

                }

            }
        }
        Timer TimerSpeed;
        public void StartWrite()
        {
            TimerSpeed = new Timer();
            TimerSpeed.Interval = 1000;
            TimerSpeed.Elapsed += TimerSpeed_Tick;
            TimerSpeed.Start();
            Fs = new FileStream(Directory + FileName, FileMode.OpenOrCreate, FileAccess.Write);
            RecivedData = Data.Length;
            Fs.Write(Data, 0, Data.Length);
            if (RecivedData >= TotalLength)
            {
                Fs.Close();
                //File.WriteAllBytes(Directory + FileName, Data);
                IsCompleted = true;
                TimerSpeed.Stop();
            }
        }

        Int64 LastSize = 0;

        public double Speed = 0;
        private void TimerSpeed_Tick(object sender, EventArgs e)
        {
            Speed = RecivedData - LastSize;
            LastSize = RecivedData;
        }
    }
}
