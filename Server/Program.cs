using System;
using System.ServiceModel;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(EVChargingService));
            host.Open();
            Console.WriteLine("[SERVER] Servis je pokrenut. Cekam klijente...");
            Console.WriteLine("[SERVER] Pritisni Enter za gasenje.");
            Console.ReadLine();
            host.Close();
        }
    }
}