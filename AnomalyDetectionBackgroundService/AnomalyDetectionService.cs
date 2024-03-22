using AnomalyDetection.AnomalyDetections;
using Common.Interfaces;

namespace AnomalyDetection
{
    public class AnomalyDetectionService
    {
        private Common.Interfaces.ILogger _logger;
        private AnomalyDetectionWorker1 _anomalyDetectionWorker1;
        private AnomalyDetectionWorker2 _anomalyDetectionWorker2;
        private AnomalyDetectionWorker3 _anomalyDetectionWorker3;

        private IAnomalyDetectionWorker[] _workers;

        public AnomalyDetectionService(AnomalyDetectionWorker1 anomalyDetectionWorker1, AnomalyDetectionWorker2 anomalyDetectionWorker2, AnomalyDetectionWorker3 anomalyDetectionWorker3, Common.Interfaces.ILogger logger)
        {
            _anomalyDetectionWorker1 = anomalyDetectionWorker1;
            _anomalyDetectionWorker2 = anomalyDetectionWorker2;
            _anomalyDetectionWorker3 = anomalyDetectionWorker3;

            _workers = new IAnomalyDetectionWorker[] { anomalyDetectionWorker1, anomalyDetectionWorker2, anomalyDetectionWorker3 };
            _logger = logger;
        }

        public async Task RunAsync()
        {
            List<Task> workes = _workers.Select(w => w.RunAsync()).ToList();
            await (Task.WhenAll(workes));
        }
    }
}
