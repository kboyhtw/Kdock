using System;
using System.Text;

namespace Kdock.Logic
{
    public static class FileCommands
    {
        public static void Browse(LanSession session, string path)
        {
            if (session == null) return;
            session.Send(new NetworkPackage(PackageType.String, "files:getdrives:" + Environment.MachineName, Encoding.UTF8.GetBytes(path ?? "")));
        }

        public static void Transfer(LanSession session, object tag, string destinationFolder)
        {
            if (session == null || tag == null) return;
            string raw = tag.ToString();
            int split = raw.IndexOf(':');
            if (split < 0) return;
            string action = raw.Substring(0, split);
            string source = raw.Substring(split + 1);
            string payload = source + KGlobal.mainsep + (destinationFolder ?? "");
            session.Send(new NetworkPackage(PackageType.String, "files:" + action, Encoding.UTF8.GetBytes(payload)));
        }

        public static void GetFile(LanSession session, string path)
        {
            if (session == null) return;
            session.Send(new NetworkPackage(PackageType.String, "files:getfile:" + Environment.MachineName, Encoding.UTF8.GetBytes(path ?? "")));
        }

        public static void Delete(LanSession session, bool folder, string path)
        {
            if (session == null) return;
            string command = folder ? "files:deletedirectory:" : "files:deletefile:";
            session.Send(new NetworkPackage(PackageType.String, command + Environment.MachineName, Encoding.UTF8.GetBytes(path ?? "")));
        }
    }
}
