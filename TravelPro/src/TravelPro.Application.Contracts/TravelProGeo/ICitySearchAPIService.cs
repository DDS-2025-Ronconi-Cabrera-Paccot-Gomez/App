using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelPro.TravelProGeo
{
    public interface ICitySearchAPIService : Volo.Abp.DependencyInjection.ITransientDependency
    {
        /// <summary>
        /// Busca ciudades en la API GeoDB Cities por un prefijo de nombre.
        /// </summary>
        /// <param name="namePrefix">El prefijo del nombre de la ciudad a buscar.</param>
        /// <returns>Una lista de resultados que coinciden.</returns>
        Task<List<CitySearchResultDto>> SearchCitiesByNameAsync(string namePrefix);
    }
}
