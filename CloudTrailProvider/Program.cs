using CloudTrailProvider;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await Console.Out.WriteLineAsync(DateTime.UtcNow.ToLongTimeString());
        CloudTrailLoadProvider cloudTrailLoadProvider = new CloudTrailLoadProvider();

        await Task.Delay(90000);

        Task task1 = cloudTrailLoadProvider.ProvideAsync(100_000);
        Task task2 = cloudTrailLoadProvider.ProvideAsync(100_000);

        await Task.WhenAll(task1, task2);
        await Console.Out.WriteLineAsync(DateTime.UtcNow.ToLongTimeString());
    }
}