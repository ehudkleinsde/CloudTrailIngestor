using Common.Contracts;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace CoudTrailIngestor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudTrailIngestorController : ControllerBase
    {
        private Uri _messageQueueUri;
        private readonly IHttpClientFactory _clientFactory;
        private Common.Interfaces.ILogger _logger;
        private IMemoryCacheClient _cache;

        private TimeSpan _cacheTTL = TimeSpan.FromHours(1);

        public CloudTrailIngestorController(IHttpClientFactory clientFactory, Common.Interfaces.ILogger logger, IMemoryCacheClient cache)
        {
            _clientFactory = clientFactory;
            _messageQueueUri = new Uri("http://cloudtrailmessagebroker:8080/CloudTrailMessageBroker/EnqueueAsync");
            _logger = logger;
            _cache = cache;
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
                return await EnqueueAsync(cloudTrail);
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", JsonConvert.SerializeObject(cloudTrail), ex);
                return StatusCode(500);
            }
        }

        private async Task<IActionResult> EnqueueAsync(CloudTrail cloudTrail)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _messageQueueUri) { Content = new StringContent(JsonConvert.SerializeObject(cloudTrail), Encoding.UTF8, "application/json") };
            var client = _clientFactory.CreateClient();

            try
            {
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", responseContent);
                    return Content(responseContent, "application/json");
                }

                _logger.Info($"{nameof(CloudTrailIngestorController)}.{nameof(PostCloudTrailAsync)}", $"{response.StatusCode}, {response.ReasonPhrase}");
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(CloudTrailIngestorController)}.{nameof(EnqueueAsync)}", JsonConvert.SerializeObject(cloudTrail), ex);
                return StatusCode(500);
            }
        }
    }
}
