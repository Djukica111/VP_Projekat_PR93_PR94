using System;

namespace Common
{
    public class TransferStartedEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public DateTime VremePocetka { get; }

        public TransferStartedEventArgs(string vehicleId)
        {
            VehicleId = vehicleId;
            VremePocetka = DateTime.Now;
        }
    }

    public class SampleReceivedEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public int RowIndex { get; }
        public int UkupnoPrimljeno { get; }

        public SampleReceivedEventArgs(string vehicleId, int rowIndex, int ukupno)
        {
            VehicleId = vehicleId;
            RowIndex = rowIndex;
            UkupnoPrimljeno = ukupno;
        }
    }

    public class TransferCompletedEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public int UkupnoRedova { get; }
        public DateTime VremeKraja { get; }

        public TransferCompletedEventArgs(string vehicleId, int ukupno)
        {
            VehicleId = vehicleId;
            UkupnoRedova = ukupno;
            VremeKraja = DateTime.Now;
        }
    }

    public class WarningRaisedEventArgs : EventArgs
    {
        public string VehicleId { get; }
        public int RowIndex { get; }
        public string TipUpozorenja { get; }
        public string Poruka { get; }
        public double TrenutnaVrednost { get; }

        public WarningRaisedEventArgs(string vehicleId, int rowIndex,
            string tipUpozorenja, string poruka, double trenutnaVrednost)
        {
            VehicleId = vehicleId;
            RowIndex = rowIndex;
            TipUpozorenja = tipUpozorenja;
            Poruka = poruka;
            TrenutnaVrednost = trenutnaVrednost;
        }
    }
}