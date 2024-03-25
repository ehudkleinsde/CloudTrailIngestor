using CloudTrailProvider;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await Console.Out.WriteLineAsync("Waiting 30s for Docker services to load...");
        await Task.Delay(30000);
        await Console.Out.WriteLineAsync($"Generating  200k random CloudTrail events");
        Stopwatch stopwatch = Stopwatch.StartNew();

        List<Task> providers = new();

        int producers = 50;
        int events = 200000;

        for(int i = 0; i < producers; i++)
        {
            var provider = new CloudTrailLoadProvider();
            providers.Add(provider.ProvideAsync(events/producers));
        }

        await Task.WhenAll(providers);
        stopwatch.Stop();
        await Console.Out.WriteLineAsync($"Done. Took {stopwatch.Elapsed.TotalSeconds} seconds to push {200}k random CloudTrail events. Throughput is {200_000.0/ stopwatch.Elapsed.TotalSeconds} events/second");
    }
}