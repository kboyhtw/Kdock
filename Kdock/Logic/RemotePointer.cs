using System;
using System.Drawing;

namespace Kdock.Logic
{
    public static class RemotePointer
    {
        public static Rectangle Fit(Size source, Rectangle bounds)
        {
            if (source.Width <= 1 || source.Height <= 1 || bounds.Width <= 1 || bounds.Height <= 1)
                return bounds;

            float scale = Math.Min(bounds.Width / (float)source.Width, bounds.Height / (float)source.Height);
            int width = Math.Max(1, (int)(source.Width * scale));
            int height = Math.Max(1, (int)(source.Height * scale));
            return new Rectangle(bounds.X + (bounds.Width - width) / 2, bounds.Y + (bounds.Height - height) / 2, width, height);
        }

        public static bool TryMap(Point local, Size source, Rectangle shown, out Point remote)
        {
            remote = Point.Empty;
            if (source.Width <= 1 || source.Height <= 1 || shown.Width <= 1 || shown.Height <= 1)
                return false;
            if (local.X < shown.Left || local.Y < shown.Top || local.X > shown.Right || local.Y > shown.Bottom)
                return false;

            remote.X = (int)((local.X - shown.Left) * (double)source.Width / shown.Width);
            remote.Y = (int)((local.Y - shown.Top) * (double)source.Height / shown.Height);
            if (remote.X < 0) remote.X = 0;
            if (remote.Y < 0) remote.Y = 0;
            if (remote.X >= source.Width) remote.X = source.Width - 1;
            if (remote.Y >= source.Height) remote.Y = source.Height - 1;
            return true;
        }

        public static string TextKey(char c)
        {
            switch (c)
            {
                case '+': return "{+}";
                case '^': return "{^}";
                case '%': return "{%}";
                case '~': return "{~}";
                case '(': return "{(}";
                case ')': return "{)}";
                case '{': return "{{}";
                case '}': return "{}}";
                case '\r': return "{ENTER}";
                case '\n': return "";
                case '\t': return "{TAB}";
                case '\b': return "{BS}";
                default:
                    if (char.IsControl(c)) return "";
                    return c.ToString();
            }
        }

        public static string SpecialKey(int keyValue)
        {
            switch (keyValue)
            {
                case 8: return "{BS}";
                case 19: return "{BREAK}";
                case 20: return "{CAPSLOCK}";
                case 46: return "{DELETE}";
                case 40: return "{DOWN}";
                case 35: return "{End}";
                case 13: return "{ENTER}";
                case 27: return "{ESC}";
                case 36: return "{HOME}";
                case 45: return "{INSERT}";
                case 37: return "{LEFT}";
                case 144: return "{NUMLOCK}";
                case 34: return "{PGDN}";
                case 33: return "{PGUP}";
                case 39: return "{RIGHT}";
                case 145: return "{SCROLLLOCK}";
                case 9: return "{TAB}";
                case 38: return "{UP}";
                case 112: return "{F1}";
                case 113: return "{F2}";
                case 114: return "{F3}";
                case 115: return "{F4}";
                case 116: return "{F5}";
                case 117: return "{F6}";
                case 118: return "{F7}";
                case 119: return "{F8}";
                case 120: return "{F9}";
                case 121: return "{F10}";
                case 122: return "{F11}";
                case 123: return "{F12}";
                case 124: return "{F13}";
                case 125: return "{F14}";
                case 126: return "{F15}";
                case 127: return "{F16}";
                case 107: return "{ADD}";
                case 109: return "{SUBTRACT}";
                case 106: return "{MULTIPLY}";
                case 111: return "{DIVIDE}";
                case 65552: return "+";
                case 131089: return "^";
                case 262162: return "%";
                default: return "";
            }
        }
    }
}
