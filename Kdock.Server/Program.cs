using System;

namespace Kdock
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (DeskBroker.IsHosted())
            {
                DeskBroker.RunHosted(args);
                return;
            }

            int port = 12605;
            if (args != null && args.Length > 0)
                int.TryParse(args[0], out port);
            DeskBroker.Run(port);
        }
    }
}
