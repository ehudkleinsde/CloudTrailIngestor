using Common.Contracts;

namespace Common.Interfaces
{
    public interface IDBDriver
    {
        Task<AnomalyDetectionResult> GetAnomalyDetectionResultAsync(AnomalyDetectionResult anomalyDetectionResult);
        Task UpsertAnomalyDetectionResultAsync(AnomalyDetectionResult anomalyDetectionResult);
    }
}
