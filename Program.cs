
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RideWild.DataModels;
using RideWild.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Data;
using System.Diagnostics;
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
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();



            var app = builder.Build();

            app.UseCors("CORSPolicy");

            // middleware exception handler
            app.UseExceptionHandler(errApp =>
            {
                // Specifica cosa fare quando si verifica un'eccezione
                errApp.Run(async ctx =>
                {
                    // Ottiene l'oggetto che contiene informazioni sull'eccezione
                    var feat = ctx.Features.Get<IExceptionHandlerFeature>();

                    // Se è presente un'eccezione
                    if (feat != null)
                    {
                        // Registra l'errore con Serilog
                        Serilog.Log.Error(feat.Error, "Unhandled exception on {Path}", ctx.Request.Path);
                    }

                    // Imposta il codice di risposta HTTP a 500 (Internal Server Error)
                    ctx.Response.StatusCode = 500;

                    // Scrive una risposta semplificata al client
                    await ctx.Response.WriteAsync("Internal server error.");
                });
            });

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
