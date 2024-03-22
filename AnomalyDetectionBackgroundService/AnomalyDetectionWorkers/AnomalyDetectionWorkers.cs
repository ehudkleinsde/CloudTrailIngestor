using Common.Contracts;
using Common.Interfaces;
using Confluent.Kafka;

namespace AnomalyDetection.AnomalyDetections
{
    public class AnomalyDetectionWorker1 : AnomalyDetectionWorkerBase
    {
        protected override string AnomalyDetectionType { get; } = "Anomaly1";
        protected override string AnomalyDetectionVersion { get; } = "1";
        protected override ConsumerConfig ConsumerConfig { get;  } = new ConsumerConfig
        {
            BootstrapServers = $"{kafkaBaseUrl}:9092",
            GroupId = $"{nameof(AnomalyDetectionWorker1)}-consumer-group",
            ClientId = $"{nameof(AnomalyDetectionWorker1)}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,

        };

        public AnomalyDetectionWorker1(Common.Interfaces.ILogger logger, IDBDriver dbDriver) : base(logger, dbDriver) { }

        public override async Task<int> AnalyzeAsync(CloudTrail cloudTrail)
        {
            return _rand.Next();
        }
    }

    public class AnomalyDetectionWorker2 : AnomalyDetectionWorkerBase
    {
        protected override ConsumerConfig ConsumerConfig { get; } = new ConsumerConfig
        {
            BootstrapServers = $"{kafkaBaseUrl}:9092",
            GroupId = $"{nameof(AnomalyDetectionWorker2)}-consumer-group",
            ClientId = $"{nameof(AnomalyDetectionWorker2)}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

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
        protected override ConsumerConfig ConsumerConfig { get; } = new ConsumerConfig
        {
            BootstrapServers = $"{kafkaBaseUrl}:9092",
            GroupId = $"{nameof(AnomalyDetectionWorker3)}-consumer-group",
            ClientId = $"{nameof(AnomalyDetectionWorker3)}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        protected override string AnomalyDetectionType { get; } = "Anomaly3";
        protected override string AnomalyDetectionVersion { get; } = "1";
        public AnomalyDetectionWorker3(Common.Interfaces.ILogger logger, IDBDriver dbDriver) : base(logger, dbDriver) { }

        public override async Task<int> AnalyzeAsync(CloudTrail cloudTrail)
        {
            return _rand.Next();
        }
    }
}
