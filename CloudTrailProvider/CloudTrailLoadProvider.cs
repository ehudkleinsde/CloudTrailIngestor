using Common.Contracts;
using Common.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace CloudTrailProvider
{
    internal class CloudTrailLoadProvider : ICloudTrailLoadProvider
    {
        private Uri _messageQueueUri = new Uri("http://cloudtrailmessagebroker:8080/CloudTrailMessageBroker/EnqueueAsync");
        private HttpClient _httpClient;
        private Random _rnd;

        public CloudTrailLoadProvider()
        {
            _httpClient = new HttpClient();
            _rnd = new Random();
        }

        public async Task Provide(int amount)
        {
            int provided = 0;
            CloudTrail random;
            HttpRequestMessage request;
            string responseContent;

            while (provided < amount)
            {
                provided++;

                random = GetRandomCloudTrail();
                request = new HttpRequestMessage(HttpMethod.Post, _messageQueueUri) { Content = new StringContent(JsonConvert.SerializeObject(random), Encoding.UTF8, "application/json") };

                try
                {
                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private CloudTrail GetRandomCloudTrail()
        {
            int rnd = _rnd.Next(1, 5);

            return new() {
                AffectedAssets = GenerateRandomStringArray(),
                EventId = Guid.NewGuid(),
                EventType = rnd < 5 ? EventType.Read : EventType.Create,
                RequestId = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                TimestampUTC = DateTime.UtcNow,
            };
        }

        private string[] GenerateRandomStringArray()
        {
            int arrayLength = _rnd.Next(1, 6);
            string[] randomStrings = new string[arrayLength];

            for (int i = 0; i < arrayLength; i++)
            {
                randomStrings[i] = GenerateRandomString(_rnd.Next(1, 6));
            }

            return randomStrings;
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[length];
            }

            return new String(stringChars);
        }
    }
}
