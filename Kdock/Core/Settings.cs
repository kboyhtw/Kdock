using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public class Settings
    {
        public Point apppos = new Point(1, 30);
        public string opa = "0.75";
        public string lay = "l";
        public string ap = "";

        public string nattached = "";
        public double nFS = 12.0;
        public Point npoint = new Point(30, 30);
        public string ndata = "";
        public Size nsize = new Size(700, 400);

        public Point wpoint = new Point(30, 30);
        public string wurls = "";
        public Point LPoint = new Point(30, 30);
        public string LPC = "";
        public string LshowPC = "Y";
        public string wshowurls = "Y";
        public string LanEng = "1";
        public char smallsep = (char)240;
        public char mainsep = (char)243;


        public Settings()
        {

        }

        public Settings(string SettingString)
        {
            LoadSettings(SettingString);
        }

        private void LoadSettings(string SettingString)
        {
            try
            {
                string[] settings = SettingString.Split(mainsep);
                apppos = new Point(int.Parse(settings[0].Split(smallsep)[0]), int.Parse(settings[0].Split(smallsep)[1]));
                if (apppos.X > Screen.PrimaryScreen.Bounds.Width - 50 | apppos.Y > Screen.PrimaryScreen.Bounds.Height - 50)
                {
                    apppos.X = 1;
                    apppos.Y = 30;
                }
                opa = settings[0].Split(smallsep)[2];
                lay = settings[0].Split(smallsep)[3];
                ap = settings[0].Split(smallsep)[4];

                npoint = new Point(int.Parse(settings[1].Split(smallsep)[0]), int.Parse(settings[1].Split(smallsep)[1]));
                nsize = new Size(int.Parse(settings[1].Split(smallsep)[2]), int.Parse(settings[1].Split(smallsep)[3]));
                nFS = double.Parse(settings[1].Split(smallsep)[4]);
                nattached = settings[1].Split(smallsep)[5];
                ndata = settings[1].Split(smallsep)[6];

                wpoint = new Point(int.Parse(settings[2].Split(smallsep)[0]), int.Parse(settings[2].Split(smallsep)[1]));
                wshowurls = settings[2].Split(smallsep)[2];
                wurls = settings[2].Split(smallsep)[3];
                LanEng = settings[3];
                LPoint = new Point(int.Parse(settings[4].Split(smallsep)[0]), int.Parse(settings[4].Split(smallsep)[1]));
                LshowPC = settings[4].Split(smallsep)[2];
                LPC = settings[4].Split(smallsep)[3];
                settings = null;
            }
            catch (Exception)
            {

            }
        }

        public static Settings Parse(string SettingString)
        {
            Settings setting = new Settings(SettingString);
            return setting;
        }

        public override string ToString()
        {
            return apppos.X.ToString() + smallsep
                + apppos.Y.ToString() + smallsep
                + opa + smallsep + lay + smallsep
                + ap
                + mainsep
                + npoint.X.ToString() + smallsep
                + npoint.Y.ToString() + smallsep
                + nsize.Width.ToString() + smallsep
                + nsize.Height.ToString() + smallsep
                + nFS.ToString() + smallsep
                + nattached + smallsep
                + ndata
                + mainsep
                + wpoint.X.ToString() + smallsep
                + wpoint.Y.ToString() + smallsep
                + wshowurls + smallsep
                + wurls
                + mainsep
                + LanEng
                + mainsep
                + LPoint.X.ToString() + smallsep
                + LPoint.Y.ToString() + smallsep
                + LshowPC + smallsep
                + LPC;
        }

        public void Load(string path)
        {
            string s = path + @"\settings.k";
            if (File.Exists(s))
            {
                LoadSettings(File.ReadAllText(s));
            }
        }
        public void Save(string path)
        {
            string s = path + @"\settings.k";
            File.Delete(s);
            File.AppendAllText(s, ToString());
            File.SetAttributes(s, System.IO.FileAttributes.Hidden);
            File.Copy(path + @"\settings.k", path + @"\Backsettings.k", true);
        }
    }
}
