using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Kdock
{
    public class Theme
    {

        public Color clr0 = Color.FromArgb(64, 64, 64);
        public Color clr1 = Color.White;
        public Color clr2 = Color.DeepSkyBlue;
        public Color clr3 = Color.Red;
        public Color clr4 = Color.White;
        public Color clr5 = Color.Black;
        public Color clr6 = Color.Gainsboro;
        public Color clr7 = Color.DeepSkyBlue;
        public Color clr8 = Color.Blue;
        public bool Auto = false;
        public Theme()
        {

        }

        public static Theme Parse(string ThemeString)
        {
            Theme thm = new Theme(ThemeString);
            return thm;
        }

        public Theme(string ThemeString)
        {
            //if (File.Exists(KGlobal.path + @"\theme.k"))
            //{

            //try
            //{
            //    str = File.ReadAllText(KGlobal.path + @"\theme.k").Split('_')[0].Split(':');
            //}
            //catch (Exception ex)
            //{
            //    File.Copy(KGlobal.path + @"\Backtheme.k", KGlobal.path + @"\theme.k", true);
            //    str = File.ReadAllText(KGlobal.path + @"\theme.k").Split('_')[0].Split(':');
            //}
            try
            {
                string[] str = new string[9];
                str = ThemeString.Split('_')[0].Split(':');
                string[] tocolor;
                tocolor = str[0].Split(',');
                clr0 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[1].Split(',');
                clr1 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[2].Split(',');
                clr2 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[3].Split(',');
                clr3 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[4].Split(',');
                clr4 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[5].Split(',');
                clr5 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[6].Split(',');
                clr6 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[7].Split(',');
                clr7 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                tocolor = str[8].Split(',');
                clr8 = Color.FromArgb(int.Parse(tocolor[0]), int.Parse(tocolor[1]), int.Parse(tocolor[2]), int.Parse(tocolor[3]));
                Auto = bool.Parse(str[9]);
                tocolor = null;
                str = null;
            }
            catch (Exception ex)
            {
                clr0 = Color.FromArgb(64, 64, 64);
                clr1 = Color.White;
                clr2 = Color.DeepSkyBlue;
                clr3 = Color.Red;
                clr4 = Color.White;
                clr5 = Color.Black;
                clr6 = Color.Gainsboro;
                clr7 = Color.DeepSkyBlue;
                clr8 = Color.Blue;
                Auto = false;
            }

        }

        public override string ToString()
        {
            string sres = clr0.A.ToString() + "," + clr0.R.ToString() + "," + clr0.G.ToString() + "," + clr0.B.ToString() + ":"
                        + clr1.A.ToString() + "," + clr1.R.ToString() + "," + clr1.G.ToString() + "," + clr1.B.ToString() + ":"
                        + clr2.A.ToString() + "," + clr2.R.ToString() + "," + clr2.G.ToString() + "," + clr2.B.ToString() + ":"
                        + clr3.A.ToString() + "," + clr3.R.ToString() + "," + clr3.G.ToString() + "," + clr3.B.ToString() + ":"
                        + clr4.A.ToString() + "," + clr4.R.ToString() + "," + clr4.G.ToString() + "," + clr4.B.ToString() + ":"
                        + clr5.A.ToString() + "," + clr5.R.ToString() + "," + clr5.G.ToString() + "," + clr5.B.ToString() + ":"
                        + clr6.A.ToString() + "," + clr6.R.ToString() + "," + clr6.G.ToString() + "," + clr6.B.ToString() + ":"
                        + clr7.A.ToString() + "," + clr7.R.ToString() + "," + clr7.G.ToString() + "," + clr7.B.ToString() + ":"
                        + clr8.A.ToString() + "," + clr8.R.ToString() + "," + clr8.G.ToString() + "," + clr8.B.ToString() + ":"
                        + Auto.ToString();
            return sres;
        }
    }
}
