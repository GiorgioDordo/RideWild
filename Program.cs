
using Microsoft.EntityFrameworkCore;
using RideWild.DataModels;
using RideWild.Models;
using System.Text.Json.Serialization;

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
