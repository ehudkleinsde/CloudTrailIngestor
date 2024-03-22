using AnomalyDetection;
using AnomalyDetection.AnomalyDetections;
using Common.Interfaces;
using DBDriver;
using Logging;
using SimpleInjector;

internal class Program
{
    private static string _mongoDBConnStr = "mongodb://localmongodb:27017";//"mongodb://localhost:27018";

    private static async Task Main(string[] args)
    {
        var container = new SimpleInjector.Container();

        container.Register<IDBDriver>(() => new MongoDBDriver(_mongoDBConnStr, "AnomalyDetectionResult"), Lifestyle.Singleton);
        container.Register<ILogger>(() => new Serilogger("AnomalyDetectionBackgroundService"), Lifestyle.Singleton);

        container.Collection.Register<IAnomalyDetectionWorker>(new[]
        {
            typeof(AnomalyDetectionWorker1),
            typeof(AnomalyDetectionWorker2),
            typeof(AnomalyDetectionWorker3)
        });

        container.Register<AnomalyDetectionService>(Lifestyle.Transient);

        container.Verify();

        var service = container.GetInstance<AnomalyDetectionService>();
        await service.RunAsync();
    }
}