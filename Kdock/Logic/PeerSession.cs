namespace Kdock.Logic
{
    public static class PeerSession
    {
        public static void Open(string address, string mode)
        {
            if (DeskRelay.IsId(address))
            {
                DeskRelay.EnsureOnline();
                DeskRelay.Call(address, mode);
                return;
            }
            if (mode == "chat") LanSession.BeginChat(address);
            else if (mode == "files") LanSession.BeginFiles(address);
            else LanSession.BeginRemote(address);
        }
    }
}
