using Cassandra;
using Common.Contracts;
using Common.Interfaces;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace CoudTrailIngestor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudTrailIngestorController : ControllerBase
    {
        private Common.Interfaces.ILogger _logger;
        private IMemoryCacheClient _cache;
        private ICassandraDBDriver _cassandraDBDriver;

        private TimeSpan _cacheTTL = TimeSpan.FromHours(1);

        private ProducerConfig _config;
        private string _topic = "cloudtrailtopic";
        private IProducer<string, string> _producer;

        private ConcurrentDictionary<string, Task> _tasks;

        public CloudTrailIngestorController(Common.Interfaces.ILogger logger, IMemoryCacheClient cache, IProducer<string, string> producer, ICassandraDBDriver cassandraDBDriver)
        {
            _logger = logger;
            _cache = cache;
            _producer = producer;
            _cassandraDBDriver = cassandraDBDriver;

            _tasks = new();
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

            string eventIdentifier = $"{cloudTrail.EventId}.{cloudTrail.EventType}";

            var inCache = _cache.Get(eventIdentifier);

            if (inCache != null)
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
                if (await _cassandraDBDriver.WriteIfNotExists(eventIdentifier))
                {
                    _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", $"Added to cassandra and kafka: {eventIdentifier}");
                    await EnqueueAsync(cloudTrail);
                }
                else
                {
                    _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", $"Found in Cassandra, deduped: {eventIdentifier}");
                }

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
                var message = new Message<string, string> { Key = $"{cloudTrail.EventId}.{cloudTrail.EventType}", Value = JsonConvert.SerializeObject(cloudTrail) };

                Task<Task<DeliveryResult<string, string>?>> task = _producer.ProduceAsync(_topic, message)
                                   .ContinueWith(async t =>
                                   {
                                       if (!t.IsFaulted && t.Result.Status == PersistenceStatus.Persisted)
                                       {
                                           //_logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", $"Delivered message to {t.Result.TopicPartitionOffset}");

                                           int removeRetry = 0;
                                           while (removeRetry < 10 && !_tasks.TryRemove(cloudTrail.EventId + cloudTrail.EventType.ToString(), out Task done))
                                           {
                                               removeRetry++;
                                               await Task.Delay(10);
                                           }

                                           //TODO: handle cant remove from dictionary

                                           if (_tasks.Count == 0)
                                           {
                                               //_logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", $"Delivery report queue is empty!");
                                           }

                                           return t.Result;
                                       }
                                       else
                                       {
                                           //_logger.Warn($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", $"Failed to deliver message: {t.Exception?.Message ?? t.Result.Status.ToString()}");
                                           // TODO: Implement retry
                                           return null;
                                       }
                                   });

                _tasks[cloudTrail.EventId + cloudTrail.EventType.ToString()] = task;
                _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", $"Message added to send task list");
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
