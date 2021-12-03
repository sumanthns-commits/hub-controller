using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Amazon;
using Amazon.DynamoDBv2;
using HubController.Services;
using HubController.Middleware;
using HubController.Repositories;

namespace HubController
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson();

            services.AddSwaggerGen();

            string region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.USEast1.SystemName;
            services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region)));
            
            // Add custom services
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IHubRepository, DynamoHubRespository>();
            services.AddSingleton<IHubPasswordRepository, DynamoHubPasswordRepository>();
            services.AddSingleton<IHubService, HubService>();
            services.AddSingleton<IThingIdGenerator, ThingIdGenerator>();
            services.AddSingleton<IThingService, ThingService>();
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IHubPasswordService, HubPasswordService>();

            // Auth is handled in api gateway. Provide a dummy authentication handler to handle
            // authrorization errors
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "forbidScheme";
                options.DefaultForbidScheme = "forbidScheme";
                options.AddScheme<NoopAuthenticationHandler>("forbidScheme", "Handle Forbidden");
            });

            var hubCreatorScope = Environment.GetEnvironmentVariable("HUB_CREATOR_SCOPE");
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HubCreator", policy => policy.RequireClaim("scope", hubCreatorScope));
            });

            // Add AutoMapper
            services.AddAutoMapper(typeof(AutoMappingProfile));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });

            app.UseSwagger();

            app.UseSwaggerUI();
        }
    }
}
