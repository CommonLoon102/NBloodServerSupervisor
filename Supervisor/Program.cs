using System;
using System.Threading;
using Model;

namespace Supervisor
{
    class Program
    {
        public static readonly State State = new State();

        public static void Main()
        {
            NBloodServerListener.StartListening();
            WebApiListener.StartListening();
            PublicServerManager.Start();
            PrivateServerManager.Start();
            
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
