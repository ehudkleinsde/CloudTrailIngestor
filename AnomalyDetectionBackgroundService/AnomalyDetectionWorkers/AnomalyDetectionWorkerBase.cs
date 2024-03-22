using Common.Contracts;
using Common.Interfaces;
using Newtonsoft.Json;

namespace AnomalyDetection.AnomalyDetections
{
    public abstract class AnomalyDetectionWorkerBase : IAnomalyDetectionWorker
    {
        protected Random _rand = new Random();

        protected Common.Interfaces.ILogger _logger;
        protected IDBDriver _dbDriver;
        private static readonly HttpClient _client = new HttpClient();

        protected abstract string TopicName { get; }
        protected abstract string AnomalyDetectionType { get; }
        protected abstract string AnomalyDetectionVersion { get; }

        private Uri _messageQueueUri = new Uri("http://cloudtrailmessagebroker:8080/CloudTrailMessageBroker/DequeueAsync");

        public AnomalyDetectionWorkerBase(Common.Interfaces.ILogger logger, IDBDriver dbDriver)
        {
            _logger = logger;
            _dbDriver = dbDriver;
        }

        public abstract Task<int> AnalyzeAsync(CloudTrail cloudTrail);

        public async Task RunAsync()
        {
            CloudTrail cloudTrail;

            //TODO: add cancellationToken
            while (true)
            {
                while ((cloudTrail = await DequeueAsync(TopicName)) == null)
                {
                    _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"{TopicName} is empty");
                    await Task.Delay(5000);
                }

                _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"Got cloudTrail from {TopicName}");

                AnomalyDetectionResult result = new();
                result.CloudTrail = cloudTrail;
                result.AnomalyDetectionType = AnomalyDetectionType;
                result.AnomalyDetectionVersion = AnomalyDetectionVersion;

                bool alreadyExists = await AlreadyExists(result);
                _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"AlreadyExistsInDB: {alreadyExists}, ID: {cloudTrail.EventType+"."+cloudTrail.EventId}");

                if (!alreadyExists)
                {
                    var score = await AnalyzeAsync(cloudTrail);
                    _logger.Info($"{nameof(AnomalyDetectionWorkerBase)}.{nameof(RunAsync)}", $"Score: {score}, ID: {cloudTrail.EventType + "." + cloudTrail.EventId}");

                    if (score > 0)
                    {
                        result.AnomalyScore = score;
                        result.ProcessingTimestampUTC = DateTime.UtcNow;

                        await _dbDriver.UpsertAnomalyDetectionResultAsync(result);
                    }
                }
            }
        }

        protected async Task<CloudTrail> DequeueAsync(string topicName)
        {
            string urlWithParameters = $"{_messageQueueUri}?topicName={topicName}";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(urlWithParameters);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                CloudTrail cloudTrail = JsonConvert.DeserializeObject<CloudTrail>(responseBody);

                return cloudTrail;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        protected async Task<bool> AlreadyExists(AnomalyDetectionResult anomalyDetectionResult)
        {
            return await _dbDriver.GetAnomalyDetectionResultAsync(anomalyDetectionResult) != null;
        }
    }
}
