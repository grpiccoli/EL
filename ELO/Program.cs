using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ELO
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
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
