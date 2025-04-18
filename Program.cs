
using Microsoft.EntityFrameworkCore;
using RideWild.Models;

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

            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorkLT2019")));

            builder.Services.AddDbContext<AdventureWorkDataContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksDatas")));


            var app = builder.Build();



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
