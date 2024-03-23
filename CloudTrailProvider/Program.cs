using CloudTrailProvider;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await Console.Out.WriteLineAsync(DateTime.UtcNow.ToLongTimeString());
        CloudTrailLoadProvider cloudTrailLoadProvider = new CloudTrailLoadProvider();

        await Task.Delay(60000);

        Task task1 = cloudTrailLoadProvider.ProvideAsync(500_000);
        Task task2 = cloudTrailLoadProvider.ProvideAsync(500_000);

        await Task.WhenAll(task1, task2);
        await Console.Out.WriteLineAsync(DateTime.UtcNow.ToLongTimeString());
    }
}