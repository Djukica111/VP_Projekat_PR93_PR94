using System;
using System.ServiceModel;
using Common;

namespace Server
{
    public class EVChargingService : IEVChargingService
    {
        public void StartSession(string vehicleId)
        {
            Console.WriteLine($"[SERVER] Sesija zapoceta za vozilo: {vehicleId}");
        }

        public void PushSample(ChargingSample sample)
        {
            ValidateSample(sample);
            Console.WriteLine($"[SERVER] Primljen red {sample.RowIndex} za vozilo {sample.VehicleId} - prenos u toku...");
        }

        public void EndSession(string vehicleId)
        {
            Console.WriteLine($"[SERVER] Sesija zavrsena za vozilo: {vehicleId}");
            Console.WriteLine($"[SERVER] Prenos zavrsen.");
        }

        private void ValidateSample(ChargingSample sample)
        {
            if (string.IsNullOrEmpty(sample.Timestamp))
            {
                throw new FaultException<string>(
                    "Neispravan red: Timestamp je prazan.",
                    new FaultReason("Validacija nije prosla"));
            }

            if (sample.VoltageAvg <= 0)
            {
                throw new FaultException<string>(
                    $"Neispravan red {sample.RowIndex}: Napon mora biti veci od 0.",
                    new FaultReason("Validacija nije prosla"));
            }

            if (sample.FrequencyAvg <= 0)
            {
                throw new FaultException<string>(
                    $"Neispravan red {sample.RowIndex}: Frekvencija mora biti veca od 0.",
                    new FaultReason("Validacija nije prosla"));
            }
        }
    }
}