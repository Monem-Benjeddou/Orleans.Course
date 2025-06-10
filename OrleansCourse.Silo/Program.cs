using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrleansCourse.Silo.Startup;

try
{
    using IHost host = await StartSiloAsync();
    Console.WriteLine("\n\n Press Enter to terminate...\n\n");
    Console.ReadLine();

    await host.StopAsync();
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return 1;
}

static async Task<IHost> StartSiloAsync()
{
    var builder = Host.CreateDefaultBuilder();

    builder.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    });

    builder.ConfigureLogging(logging =>
    {
        logging.AddConsole();
    });

    builder.UseOrleans((context, silo) =>
    {
        var connectionString = context.Configuration.GetConnectionString("OrleansCluster")
                               ?? throw new InvalidOperationException("Connection string 'OrleansCluster' not found.");

        var siloIPAddress = GetLocalIPAddress();

        silo.ConfigureEndpoints(
            siloPort: 11111,
            gatewayPort: 30000,
            listenOnAnyHostAddress: false,
            advertisedIP: siloIPAddress
        );

        silo.UseAdoNetClustering(options =>
            {
                options.ConnectionString = connectionString;
                options.Invariant = "Npgsql";
            })
            .AddAdoNetGrainStorage("ClassStorage", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddAdoNetGrainStorage("StudentStorage", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddAdoNetGrainStorage("UserStorage", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddAdoNetGrainStorage("ClassRegistryStorage", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddAdoNetGrainStorage("studentRegistryStore", options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddStartupTask<StartupSeeder>();
    });

    var host = builder.Build();
    await host.RunAsync();
    return host;
}

static IPAddress GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
        {
            return ip;
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}
