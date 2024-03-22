using Common.Contracts;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MessageBroker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudTrailMessageBrokerController : ControllerBase
    {
        private Dictionary<string, ConcurrentQueue<CloudTrail>> _topics;
        private Common.Interfaces.ILogger _logger;

        public CloudTrailMessageBrokerController(Dictionary<string, ConcurrentQueue<CloudTrail>> topics, Common.Interfaces.ILogger logger)
        {
            _logger = logger;
            _topics = topics;
        }

        [HttpPost("EnqueueAsync", Name = nameof(EnqueueAsync))]
        public async Task<IActionResult> EnqueueAsync([FromBody] CloudTrail message)
        {
            _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(EnqueueAsync)}", JsonConvert.SerializeObject(message));

            if (!ModelState.IsValid)
            {
                _logger.Error(nameof(EnqueueAsync), JsonConvert.SerializeObject(message));
                return BadRequest(ModelState);
            }

            try
            {
                foreach (var topic in _topics.Values)
                {
                    topic.Enqueue(message);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailMessageBrokerController)}.{nameof(EnqueueAsync)}", JsonConvert.SerializeObject(message), ex);
                return StatusCode(500);
            }
        }

        [HttpGet("CountAsync", Name = nameof(CountAsync))]
        public async Task<IActionResult> CountAsync(string topicName)
        {
            _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(CountAsync)}", "Start");

            try
            {
                if (_topics.ContainsKey(topicName))
                {
                    int queueSize = _topics[topicName].Count;
                    _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(CountAsync)}", $"Queue size: {queueSize}");
                    return Ok(queueSize);
                }
                else
                {
                    return NotFound(topicName);
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailMessageBrokerController)}.{nameof(CountAsync)}", "Error", ex);
                return StatusCode(500);
            }
        }

        [HttpGet("CountAllAsync", Name = nameof(CountAllAsync))]
        public async Task<IActionResult> CountAllAsync()
        {
            _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(CountAsync)}", "Start");

            Dictionary<string, int> counts = new Dictionary<string, int>();

            try
            {
                foreach (string topic in _topics.Keys)
                {
                    counts[topic] = _topics[topic].Count;
                }

                string result = JsonConvert.SerializeObject(counts);

                _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(CountAsync)}", $"{result}");
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailMessageBrokerController)}.{nameof(CountAsync)}", "Error", ex);
                return StatusCode(500);
            }
        }

        [HttpGet("PeekAsync", Name = nameof(PeekAsync))]
        public async Task<IActionResult> PeekAsync(string topicName)
        {
            _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(PeekAsync)}", "Start");

            try
            {
                if (!_topics.ContainsKey(topicName))
                {
                    return NotFound(topicName);
                }

                if (_topics[topicName].TryPeek(out CloudTrail message))
                {
                    _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(PeekAsync)}", JsonConvert.SerializeObject(message));
                    return Ok(message);
                }
                else
                {
                    _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(PeekAsync)}", "Queue is empty");
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailMessageBrokerController)}.{nameof(PeekAsync)}", "Error", ex);
                return StatusCode(500);
            }
        }

        [HttpGet("DequeueAsync", Name = nameof(DequeueAsync))]
        public async Task<IActionResult> DequeueAsync(string topicName)
        {
            _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(DequeueAsync)}", $"Topic Name: {topicName}");

            if (!_topics.ContainsKey(topicName))
            {
                return NotFound(topicName);
            }

            try
            {
                if (_topics[topicName].TryDequeue(out CloudTrail cloudTrail))
                {
                    _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(DequeueAsync)}", JsonConvert.SerializeObject(cloudTrail));
                    return Ok(cloudTrail);
                }
                else
                {
                    _logger.Info($"{nameof(CloudTrailMessageBrokerController)}.{nameof(DequeueAsync)}", "Queue is empty");
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailMessageBrokerController)}.{nameof(DequeueAsync)}", "Error", ex);
                return StatusCode(500);
            }
        }
    }
}
