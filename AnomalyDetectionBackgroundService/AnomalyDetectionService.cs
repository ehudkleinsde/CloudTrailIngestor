using Common.Interfaces;

namespace AnomalyDetection
{
    public class AnomalyDetectionService
    {
        private Common.Interfaces.ILogger _logger;
        private IAnomalyDetectionWorker[] _anomalyDetectionWorkers;

        public AnomalyDetectionService(IAnomalyDetectionWorker[] anomalyDetectionWorkers, Common.Interfaces.ILogger logger)
        {
            _anomalyDetectionWorkers = anomalyDetectionWorkers;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            List<Task> workes = _anomalyDetectionWorkers.Select(w => w.RunAsync()).ToList();
            await(Task.WhenAll(workes));
        }
    }
}
