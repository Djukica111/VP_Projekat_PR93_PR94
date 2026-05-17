using System;
using System.ServiceModel;
using Common;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(EVChargingService));
            host.Open();

            // Dohvati instancu servisa i pretplati subscriber na dogadjaje
            EVChargingService servis = new EVChargingService();
            TransferEventSubscriber subscriber =
                new TransferEventSubscriber(servis.Publisher);

            Console.WriteLine("[SERVER] Servis je pokrenut. Cekam klijente...");
            Console.WriteLine("[SERVER] Pritisni Enter za gasenje.");
            Console.ReadLine();

            host.Close();
        }
    }
}