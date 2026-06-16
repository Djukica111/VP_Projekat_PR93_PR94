using Common;
using System;

namespace Server
{
    public class TransferEventSubscriber
    {
        public TransferEventSubscriber(TransferEventPublisher publisher)
        {
            publisher.OnTransferStarted += OnTransferStarted;
            publisher.OnSampleReceived += OnSampleReceived;
            publisher.OnTransferCompleted += OnTransferCompleted;
            publisher.OnWarningRaised += OnWarningRaised;
        }

        private void OnTransferStarted(object sender, TransferStartedEventArgs e)
        {
            Console.WriteLine($"[DOGADJAJ] Prenos poceо | " +
                $"Vozilo: {e.VehicleId} | " +
                $"Vreme: {e.VremePocetka:HH:mm:ss}");
        }

        private void OnSampleReceived(object sender, SampleReceivedEventArgs e)
        {
            Console.WriteLine($"[DOGADJAJ] Uzorak primljen | " +
                $"Vozilo: {e.VehicleId} | " +
                $"Red: {e.RowIndex} | " +
                $"Ukupno: {e.UkupnoPrimljeno}");
        }

        private void OnTransferCompleted(object sender, TransferCompletedEventArgs e)
        {
            Console.WriteLine($"[DOGADJAJ] Prenos zavrsen | " +
                $"Vozilo: {e.VehicleId} | " +
                $"Ukupno redova: {e.UkupnoRedova} | " +
                $"Vreme: {e.VremeKraja:HH:mm:ss}");
        }

        private void OnWarningRaised(object sender, WarningRaisedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[UPOZORENJE] {e.TipUpozorenja} | " +
                $"Vozilo: {e.VehicleId} | " +
                $"Red: {e.RowIndex} | " +
                $"Vrednost: {e.TrenutnaVrednost:F2} | " +
                $"{e.Poruka}");
            Console.ResetColor();
        }
    }
}