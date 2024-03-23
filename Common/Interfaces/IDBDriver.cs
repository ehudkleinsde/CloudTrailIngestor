using Common.Contracts;

namespace Common.Interfaces
{
    public interface IDBDriver
    {
        Task InsertAnomalyDetectionResultAsync(AnomalyDetectionResult anomalyDetectionResult);
    }
}
