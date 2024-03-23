using Cache;
using Confluent.Kafka;
using Logging;
using SimpleInjector;

internal class Program
{
    private static string kafkaBootstrap = "kafka";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var config = new ProducerConfig
        {
            BootstrapServers = $"{kafkaBootstrap}:9092",
            //AllowAutoCreateTopics = true,
        };

        IProducer<string, string> producer = new ProducerBuilder<string, string>(config).Build();

        //### DI ###
        var container = new SimpleInjector.Container();

        services.AddSimpleInjector(container, options =>
        {
            options.AddAspNetCore().AddControllerActivation();
        });

        container.Register<Common.Interfaces.IMemoryCacheClient, MemoryCacheClient>(Lifestyle.Singleton);
        container.Register<Common.Interfaces.ILogger>(() => new Serilogger("CloudTrailIngestor"), Lifestyle.Singleton);
        container.Register<IProducer<string, string>>(() => producer, Lifestyle.Singleton);

        //### End DI ###


        var app = builder.Build();
        app.Services.UseSimpleInjector(container);

        container.Verify();

        // Configure the HTTP request pipeline.
        /*if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }*/

        //TODO: remove swagger on prod env
        app.UseSwagger();
        app.UseSwaggerUI();

        //app.UseHttpsRedirection();

        //app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}