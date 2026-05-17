using System;
using Common;

namespace Server
{
    public class TransferEventPublisher
    {
        public delegate void TransferStartedHandler(
            object sender, TransferStartedEventArgs e);
        public delegate void SampleReceivedHandler(
            object sender, SampleReceivedEventArgs e);
        public delegate void TransferCompletedHandler(
            object sender, TransferCompletedEventArgs e);
        public delegate void WarningRaisedHandler(
            object sender, WarningRaisedEventArgs e);

        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event TransferCompletedHandler OnTransferCompleted;
        public event WarningRaisedHandler OnWarningRaised;

        public void RaiseTransferStarted(string vehicleId)
        {
            if (OnTransferStarted != null)
                OnTransferStarted(this, new TransferStartedEventArgs(vehicleId));
        }

        public void RaiseSampleReceived(string vehicleId, int rowIndex, int ukupno)
        {
            if (OnSampleReceived != null)
                OnSampleReceived(this,
                    new SampleReceivedEventArgs(vehicleId, rowIndex, ukupno));
        }

        public void RaiseTransferCompleted(string vehicleId, int ukupno)
        {
            if (OnTransferCompleted != null)
                OnTransferCompleted(this,
                    new TransferCompletedEventArgs(vehicleId, ukupno));
        }

        public void RaiseWarning(string vehicleId, int rowIndex,
            string tipUpozorenja, string poruka, double vrednost)
        {
            if (OnWarningRaised != null)
                OnWarningRaised(this,
                    new WarningRaisedEventArgs(vehicleId, rowIndex,
                        tipUpozorenja, poruka, vrednost));
        }
    }
}