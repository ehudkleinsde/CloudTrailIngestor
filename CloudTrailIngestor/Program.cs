using Cache;
using Logging;
using SimpleInjector;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;
        services.AddHttpClient();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //### DI ###
        var container = new SimpleInjector.Container();

        services.AddSimpleInjector(container, options =>
        {
            options.AddAspNetCore().AddControllerActivation();
        });

        container.Register<Common.Interfaces.IMemoryCacheClient, MemoryCacheClient>(Lifestyle.Singleton);
        container.Register<Common.Interfaces.ILogger>(() => new Serilogger("CloudTrailIngestor"), Lifestyle.Singleton);
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

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}