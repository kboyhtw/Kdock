using System.Drawing;
using System.Text;

namespace Kdock.Logic
{
    public static class RemoteCommands
    {
        public static void Ready(LanSession session)
        {
            Send(session, "remote:ready", "");
        }

        public static void Stop(LanSession session)
        {
            Send(session, "remote:stop", "");
        }

        public static void Clipboard(LanSession session, bool allowed)
        {
            if (session == null) return;
            session.SetClipboardAllowed(allowed);
            Send(session, allowed ? "remote:clipon" : "remote:clipoff", "");
        }

        public static void Pace(LanSession session, int pace)
        {
            Input(session, "pace:" + pace.ToString());
        }

        public static void Input(LanSession session, string input)
        {
            if (session == null || string.IsNullOrEmpty(input)) return;
            session.Send(new NetworkPackage(PackageType.String, "remote:input", Encoding.UTF8.GetBytes(input)));
        }

        public static void Mouse(LanSession session, Point local, Size source, Rectangle shown, string action, int wheel)
        {
            Point remote;
            if (!RemotePointer.TryMap(local, source, shown, out remote)) return;
            string payload = "M," + remote.X.ToString() + "," + remote.Y.ToString() + "," + action;
            if (action == "wh") payload += "," + wheel.ToString();
            Input(session, payload);
        }

        private static void Send(LanSession session, string instruction, string text)
        {
            if (session == null) return;
            byte[] data = string.IsNullOrEmpty(text) ? new byte[0] : Encoding.UTF8.GetBytes(text);
            session.Send(new NetworkPackage(PackageType.String, instruction, data));
        }
    }
}
