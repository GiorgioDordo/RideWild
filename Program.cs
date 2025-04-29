
using Microsoft.EntityFrameworkCore;
using RideWild.DataModels;
using RideWild.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Data;
using System.Text.Json.Serialization;
using static Serilog.Sinks.MSSqlServer.ColumnOptions;

namespace RideWild
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddControllers().AddJsonOptions(jsOpt =>
                jsOpt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed(origin => true);
                });
            });

            builder.Services.AddDbContext<AdventureWorksDataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksData")));

            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksLT2019")));

            //serilog configuration
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Async(a => a.MSSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("AdventureWorksData"),
                    tableName: "Logs",
                    autoCreateSqlTable: false,
                    batchPostingLimit: 1,
                    period: TimeSpan.FromSeconds(5)
                ))
                .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();
            app.UseCors("CORSPolicy");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
