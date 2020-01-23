using System;
using System.Threading;
using Model;

namespace Supervisor
{
    class Program
    {
        public static readonly State State = new State();

        public static void Main(string[] args)
        {
            NBloodServerListener.StartListening();
            WebApiListener.StartListening();

            if (args.Length > 0)
                PublicServerManager.Start(args[0]);

            PrivateServerManager.Start();
            
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
