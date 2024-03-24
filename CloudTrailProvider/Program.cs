using CloudTrailProvider;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        CloudTrailLoadProvider cloudTrailLoadProvider = new CloudTrailLoadProvider();

        await Console.Out.WriteLineAsync("Waiting 30s for Docker services to load...");
        await Task.Delay(30000);
        await Console.Out.WriteLineAsync($"Generating  200k random CloudTrail events");
        Stopwatch stopwatch = Stopwatch.StartNew();
        Task task1 = cloudTrailLoadProvider.ProvideAsync(100_000);
        Task task2 = cloudTrailLoadProvider.ProvideAsync(100_000);
        await Task.WhenAll(task1, task2);
        stopwatch.Stop();
        await Console.Out.WriteLineAsync($"Done. Took {stopwatch.Elapsed.TotalSeconds} seconds to push {200}k random CloudTrail events. Throughput is {200_000.0/ stopwatch.Elapsed.TotalSeconds} events/second");
    }
}