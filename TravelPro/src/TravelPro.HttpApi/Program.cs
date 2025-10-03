﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TravelPro.TravelProGeo;
using TestGeodbCities.Infrastructure;

namespace TravelPro.HttpApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configuramos el contenedor de dependencias
            var services = new ServiceCollection();
            services.AddTransient<ITravelProService, CitySearchService>();
            var provider = services.BuildServiceProvider();

            // Obtenemos la instancia del servicio
            var citySearchService = provider.GetRequiredService<ITravelProService>();

            Console.WriteLine("Ingrese el nombre de la ciudad a buscar:");
            string cityName = Console.ReadLine();

            var cities = await citySearchService.SearchCitiesByNameAsync(cityName);

            if (cities.Count == 0)
            {
                Console.WriteLine("No se encontraron ciudades.");
            }
            else
            {
                Console.WriteLine("Resultados:");
                foreach (var city in cities)
                {
                    Console.WriteLine($"{city.Name} ({city.Country}) - Población: {city.Population}");
                }
            }
        }
    }
}
