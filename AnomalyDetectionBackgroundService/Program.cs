using AnomalyDetection;
using AnomalyDetection.AnomalyDetections;
using Common.Interfaces;
using MongoDB;
using Logging;
using SimpleInjector;

internal class Program
{
    private static string _mongoDBConnStr = "mongodb://localmongodb:27017";

    private static async Task Main(string[] args)
    {
        var container = new SimpleInjector.Container();

        var mongoDB = new MongoDB.MongoDBDriver(_mongoDBConnStr, "AnomalyDetectionResult");
        var serilogger = new Serilogger("AnomalyDetectionBackgroundService");

        AnomalyDetectionWorker1 anomalyDetectionWorker1_0 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_1 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_2 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_3 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_4 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_5 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_6 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_7 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_8 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_9 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_10 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_11 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_12 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_13 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_14 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_15 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_16 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_17 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_18 = new(serilogger, mongoDB);
        AnomalyDetectionWorker1 anomalyDetectionWorker1_19 = new(serilogger, mongoDB);

        AnomalyDetectionWorker2 anomalyDetectionWorker2_0 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_1 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_2 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_3 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_4 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_5 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_6 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_7 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_8 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_9 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_10 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_11 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_12 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_13 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_14 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_15 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_16 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_17 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_18 = new(serilogger, mongoDB);
        AnomalyDetectionWorker2 anomalyDetectionWorker2_19 = new(serilogger, mongoDB);

        container.RegisterInstance<IDBDriver>(mongoDB);
        container.RegisterInstance<ILogger>(serilogger);

        IAnomalyDetectionWorker[] workes = new IAnomalyDetectionWorker[]
        {
            anomalyDetectionWorker1_0,
            anomalyDetectionWorker1_1,
            anomalyDetectionWorker1_2,
            anomalyDetectionWorker1_3,
            anomalyDetectionWorker1_4,
            anomalyDetectionWorker1_5,
            anomalyDetectionWorker1_6,
            anomalyDetectionWorker1_7,
            anomalyDetectionWorker1_8,
            anomalyDetectionWorker1_9,
            anomalyDetectionWorker1_10,
            anomalyDetectionWorker1_11,
            anomalyDetectionWorker1_12,
            anomalyDetectionWorker1_13,
            anomalyDetectionWorker1_14,
            anomalyDetectionWorker1_15,
            anomalyDetectionWorker1_16,
            anomalyDetectionWorker1_17,
            anomalyDetectionWorker1_18,
            anomalyDetectionWorker1_19,

            anomalyDetectionWorker2_0,
            anomalyDetectionWorker2_1,
            anomalyDetectionWorker2_2,
            anomalyDetectionWorker2_3,
            anomalyDetectionWorker2_4,
            anomalyDetectionWorker2_5,
            anomalyDetectionWorker2_6,
            anomalyDetectionWorker2_7,
            anomalyDetectionWorker2_8,
            anomalyDetectionWorker2_9,
            anomalyDetectionWorker2_10,
            anomalyDetectionWorker2_11,
            anomalyDetectionWorker2_12,
            anomalyDetectionWorker2_13,
            anomalyDetectionWorker2_14,
            anomalyDetectionWorker2_15,
            anomalyDetectionWorker2_16,
            anomalyDetectionWorker2_17,
            anomalyDetectionWorker2_18,
            anomalyDetectionWorker2_19,

        };

        container.RegisterInstance<IAnomalyDetectionWorker[]>(workes);
        container.Register<AnomalyDetectionService>(Lifestyle.Singleton);

        container.Verify();

        var service = container.GetInstance<AnomalyDetectionService>();
        await service.RunAsync();
    }
}