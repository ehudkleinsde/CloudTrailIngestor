using Common.Contracts;
using Common.Interfaces;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoudTrailIngestor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudTrailIngestorController : ControllerBase
    {
        private Common.Interfaces.ILogger _logger;
        private IMemoryCacheClient _cache;

        private TimeSpan _cacheTTL = TimeSpan.FromHours(1);

        private static string kafkaBootstrap = "localhost";

        private ProducerConfig _config;
        private IProducer<Null, string> _producer;
        private string _topic = "cloudtrailtopic";

        public CloudTrailIngestorController(Common.Interfaces.ILogger logger, IMemoryCacheClient cache)
        {
            _logger = logger;
            _cache = cache;

            _config = new ProducerConfig
            {
                BootstrapServers = $"{kafkaBootstrap}:9092",
            };

            _producer = new ProducerBuilder<Null, string>(_config).Build();
        }

        [HttpPost(Name = nameof(PostCloudTrailAsync))]
        public async Task<IActionResult> PostCloudTrailAsync([FromBody] CloudTrail cloudTrail)
        {
            _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", JsonConvert.SerializeObject(cloudTrail));

            if (!ModelState.IsValid)
            {
                _logger.Error($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", JsonConvert.SerializeObject(cloudTrail));
                return BadRequest(ModelState);
            }

            string eventIdentifier = $"{cloudTrail.EventType}.{cloudTrail.EventId}";
            if (_cache.Get(eventIdentifier) != null)
            {
                _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", $"Found in cache: {eventIdentifier}");
                return Ok();
            }
            else
            {
                _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", $"Added to cache: {eventIdentifier}");
                _cache.Set(eventIdentifier, cloudTrail, _cacheTTL);
            }

            try
            {
                await EnqueueAsync(cloudTrail);
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", JsonConvert.SerializeObject(cloudTrail), ex);
                return StatusCode(500);
            }

            return Ok();
        }

        private async Task EnqueueAsync(CloudTrail cloudTrail)
        {
            try
            {
                var message = new Message<Null, string> { Value = JsonConvert.SerializeObject(cloudTrail) };
                DeliveryResult<Null, string> deliveryReport = await _producer.ProduceAsync(_topic, message);
                _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", $"Message sent to topic");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.Error($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", $"{e.Error.Reason}");
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", JsonConvert.SerializeObject(cloudTrail), ex);
            }
        }
    }
}
