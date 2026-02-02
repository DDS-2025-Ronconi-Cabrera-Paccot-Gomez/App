using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Net.Http; // <--- Necesario para HttpClient
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.Libs;

namespace TravelPro;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting TravelPro.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration
#if DEBUG
                        .MinimumLevel.Debug()
#else
                        .MinimumLevel.Information()
#endif
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Async(c => c.File("Logs/logs.txt"))
                        .WriteTo.Async(c => c.Console())
                        .WriteTo.Async(c => c.AbpStudio(services));
                });
            await builder.AddApplicationAsync<TravelProHttpApiHostModule>();
            builder.Services.Configure<AbpMvcLibsOptions>(options =>
            {
                options.CheckLibs = false;
            });
            var app = builder.Build();
            await app.InitializeApplicationAsync();

            // =================================================================
            // 🛠️ BLOQUE DE DIAGNÓSTICO: PRUEBA DE API EXTERNA (GEO DB) 🛠️
            // =================================================================
            // Este código se ejecuta al iniciar para mostrar el JSON real en consola.
            try
            {
                Log.Information("\n⬇️⬇️⬇️ INICIANDO CONSULTA DE PRUEBA A GEODB ⬇️⬇️⬇️");

                using (var client = new HttpClient())
                {
                    // Usamos tu Key actual
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "1b87288382msh04081de1250362fp1acf94jsn6c66e7e31d14");
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com");

                    // Buscamos 'Buenos Aires' para ver si trae la región
                    string testUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo/cities?namePrefix=Buenos%20Aires&limit=2&sort=-population&languageCode=en";

                    Log.Information($"🌍 URL Consultada: {testUrl}");

                    var response = await client.GetAsync(testUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        Log.Information("📦 JSON CRUDO RECIBIDO:");
                        Log.Information(json); // <--- ¡AQUÍ APARECERÁ EL JSON EN LA CONSOLA!
                    }
                    else
                    {
                        Log.Error($"❌ Error API: {response.StatusCode}");
                    }
                }
                Log.Information("⬆️⬆️⬆️ FIN CONSULTA DE PRUEBA ⬆️⬆️⬆️\n");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Falló la prueba manual de la API.");
            }
            // =================================================================

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}