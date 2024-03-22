using Common.Contracts;
using Common.Interfaces;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace AnomalyDetection.AnomalyDetections
{
    public abstract class AnomalyDetectionWorkerBase : IAnomalyDetectionWorker
    {
        protected static string kafkaBaseUrl = "localhost";

        protected Random _rand = new Random();

        protected Common.Interfaces.ILogger _logger;
        protected IDBDriver _dbDriver;

        protected abstract string AnomalyDetectionType { get; }
        protected abstract string AnomalyDetectionVersion { get; }
        protected abstract ConsumerConfig ConsumerConfig { get; }

        IConsumer<Ignore, string> _consumer;
        CancellationTokenSource _cts;

        public AnomalyDetectionWorkerBase(Common.Interfaces.ILogger logger, IDBDriver dbDriver)
        {
            _logger = logger;
            _dbDriver = dbDriver;
            _cts = new CancellationTokenSource();
            _consumer = new ConsumerBuilder<Ignore, string>(ConsumerConfig).Build();
            _consumer.Subscribe("cloudtrailtopic");
        }

        public abstract Task<int> AnalyzeAsync(CloudTrail cloudTrail);

        public async Task RunAsync()
        {
            _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"Worker: {AnomalyDetectionType} - Start");

            try
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(1000);

                        ConsumeResult<Ignore, string> cr = _consumer.Consume(_cts.Token);
                        _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"Worker: {AnomalyDetectionType}, Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");

                        CloudTrail cloudTrail = JsonConvert.DeserializeObject<CloudTrail>(cr.Value);

                        AnomalyDetectionResult result = new();
                        result.CloudTrail = cloudTrail;
                        result.AnomalyDetectionType = AnomalyDetectionType;
                        result.AnomalyDetectionVersion = AnomalyDetectionVersion;

                        bool alreadyExists = await AlreadyExists(result);
                        _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"Worker: {AnomalyDetectionType}, AlreadyExistsInDB: {alreadyExists}, ID: {cloudTrail.EventType + "." + cloudTrail.EventId}");

                        if (!alreadyExists)
                        {
                            var score = await AnalyzeAsync(cloudTrail);
                            _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"Worker: {AnomalyDetectionType}, Score: {score}, ID: {cloudTrail.EventType + "." + cloudTrail.EventId}");

                            if (score > 0)
                            {
                                result.AnomalyScore = score;
                                result.ProcessingTimestampUTC = DateTime.UtcNow;

                                await _dbDriver.UpsertAnomalyDetectionResultAsync(result);
                            }
                        }
                    }
                    catch (ConsumeException e)
                    {
                        _logger.Error($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", e.Error.Reason, e);
                    }
                    catch(Exception e)
                    {
                        _logger.Error($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", "Error", e);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", "Closing");
                _consumer.Close();
            }
        }

        protected async Task<bool> AlreadyExists(AnomalyDetectionResult anomalyDetectionResult)
        {
            return await _dbDriver.GetAnomalyDetectionResultAsync(anomalyDetectionResult) != null;
        }
    }
}
