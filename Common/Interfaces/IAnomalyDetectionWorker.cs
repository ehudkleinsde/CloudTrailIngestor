using Common.Contracts;

namespace Common.Interfaces
{
    public interface IAnomalyDetectionWorker
    {
        Task RunAsync();
        Task<int> AnalyzeAsync(CloudTrail cloudTrail);
    }
}
