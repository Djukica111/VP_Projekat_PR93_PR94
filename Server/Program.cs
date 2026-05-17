using System;
using System.ServiceModel;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            EVChargingService servis = new EVChargingService();
            TransferEventSubscriber subscriber =
                new TransferEventSubscriber(servis.Publisher);

            ServiceHost host = new ServiceHost(servis);
            host.Open();

            Console.WriteLine("[SERVER] Servis je pokrenut. Cekam klijente...");
            Console.WriteLine("[SERVER] Pritisni Enter za gasenje.");
            Console.ReadLine();

            host.Close();
        }
    }
}
