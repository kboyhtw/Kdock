using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public class NetworkPackage
    {
        public string Type = "B";
        public object DataInstruction = "";
        public object Data;
        object DataLength = 0;
        object DataInstructionLength = 0;

        int TypeLeft = 0;
        int DataLengthLeft = 0;
        int DataInstructionLengthLeft = 0;
        int DataInstructionLeft = 0;
        int DataLeft = 0;



        public bool IsDataCompleted = false;

        public NetworkPackage(string _DataInstruction, object _Data)
        {
            DataInstruction = _DataInstruction;
            Data = _Data;
            if (Data.GetType() == typeof(byte[]))
            {
                Type = "B";
            }
            else if (Data.GetType() == typeof(Bitmap) || Data.GetType() == typeof(Image))
            {
                Type = "I";
            }
            else if (Data.GetType() == typeof(String))
            {
                Type = "S";
            }
        }
        public NetworkPackage()
        { }

        int Steps = 0;
        public NetworkPackage(ref byte[] ByteData)
        {
            DateTime dt = DateTime.Now;
            DataInstructionLength = 0;
            DataLength = 0;

            ByteData = AddBytes(ByteData);
            //try
            //{
            //    int startIndex = 0;
            //    byte[] bttype = new byte[1];
            //    Array.Copy(ByteData, startIndex, bttype, 0, 1);

            //    startIndex += 1;

            //    Type = System.Text.ASCIIEncoding.Default.GetString(bttype);

            //    if (ByteData.Length >= startIndex + 8)
            //    {
            //        byte[] btdatainslen = new byte[8];
            //        Array.Copy(ByteData, startIndex, btdatainslen, 0, 8);
            //        DataInstructionLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString(btdatainslen));
            //    }
            //    else
            //    {
            //        DataInstructionLengthLeft = (startIndex + 8) - ByteData.Length;
            //        int NewLen = ByteData.Length - startIndex;
            //        byte[] btdatainslen = new byte[NewLen];
            //        Array.Copy(ByteData, startIndex, btdatainslen, 0, NewLen);
            //        DataInstructionLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString(btdatainslen));
            //    }


            //    startIndex += 8;

            //    if (ByteData.Length >= startIndex + 8)
            //    {
            //        byte[] btdatalen = new byte[8];
            //        Array.Copy(ByteData, startIndex, btdatalen, 0, 8);
            //        DataLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString(btdatalen));
            //    }
            //    else
            //    {
            //        DataLengthLeft = (startIndex + 8) - ByteData.Length;
            //        int NewLen = ByteData.Length - startIndex;
            //        byte[] btdatalen = new byte[NewLen];
            //        Array.Copy(ByteData, startIndex, btdatalen, 0, NewLen);
            //        DataLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString(btdatalen));
            //    }

            //    startIndex += 8;

            //    if (ByteData.Length >= startIndex + DataInstructionLength)
            //    {
            //        byte[] btdatains = new byte[DataInstructionLength];
            //        Array.Copy(ByteData, startIndex, btdatains, 0, DataInstructionLength);
            //        DataInstruction = System.Text.ASCIIEncoding.Default.GetString(btdatains);
            //    }
            //    else
            //    {
            //        DataInstructionLeft = (startIndex + DataInstructionLength) - ByteData.Length;
            //        int NewLen = ByteData.Length - startIndex;
            //        byte[] btdatains = new byte[NewLen];
            //        Array.Copy(ByteData, startIndex, btdatains, 0, NewLen);
            //        DataInstruction = System.Text.ASCIIEncoding.Default.GetString(btdatains);
            //    }

            //    startIndex += DataInstructionLength;

            //    if (ByteData.Length >= startIndex + DataLength)
            //    {

            //        byte[] btdata = new byte[DataLength];
            //        Array.Copy(ByteData, startIndex, btdata, 0, DataLength);
            //        if (Type == "I")
            //        {
            //            Image img = Image.FromStream(new MemoryStream(btdata));
            //            Data = img;
            //        }
            //        else if (Type == "S")
            //        {
            //            Data = System.Text.ASCIIEncoding.Default.GetString(btdata);
            //        }
            //        else
            //        {
            //            Data = btdata;
            //        }
            //    }
            //    else
            //    {
            //        DataLeft = (startIndex + DataLength) - ByteData.Length;
            //        int NewLen = ByteData.Length - startIndex;
            //        byte[] btdata = new byte[NewLen];
            //        Array.Copy(ByteData, startIndex, btdata, 0, NewLen);
            //        Data = btdata;
            //    }


            //    startIndex += DataLength;
            //    int Extralength = ByteData.Length - startIndex;
            //    byte[] extra = new byte[Extralength];
            //    Array.Copy(ByteData, startIndex, extra, 0, Extralength);
            //    ByteData = extra;
            //}
            //catch (Exception ex)
            //{


            //}

            //try
            //{
            //    // Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            //    byte[] DatalenByte = new byte[17];
            //    Buffer.BlockCopy(ByteData, 0, DatalenByte, 0, 17);
            //    Steps = 1;
            //    int startIndex = 0;
            //    byte[] bttype = new byte[1];
            //    Buffer.BlockCopy(DatalenByte, startIndex, bttype, 0, 1);
            //    startIndex += 1;
            //    Steps = 2;
            //    Type = System.Text.ASCIIEncoding.Default.GetString(bttype);

            //    byte[] btdatainslen = new byte[8];
            //    Buffer.BlockCopy(DatalenByte, startIndex, btdatainslen, 0, 8);
            //    DataInstructionLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString(btdatainslen));
            //    Steps = 3;

            //    startIndex += 8;

            //    byte[] btdatalen = new byte[8];
            //    Buffer.BlockCopy(DatalenByte, startIndex, btdatalen, 0, 8);
            //    DataLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString(btdatalen));

            //    Steps = 4;


            //    byte[] DataByte = new byte[DataInstructionLength + DataLength];
            //    Buffer.BlockCopy(ByteData, 17, DataByte, 0, DataInstructionLength + DataLength);
            //    Steps = 5;
            //    startIndex = 0;
            //    byte[] btdatains = new byte[DataInstructionLength];
            //    Buffer.BlockCopy(DataByte, 0, btdatains, 0, DataInstructionLength);
            //    DataInstruction = System.Text.ASCIIEncoding.Default.GetString(btdatains);

            //    startIndex += DataInstructionLength;
            //    Steps = 6;
            //    byte[] btdata = new byte[DataLength];
            //    Buffer.BlockCopy(DataByte, startIndex, btdata, 0, DataLength);

            //    if (Type == "I")
            //    {
            //        Image img = Image.FromStream(new MemoryStream(btdata));
            //        Data = img;
            //    }
            //    else if (Type == "S")
            //    {
            //        Data = System.Text.ASCIIEncoding.Default.GetString(btdata);
            //    }
            //    else
            //    {
            //        Data = btdata;
            //    }
            //    Steps = 7;
            //    startIndex += DataLength + 17;
            //    int Extralength = ByteData.Length - startIndex;
            //    byte[] extra = new byte[Extralength];
            //    Buffer.BlockCopy(ByteData, startIndex, extra, 0, Extralength);
            //    ByteData = extra;
            //    Steps = 8;
            //}
            //catch (Exception ex)
            //{


            //}

        }

        public byte[] AddBytes(byte[] ByteData)
        {
            DateTime dt = DateTime.Now;

            try
            {

                if (ByteData.Length == 0)
                {
                    return new byte[0];
                }
                int startIndex = 0;

                if (TypeLeft != -1)
                {

                    byte[] bttype = new byte[1];
                    Array.Copy(ByteData, startIndex, bttype, 0, 1);
                    Type = System.Text.ASCIIEncoding.Default.GetString(bttype);
                    if (Type != "B" & Type != "I" & Type != "S")
                    {

                    }
                    TypeLeft = -1;
                    startIndex += 1;

                }

                if (DataInstructionLengthLeft != -1)
                {
                    startIndex = AssignBytes(ByteData, startIndex, 8, ref DataInstructionLength, ref DataInstructionLengthLeft);
                    if (((byte[])DataInstructionLength).Length == 8)
                    {
                        DataInstructionLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString((byte[])DataInstructionLength));
                    }
                    //else
                    //{
                    //    return new byte[0];
                    //}
                }


                if (DataLengthLeft != -1)
                {
                    startIndex = AssignBytes(ByteData, startIndex, 8, ref DataLength, ref DataLengthLeft);
                    if (((byte[])DataLength).Length == 8)
                    {
                        DataLength = int.Parse(System.Text.ASCIIEncoding.Default.GetString((byte[])DataLength));
                    }
                    //else
                    //{
                    //    return new byte[0];
                    //}
                }

                if (DataInstructionLeft != -1 & DataInstructionLength != null & DataInstructionLength is int)
                {
                    startIndex = AssignBytes(ByteData, startIndex, (int)DataInstructionLength, ref DataInstruction, ref DataInstructionLeft);
                    if (((byte[])DataInstruction).Length == (int)DataInstructionLength)
                    {
                        DataInstruction = System.Text.ASCIIEncoding.Default.GetString((byte[])DataInstruction);
                    }
                    //else
                    //{
                    //    return new byte[0];
                    //}

                }

                if (DataLeft != -1 & DataLength != null & DataLength is int)
                {
                    startIndex = AssignBytes(ByteData, startIndex, (int)DataLength, ref Data, ref DataLeft);
                    if (((byte[])Data).Length == (int)DataLength)
                    {
                        if (Type == "I")
                        {
                            Image img = Image.FromStream(new MemoryStream((byte[])Data));
                            Data = img;
                        }
                        else if (Type == "S")
                        {
                            Data = System.Text.ASCIIEncoding.Default.GetString((byte[])Data);
                        }
                    }
                    //else
                    //{
                    //    return new byte[0];
                    //}
                }

                if (DataLeft == -1 & DataInstructionLeft == -1)
                {
                    IsDataCompleted = true;
                }

                int Extralength = ByteData.Length - startIndex;
                byte[] extra = new byte[Extralength];
                Array.Copy(ByteData, startIndex, extra, 0, Extralength);
                ByteData = null;
                File.AppendAllText("timestamp.set", (DateTime.Now - dt).ToString() + "\r\n");
                return extra;
            }
            catch (Exception ex)
            {
                File.AppendAllText("timestamp.set", (DateTime.Now - dt).ToString() + "\r\n");
                return new byte[0];
            }

        }



        private int AssignBytes(byte[] ByteData, int StartIndex, int DataLength, ref object DataOf, ref int LeftLength)
        {
            if (LeftLength > 0)
            {
                if (ByteData.Length >= StartIndex + LeftLength)
                {
                    byte[] btdatainslen = new byte[LeftLength];
                    Array.Copy(ByteData, StartIndex, btdatainslen, 0, LeftLength);
                    DataOf = KGlobal.Combine((byte[])DataOf, btdatainslen);
                    StartIndex = StartIndex + LeftLength;
                    LeftLength = -1;
                    return StartIndex;
                }
                else
                {
                    LeftLength = (StartIndex + LeftLength) - ByteData.Length;
                    int NewLen = ByteData.Length - StartIndex;
                    byte[] btdata = new byte[NewLen];
                    Array.Copy(ByteData, StartIndex, btdata, 0, NewLen);
                    DataOf = KGlobal.Combine((byte[])DataOf, btdata);
                    return StartIndex + NewLen;
                }
            }
            else
            {
                if (ByteData.Length >= StartIndex + DataLength)
                {
                    byte[] btdatainslen = new byte[DataLength];
                    Array.Copy(ByteData, StartIndex, btdatainslen, 0, DataLength);
                    DataOf = btdatainslen;
                    StartIndex = StartIndex + DataLength;
                    LeftLength = -1;
                    return StartIndex;
                }
                else
                {
                    LeftLength = (StartIndex + DataLength) - ByteData.Length;
                    int NewLen = ByteData.Length - StartIndex;
                    byte[] btdata = new byte[NewLen];
                    Array.Copy(ByteData, StartIndex, btdata, 0, NewLen);
                    DataOf = btdata;
                    return StartIndex + NewLen;
                }
            }
        }

        public byte[] ToByte()
        {
            byte[] btinst = System.Text.ASCIIEncoding.Default.GetBytes(DataInstruction.ToString());
            byte[] btData = new byte[1];
            if (Data.GetType() == typeof(byte[]))
            {
                Type = "B";
                btData = (byte[])Data;
            }
            else if (Data.GetType() == typeof(Bitmap) || Data.GetType() == typeof(Image))
            {
                Type = "I";
                ImageCodecInfo jpgEncoder = KGlobal.GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, KGlobal.RmQuality);
                myEncoderParameters.Param[0] = myEncoderParameter;
                MemoryStream ms = new MemoryStream();
                ((Bitmap)Data).Save(ms, jpgEncoder, myEncoderParameters);
                btData = ms.GetBuffer();
            }
            else if (Data.GetType() == typeof(String))
            {
                Type = "S";
                btData = System.Text.ASCIIEncoding.Default.GetBytes((string)Data);
            }
            string Info = Type + btinst.Length.ToString("00000000") + btData.Length.ToString("00000000");
            byte[] btinfo = System.Text.ASCIIEncoding.Default.GetBytes(Info);
            byte[] result = KGlobal.Combine(btinfo, btinst, btData);
            //byte[] result = btinfo.Concat(btinst).Concat(btData).ToArray();
            return result;

        }
    }
}

