using System;
using System.Configuration;
using Common;

namespace Server
{
    public class FrequencyAnalyzer
    {
        private const double NOMINALNA_FREKVENCIJA = 60.0;
        private const double DOZVOLJENO_ODSTUPANJE = 0.01;

        private double _prethodniFreqMin = -1;
        private double _prethodniFreqMax = -1;
        private double _spikePrag;

        public TransferEventPublisher Publisher { get; set; }

        public FrequencyAnalyzer(TransferEventPublisher publisher)
        {
            Publisher = publisher;

            string vrednost = ConfigurationManager
                .AppSettings["FrequencySpikeThreshold"];
            if (!double.TryParse(vrednost, out _spikePrag))
                _spikePrag = 1.0;
        }

        public void AnalizirajUzorak(ChargingSample sample)
        {
            CheckDeviation(sample);
            CheckSpike(sample);

            // Zapamti vrednosti za sledeci red
            _prethodniFreqMin = sample.FrequencyMin;
            _prethodniFreqMax = sample.FrequencyMax;
        }

        private void CheckDeviation(ChargingSample sample)
        {
            double odstupanje = Math.Abs(
                sample.FrequencyAvg - NOMINALNA_FREKVENCIJA);

            if (odstupanje > DOZVOLJENO_ODSTUPANJE)
            {
                Publisher.RaiseWarning(
                    sample.VehicleId,
                    sample.RowIndex,
                    "FrequencyDeviationWarning",
                    $"Frequency Avg ({sample.FrequencyAvg:F3} Hz) odstupa za " +
                    $"{odstupanje:F3} Hz od nominalne vrednosti " +
                    $"({NOMINALNA_FREKVENCIJA} Hz). " +
                    $"Dozvoljeno odstupanje: ±{DOZVOLJENO_ODSTUPANJE} Hz",
                    sample.FrequencyAvg);
            }
        }

        private void CheckSpike(ChargingSample sample)
        {
            // Preskoci prvi red jer nema prethodnih vrednosti
            if (_prethodniFreqMin < 0 || _prethodniFreqMax < 0)
                return;

            double deltaMin = Math.Abs(sample.FrequencyMin - _prethodniFreqMin);
            double deltaMax = Math.Abs(sample.FrequencyMax - _prethodniFreqMax);

            if (deltaMin > _spikePrag)
            {
                Publisher.RaiseWarning(
                    sample.VehicleId,
                    sample.RowIndex,
                    "FrequencySpike",
                    $"Nagli skok Frequency Min: " +
                    $"prethodni={_prethodniFreqMin:F3} Hz, " +
                    $"trenutni={sample.FrequencyMin:F3} Hz, " +
                    $"delta={deltaMin:F3} Hz (prag={_spikePrag:F3} Hz)",
                    deltaMin);
            }

            if (deltaMax > _spikePrag)
            {
                Publisher.RaiseWarning(
                    sample.VehicleId,
                    sample.RowIndex,
                    "FrequencySpike",
                    $"Nagli skok Frequency Max: " +
                    $"prethodni={_prethodniFreqMax:F3} Hz, " +
                    $"trenutni={sample.FrequencyMax:F3} Hz, " +
                    $"delta={deltaMax:F3} Hz (prag={_spikePrag:F3} Hz)",
                    deltaMax);
            }
        }

        public void ResetujStanje()
        {
            _prethodniFreqMin = -1;
            _prethodniFreqMax = -1;
        }
    }
}