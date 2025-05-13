
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RideWild.Models.DataModels;
using RideWild.Interfaces;
using RideWild.Models.AdventureModels;
using RideWild.Services;
using System.Text;
using System.Text.Json.Serialization;
using Serilog;
using RideWild.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Security.Claims;
using RideWild.Models.MongoModels;

namespace RideWild
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ReviewsDbConfig>(
                builder.Configuration.GetSection("ReviewsCollectionDB"));
            builder.Services.AddSingleton<ReviewsDbConfig>();

            JwtSettings jwtSettings = new();
            jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        RoleClaimType = ClaimTypes.Role,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, "Admin");
                });
            });

            builder.Services.AddControllers();

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

            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            //serilog configuration
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            var app = builder.Build();

            app.UseCors("CORSPolicy");

            // middleware exception handler
            app.UseExceptionHandler(errApp =>
            {
                errApp.Run(async ctx =>
                {
                    var feat = ctx.Features.Get<IExceptionHandlerFeature>();

                    if (feat != null)
                    {
                        Serilog.Log.Error(feat.Error, "Unhandled exception on {Path}", ctx.Request.Path);
                    }

                    ctx.Response.StatusCode = 500;

                    await ctx.Response.WriteAsync("Internal server error.");
                });
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}