using Common;
using System.Configuration;

namespace Server
{
    public class EnergyAnalyzer
    {
        private double _kumulativnaEnergija = 0;
        private double? _prethodniRealPowerAvg = null;
        private int _brojRedovaBezRasta = 0;
        private double _overloadPrag;

        private const int STAGNACIJA_LIMIT = 12;
        private const double MINIMALNI_RAST = 0.01;

        public TransferEventPublisher Publisher { get; set; }

        public EnergyAnalyzer(TransferEventPublisher publisher)
        {
            Publisher = publisher;

            // Ucitaj prag iz konfiguracije
            string vrednost = ConfigurationManager.AppSettings["OverloadWarningThreshold"];
            if (!double.TryParse(vrednost, out _overloadPrag))
                _overloadPrag = 50000;
        }

        public void AnalizirajUzorak(ChargingSample sample)
        {
            _kumulativnaEnergija += sample.RealPowerAvg;

            if (_prethodniRealPowerAvg.HasValue)
            {
                double rast = sample.RealPowerAvg - _prethodniRealPowerAvg.Value;

                if (rast < MINIMALNI_RAST)
                {
                    _brojRedovaBezRasta++;

                    if (_brojRedovaBezRasta >= STAGNACIJA_LIMIT)
                    {
                        Publisher.RaiseWarning(
                            sample.VehicleId,
                            sample.RowIndex,
                            "EnergyStallWarning",
                            $"Energija stagnira vec {_brojRedovaBezRasta} redova. " +
                            $"Kumulativna vrednost: {_kumulativnaEnergija:F2}",
                            _kumulativnaEnergija);

                        _brojRedovaBezRasta = 0;
                    }
                }
                else
                {
                    _brojRedovaBezRasta = 0;
                }
            }

            _prethodniRealPowerAvg = sample.RealPowerAvg;
            
            if (sample.RealPowerMax > _overloadPrag)
            {
                Publisher.RaiseWarning(
                    sample.VehicleId,
                    sample.RowIndex,
                    "OverloadWarning",
                    $"Real Power Max ({sample.RealPowerMax:F2}) " +
                    $"premasuje prag ({_overloadPrag:F2})",
                    sample.RealPowerMax);
            }
        }

        public void ResetujStanje()
        {
            _kumulativnaEnergija = 0;
            _prethodniRealPowerAvg = null;
            _brojRedovaBezRasta = 0;
        }
    }
}