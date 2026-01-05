using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPro.Users.Dtos;
using Volo.Abp.Application.Services;

namespace TravelPro.Users
{
    // Esta es TU interfaz personalizada para agregar cosas nuevas al perfil
    public interface ITravelProProfileAppService : IApplicationService
    {
        Task DeleteAsync();
        Task<PublicProfileDto> GetPublicProfileAsync(Guid userId);
        Task<PublicProfileDto> GetPublicProfileByUserNameAsync(string userName);
        Task<List<PublicProfileDto>> SearchAsync(string filter);
    }
}