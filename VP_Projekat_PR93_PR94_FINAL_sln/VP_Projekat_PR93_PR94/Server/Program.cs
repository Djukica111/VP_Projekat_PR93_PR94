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

            ServiceHost host = null;

            try
            {
                host = new ServiceHost(servis);
                host.Open();

                Console.WriteLine("[SERVER] Servis je pokrenut. Cekam klijente...");
                Console.WriteLine("[SERVER] Pritisni Enter za gasenje.");
                Console.ReadLine();

                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER] Greska: {ex.Message}");

                if (host != null)
                    host.Abort();
            }
        }
    }
}
