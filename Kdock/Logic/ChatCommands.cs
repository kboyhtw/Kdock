using System;
using System.Text;

namespace Kdock.Logic
{
    public static class ChatCommands
    {
        public static void SendText(LanSession session, string text)
        {
            if (session == null || string.IsNullOrWhiteSpace(text)) return;
            string body = text.Replace("\r\n", "\n");
            session.Send(new NetworkPackage(PackageType.String, "MSG:" + Environment.MachineName, Encoding.UTF8.GetBytes(body)));
        }

        public static void OfferFile(LanSession session, string id, string fileName, long length)
        {
            if (session == null) return;
            session.Send(new NetworkPackage(PackageType.String, "chatfile:" + id, Encoding.UTF8.GetBytes(fileName + "\t" + length)));
        }
    }
}
