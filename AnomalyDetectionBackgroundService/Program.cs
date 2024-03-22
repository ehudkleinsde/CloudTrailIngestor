using AnomalyDetection;
using AnomalyDetection.AnomalyDetections;
using Common.Interfaces;
using DBDriver;
using Logging;
using SimpleInjector;

internal class Program
{
    private static string _mongoDBConnStr = "mongodb://localhost:27018";//"mongodb://localmongodb:27017";//

    private static async Task Main(string[] args)
    {
        var container = new SimpleInjector.Container();

        container.Register<IDBDriver>(() => new MongoDBDriver(_mongoDBConnStr, "AnomalyDetectionResult"), Lifestyle.Singleton);
        container.Register<ILogger>(() => new Serilogger("AnomalyDetectionBackgroundService"), Lifestyle.Singleton);

        container.Register<AnomalyDetectionWorker1>(Lifestyle.Singleton);
        container.Register<AnomalyDetectionWorker2>(Lifestyle.Singleton);
        container.Register<AnomalyDetectionWorker3>(Lifestyle.Singleton);


        container.Register<AnomalyDetectionService>(Lifestyle.Singleton);

        container.Verify();

        var service = container.GetInstance<AnomalyDetectionService>();
        await service.RunAsync();
    }
}