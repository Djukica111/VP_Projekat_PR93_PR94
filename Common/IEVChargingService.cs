using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IEVChargingService
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        void StartSession(string vehicleId);

        [OperationContract]
        [FaultContract(typeof(string))]
        void PushSample(ChargingSample sample);

        [OperationContract]
        [FaultContract(typeof(string))]
        void EndSession(string vehicleId);
    }
}