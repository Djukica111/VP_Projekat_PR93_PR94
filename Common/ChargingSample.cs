using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ChargingSample
    {
        [DataMember]
        public string VehicleId { get; set; }

        [DataMember]
        public int RowIndex { get; set; }

        [DataMember]
        public string Timestamp { get; set; }

        // Voltage RMS
        [DataMember]
        public double VoltageMin { get; set; }
        [DataMember]
        public double VoltageAvg { get; set; }
        [DataMember]
        public double VoltageMax { get; set; }

        // Current RMS
        [DataMember]
        public double CurrentMin { get; set; }
        [DataMember]
        public double CurrentAvg { get; set; }
        [DataMember]
        public double CurrentMax { get; set; }

        // Real Power
        [DataMember]
        public double RealPowerMin { get; set; }
        [DataMember]
        public double RealPowerAvg { get; set; }
        [DataMember]
        public double RealPowerMax { get; set; }

        // Reactive Power
        [DataMember]
        public double ReactivePowerMin { get; set; }
        [DataMember]
        public double ReactivePowerAvg { get; set; }
        [DataMember]
        public double ReactivePowerMax { get; set; }

        // Apparent Power
        [DataMember]
        public double ApparentPowerMin { get; set; }
        [DataMember]
        public double ApparentPowerAvg { get; set; }
        [DataMember]
        public double ApparentPowerMax { get; set; }

        // Frequency
        [DataMember]
        public double FrequencyMin { get; set; }
        [DataMember]
        public double FrequencyAvg { get; set; }
        [DataMember]
        public double FrequencyMax { get; set; }
    }
}