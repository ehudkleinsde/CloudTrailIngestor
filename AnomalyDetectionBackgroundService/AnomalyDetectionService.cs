using AnomalyDetection.AnomalyDetections;
using Common.Interfaces;

namespace AnomalyDetection
{
    public class AnomalyDetectionService
    {
        private Common.Interfaces.ILogger _logger;
        private IAnomalyDetectionWorker[] _workers;

        public AnomalyDetectionService(
            IAnomalyDetectionWorker[] workers, Common.Interfaces.ILogger logger)
        {
            _workers = workers;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            await Task.Delay(60_000);
            List<Task> workers = _workers.Select(w => Task.Run(() => w.RunAsync())).ToList();
            await Task.WhenAll(workers);
        }
    }
}
