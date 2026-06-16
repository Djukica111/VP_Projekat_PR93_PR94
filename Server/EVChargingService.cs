using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EVChargingService : IEVChargingService
    {
        private Dictionary<string, SessionResource> _aktivneSesije
            = new Dictionary<string, SessionResource>();

        private Dictionary<string, int> _brojacRedova
            = new Dictionary<string, int>();

        private Dictionary<string, EnergyAnalyzer> _energyAnalyzeri
            = new Dictionary<string, EnergyAnalyzer>();

        private Dictionary<string, FrequencyAnalyzer> _frequencyAnalyzeri
            = new Dictionary<string, FrequencyAnalyzer>();

        public TransferEventPublisher Publisher { get; private set; }
            = new TransferEventPublisher();

        private string GetSessionPath(string vehicleId)
        {
            string datum = DateTime.Now.ToString("yyyy-MM-dd");
            string folder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", vehicleId, datum);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "session.csv");
        }

        private string GetRejectsPath(string vehicleId)
        {
            string datum = DateTime.Now.ToString("yyyy-MM-dd");
            string folder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", vehicleId, datum);
            return Path.Combine(folder, "rejects.csv");
        }

        public void StartSession(string vehicleId)
        {
            string sessionPath = GetSessionPath(vehicleId);
            SessionResource sesija = new SessionResource(sessionPath, vehicleId);

            if (new FileInfo(sessionPath).Length == 0)
            {
                sesija.WriteLine(
                    "Timestamp,VoltageMin,VoltageAvg,VoltageMax," +
                    "CurrentMin,CurrentAvg,CurrentMax," +
                    "RealPowerMin,RealPowerAvg,RealPowerMax," +
                    "ReactivePowerMin,ReactivePowerAvg,ReactivePowerMax," +
                    "ApparentPowerMin,ApparentPowerAvg,ApparentPowerMax," +
                    "FrequencyMin,FrequencyAvg,FrequencyMax,RowIndex");
            }

            _aktivneSesije[vehicleId] = sesija;
            _brojacRedova[vehicleId] = 0;
            _energyAnalyzeri[vehicleId] = new EnergyAnalyzer(Publisher);
            _frequencyAnalyzeri[vehicleId] = new FrequencyAnalyzer(Publisher);

            Publisher.RaiseTransferStarted(vehicleId);
        }

        public void PushSample(ChargingSample sample)
        {
            if (sample == null)
            {
                throw new FaultException<string>(
                    "Sample ne sme biti null.",
                    new FaultReason("Validacija nije prosla"));
            }

            if (string.IsNullOrWhiteSpace(sample.VehicleId))
            {
                throw new FaultException<string>(
                    "VehicleId ne sme biti prazan.",
                    new FaultReason("Validacija nije prosla"));
            }

            if (!_aktivneSesije.ContainsKey(sample.VehicleId))
            {
                LogRejected(sample, "Sesija nije otvorena za ovo vozilo.");
                throw new FaultException<string>(
                    $"Sesija nije otvorena za vozilo {sample.VehicleId}.",
                    new FaultReason("Sesija nije pokrenuta"));
            }

            ValidateSample(sample);

            if (_aktivneSesije.ContainsKey(sample.VehicleId))
            {
                SessionResource sesija = _aktivneSesije[sample.VehicleId];
                sesija.WriteLine(
                    $"{sample.Timestamp}," +
                    $"{sample.VoltageMin},{sample.VoltageAvg},{sample.VoltageMax}," +
                    $"{sample.CurrentMin},{sample.CurrentAvg},{sample.CurrentMax}," +
                    $"{sample.RealPowerMin},{sample.RealPowerAvg},{sample.RealPowerMax}," +
                    $"{sample.ReactivePowerMin},{sample.ReactivePowerAvg},{sample.ReactivePowerMax}," +
                    $"{sample.ApparentPowerMin},{sample.ApparentPowerAvg},{sample.ApparentPowerMax}," +
                    $"{sample.FrequencyMin},{sample.FrequencyAvg},{sample.FrequencyMax}," +
                    $"{sample.RowIndex}");

                _brojacRedova[sample.VehicleId]++;
            }

            // Pokreni analitiku
            if (_energyAnalyzeri.ContainsKey(sample.VehicleId))
                _energyAnalyzeri[sample.VehicleId].AnalizirajUzorak(sample);

            if (_frequencyAnalyzeri.ContainsKey(sample.VehicleId))
                _frequencyAnalyzeri[sample.VehicleId].AnalizirajUzorak(sample);

            Publisher.RaiseSampleReceived(
                sample.VehicleId,
                sample.RowIndex,
                _brojacRedova[sample.VehicleId]);
        }

        public void EndSession(string vehicleId)
        {
            int ukupno = _brojacRedova.ContainsKey(vehicleId)
                ? _brojacRedova[vehicleId] : 0;

            if (_aktivneSesije.ContainsKey(vehicleId))
            {
                using (SessionResource sesija = _aktivneSesije[vehicleId])
                {
                    Console.WriteLine($"[SERVER] Zatvaranje sesije...");
                }
                _aktivneSesije.Remove(vehicleId);
            }

            if (_brojacRedova.ContainsKey(vehicleId))
                _brojacRedova.Remove(vehicleId);

            if (_energyAnalyzeri.ContainsKey(vehicleId))
            {
                _energyAnalyzeri[vehicleId].ResetujStanje();
                _energyAnalyzeri.Remove(vehicleId);
            }

            if (_frequencyAnalyzeri.ContainsKey(vehicleId))
            {
                _frequencyAnalyzeri[vehicleId].ResetujStanje();
                _frequencyAnalyzeri.Remove(vehicleId);
            }

            Publisher.RaiseTransferCompleted(vehicleId, ukupno);
        }

        private void ValidateSample(ChargingSample sample)
        {
            if (string.IsNullOrEmpty(sample.Timestamp))
            {
                LogRejected(sample, "Timestamp je prazan");
                throw new FaultException<string>(
                    "Neispravan red: Timestamp je prazan.",
                    new FaultReason("Validacija nije prosla"));
            }

            if (sample.VoltageAvg <= 0)
            {
                LogRejected(sample, "Napon mora biti veci od 0");
                throw new FaultException<string>(
                    $"Neispravan red {sample.RowIndex}: Napon mora biti veci od 0.",
                    new FaultReason("Validacija nije prosla"));
            }

            if (sample.FrequencyAvg <= 0)
            {
                LogRejected(sample, "Frekvencija mora biti veca od 0");
                throw new FaultException<string>(
                    $"Neispravan red {sample.RowIndex}: Frekvencija mora biti veca od 0.",
                    new FaultReason("Validacija nije prosla"));
            }
        }

        private void LogRejected(ChargingSample sample, string razlog)
        {
            string rejectsPath = GetRejectsPath(sample.VehicleId);

            using (StreamWriter writer = new StreamWriter(rejectsPath, append: true))
            {
                if (new FileInfo(rejectsPath).Length == 0)
                    writer.WriteLine("RowIndex,Timestamp,Razlog");

                writer.WriteLine(
                    $"{sample.RowIndex},{sample.Timestamp},{razlog}");
            }

            Console.WriteLine($"[SERVER] Red {sample.RowIndex} odbijen: {razlog}");
        }
    }
}