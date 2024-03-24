using Common.Contracts;
using Common.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace CloudTrailProvider
{
    internal class CloudTrailLoadProvider : ICloudTrailLoadProvider
    {
        private Uri _messageQueueUri = new Uri("http://cloudtrailingestor:8080/CloudTrailIngestor");

        private Random _rnd;

        public CloudTrailLoadProvider()
        {
            _rnd = new Random();
        }

        public async Task ProvideAsync(int amount)
        {
            await Task.Delay(5000);

            int provided = 0;
            CloudTrail random;
            HttpRequestMessage request;
            HttpClient _httpClient = new HttpClient();

            while (provided < amount)
            {
                provided++;

                random = GetRandomCloudTrail();
                request = new HttpRequestMessage(HttpMethod.Post, _messageQueueUri) { Content = new StringContent(JsonConvert.SerializeObject(random), Encoding.UTF8, "application/json") };

                try
                {
                    await _httpClient.SendAsync(request);
                    //await Console.Out.WriteLineAsync("Sent newly random generated CloudTrailEvent");
                }
                catch (Exception ex)
                {
                }
            }
        }

        private CloudTrail GetRandomCloudTrail()
        {
            int rnd = _rnd.Next(1, 5);

            var cloudTrail = new CloudTrail()
            {
                AffectedAssets = GenerateRandomStringArray(),
                EventId = Guid.NewGuid(),
                EventType = rnd < 5 ? EventType.Read : EventType.Create,
                RequestId = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                TimestampUTC = DateTime.UtcNow,
            };

            return cloudTrail;
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
