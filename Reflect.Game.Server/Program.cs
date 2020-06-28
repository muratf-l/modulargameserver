using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reflect.Game.Ludo.Engine;
using Reflect.GameServer.Data.Models.Services;
using Reflect.GameServer.Database.Postgresql;
using Reflect.GameServer.Database.Postgresql.Context;
using Reflect.GameServer.Database.Postgresql.Services;
using Reflect.GameServer.GameManager;
using Reflect.GameServer.Library.Logging;
using Reflect.GameServer.Library.SocketLibrary;

namespace Reflect.GameServer
{
    internal class Program
    {
        public static ServiceProvider ServiceProvider;

        private static void Main(string[] args)
        {
            IConfiguration configuration;

#if DEBUG
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings-dev.json", true, true)
                .Build();
#else
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings-pro.json", optional: true, reloadOnChange: true)
                .Build();
#endif
            
            ServiceProvider = new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton<IPostgresqlDbContext, PostgresqlDbContext>()
                .AddScoped<IUserService, UserService>()
                .BuildServiceProvider();

            var port = int.Parse(configuration["Port"]);

            LogService.WriteDebug($"Server starting...{port}");

            var host1 = new GameHost();

            host1.Setup(typeof(GameCode), port, ConnectionMode.Server);

            while (!host1.IsRunning) host1.Start();

            LogService.WriteDebug("Done!");

            while (host1.IsRunning) Thread.Sleep(20);

            LogService.WriteDebug("Server stopping...");

            host1.Stop();

            LogService.WriteDebug("Done!");
        }
    }
}