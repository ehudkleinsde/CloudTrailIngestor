using Newtonsoft.Json;
using System.Text;

namespace CloudTrailProvider
{
    internal class CloudTrailLoadProvider
    {
        private Uri _messageQueueUri = new Uri("http://cloudtrailmessagebroker:8080/CloudTrailMessageBroker/EnqueueAsync");
        private HttpClient _httpClient;

        public CloudTrailLoadProvider()
        {
            _httpClient = new HttpClient();
        }

        public async Task Provide()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _messageQueueUri) { Content = new StringContent(JsonConvert.SerializeObject(cloudTrail), Encoding.UTF8, "application/json") };
            

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
