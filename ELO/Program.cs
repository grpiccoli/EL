using System;
using System.IO;
using System.Net;
using ELO.Data;
using ELO.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ELO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                CountriesInitializer.Initialize(context);
                LocationsInitializer.Initialize(context);
                ArrivalsInitializer.Initialize(context);
                ExportsInitializer.Initialize(context);
                StationsInitializer.Initialize(context);
                InstitutionsInitializer.Initialize(context);
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseKestrel(options =>
            {
                string os = Environment.OSVersion.Platform.ToString();

                options.Limits.MaxConcurrentConnections = 100;
                options.Limits.MaxConcurrentUpgradedConnections = 100;
                //options.Limits.MaxRequestBodySize = 20_000_000;
                //options.Limits.MinRequestBodyDataRate =
                //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                //options.Limits.MinResponseDataRate =
                //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                //options.Listen(IPAddress.Parse("127.0.0.5"), 5050);
                //options.Listen(IPAddress.Parse("127.0.0.5"), 5051, listenOptions =>
                //{
                //    listenOptions.UseHttps(os == "Win32NT" ?
                //        Path.Combine(
                //            Directory.GetCurrentDirectory(),
                //            "sslforfree/elochile.pfx") : "/media/guillermo/WD3DNAND-SSD-1TB/certs/elochile.pfx", "34#$ERer");
                //});
            })
            .UseUrls("http://localhost:5005/")
            .UseStartup<Startup>();
    }
}
