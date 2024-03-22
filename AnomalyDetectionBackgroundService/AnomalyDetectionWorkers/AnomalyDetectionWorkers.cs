using Common.Contracts;
using Common.Interfaces;

namespace AnomalyDetection.AnomalyDetections
{
    public class AnomalyDetectionWorker1 : AnomalyDetectionWorkerBase
    {
        protected override string TopicName { get; } = "Anomaly1";

        protected override string AnomalyDetectionType { get; } = "Anomaly1";

        protected override string AnomalyDetectionVersion { get; } = "1";

        public AnomalyDetectionWorker1(Common.Interfaces.ILogger logger, IDBDriver dbDriver) : base(logger, dbDriver) { }

        public override async Task<int> AnalyzeAsync(CloudTrail cloudTrail)
        {
            return _rand.Next();
        }
    }

    public class AnomalyDetectionWorker2 : AnomalyDetectionWorkerBase
    {
        protected override string TopicName { get; } = "Anomaly2";
        protected override string AnomalyDetectionType { get; } = "Anomaly2";

        protected override string AnomalyDetectionVersion { get; } = "1";
        public AnomalyDetectionWorker2(Common.Interfaces.ILogger logger, IDBDriver dbDriver) : base(logger, dbDriver) { }

        public override async Task<int> AnalyzeAsync(CloudTrail cloudTrail)
        {
            return _rand.Next();
        }
    }

    public class AnomalyDetectionWorker3 : AnomalyDetectionWorkerBase
    {
        protected override string TopicName { get; } = "Anomaly3";
        protected override string AnomalyDetectionType { get; } = "Anomaly3";
        protected override string AnomalyDetectionVersion { get; } = "1";
        public AnomalyDetectionWorker3(Common.Interfaces.ILogger logger, IDBDriver dbDriver) : base(logger, dbDriver) { }

        public override async Task<int> AnalyzeAsync(CloudTrail cloudTrail)
        {
            return _rand.Next();
        }
    }
}
