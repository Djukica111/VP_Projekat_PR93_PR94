using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Server
{
    public class EVChargingService : IEVChargingService
    {
        private Dictionary<string, SessionResource> _aktivneSesije
            = new Dictionary<string, SessionResource>();

        public void StartSession(string vehicleId)
        {
            Console.WriteLine($"[SERVER] Sesija zapoceta za vozilo: {vehicleId}");
            
            string filePath = Path.GetTempPath();
            _aktivneSesije[vehicleId] = new SessionResource(filePath, vehicleId);
        }

        public void PushSample(ChargingSample sample)
        {
            ValidateSample(sample);
            Console.WriteLine($"[SERVER] Primljen red {sample.RowIndex} za vozilo {sample.VehicleId} - prenos u toku...");
        }

        public void EndSession(string vehicleId)
        {
            if (_aktivneSesije.ContainsKey(vehicleId))
            {
                using (SessionResource sesija = _aktivneSesije[vehicleId])
                {
                    Console.WriteLine($"[SERVER] Sesija zavrsena za vozilo: {vehicleId}");
                }
                _aktivneSesije.Remove(vehicleId);
            }
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