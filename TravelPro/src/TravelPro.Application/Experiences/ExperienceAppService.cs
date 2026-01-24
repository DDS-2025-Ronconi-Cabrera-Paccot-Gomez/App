using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Users;
using TravelPro.Experiences.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Data; // Para IDataFilter
using Volo.Abp.ObjectMapping;


namespace TravelPro.Experiences
{
    [Authorize]
    public class ExperienceAppService : CrudAppService<
        Experience,
        ExperienceDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateExperienceDto>,
        IExperienceAppService
    {
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IDataFilter _dataFilter;
        private readonly IObjectMapper _objectMapper;
        public ExperienceAppService(IObjectMapper objectMapper,
            IRepository<Experience, Guid> repository,
            IRepository<IdentityUser, Guid> userRepository,
            IDataFilter dataFilter)
            : base(repository)
        {
            _userRepository = userRepository;
            _dataFilter = dataFilter;
            _objectMapper = objectMapper;
        }

        // --- 4.1 CREAR EXPERIENCIA ---
        public override async Task<ExperienceDto> CreateAsync(CreateUpdateExperienceDto input)
        {
            var experience = ObjectMapper.Map<CreateUpdateExperienceDto, Experience>(input);
            experience.DestinationId = input.DestinationId; // Aseguramos el ID
            if (CurrentUser.Id.HasValue)
            {
                experience.UserId = CurrentUser.Id.Value;
            }
            else
            {
                throw new UserFriendlyException("Debes estar logueado para crear una experiencia.");
            }
            // Asignamos el creador actual explícitamente (aunque ABP lo hace auto, es buena práctica si usamos lógica custom)
            // Nota: FullAuditedEntity lo llena solo si estamos autenticados.

            await Repository.InsertAsync(experience, autoSave: true);

            return await MapToGetOutputDtoAsync(experience);
        }

        // --- 4.2 EDITAR EXPERIENCIA PROPIA ---
        public override async Task<ExperienceDto> UpdateAsync(Guid id, CreateUpdateExperienceDto input)
        {
            var experience = await Repository.GetAsync(id);

            // Verificamos propiedad
            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new UserFriendlyException("No tienes permiso para editar esta experiencia.");
            }

            // Actualizamos campos
            experience.Title = input.Title;
            experience.Description = input.Description;
            experience.Sentiment = input.Sentiment;
            experience.Cost = input.Cost;
            experience.ExperienceDate = input.ExperienceDate;
            experience.Tags = input.Tags;

            await Repository.UpdateAsync(experience, autoSave: true);

            return await MapToGetOutputDtoAsync(experience);
        }

        // --- 4.3 ELIMINAR EXPERIENCIA PROPIA ---
        public override async Task DeleteAsync(Guid id)
        {
            var experience = await Repository.GetAsync(id);

            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new UserFriendlyException("No tienes permiso para eliminar esta experiencia.");
            }

            await Repository.DeleteAsync(experience);
        }

        // --- 4.4 CONSULTAR EXPERIENCIAS DE UN DESTINO ---
        [AllowAnonymous]
        public async Task<List<ExperienceDto>> GetListByDestinationAsync(Guid destinationId)
        {
            // Desactivamos el filtro para ver las de OTROS usuarios
            using (_dataFilter.Disable<IUserOwned>())
            {
                var query = await Repository.GetQueryableAsync();
                var experiences = await AsyncExecuter.ToListAsync(
                    query.Where(e => e.DestinationId == destinationId)
                         .OrderByDescending(e => e.CreationTime)
                );

                return await EnrichWithUserNamesAsync(experiences);
            }
        }

        // --- 4.5 FILTRAR POR VALORACIÓN (Sentimiento) ---
        [AllowAnonymous]
        public async Task<List<ExperienceDto>> GetListBySentimentAsync(Guid destinationId, ExperienceSentiment sentiment)
        {
            using (_dataFilter.Disable<IUserOwned>())
            {
                var query = await Repository.GetQueryableAsync();
                var experiences = await AsyncExecuter.ToListAsync(
                    query.Where(e => e.DestinationId == destinationId && e.Sentiment == sentiment)
                         .OrderByDescending(e => e.CreationTime)
                );

                return await EnrichWithUserNamesAsync(experiences);
            }
        }

        // --- 4.6 BUSCAR POR PALABRAS CLAVE ---
        [AllowAnonymous]
        public async Task<List<ExperienceDto>> SearchByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<ExperienceDto>();

            var normalizedKeyword = keyword.Trim().ToLower();

            using (_dataFilter.Disable<IUserOwned>())
            {
                var query = await Repository.GetQueryableAsync();

                // Buscamos en Título, Descripción o Tags
                var experiences = await AsyncExecuter.ToListAsync(
                    query.Where(e =>
                        e.Title.ToLower().Contains(normalizedKeyword) ||
                        e.Description.ToLower().Contains(normalizedKeyword) ||
                        e.Tags.ToLower().Contains(normalizedKeyword))
                         .OrderByDescending(e => e.CreationTime)
                );

                return await EnrichWithUserNamesAsync(experiences);
            }
        }

        // --- MÉTODO AUXILIAR PARA OBTENER NOMBRES DE USUARIO ---
        private async Task<List<ExperienceDto>> EnrichWithUserNamesAsync(List<Experience> experiences)
        {
            if (!experiences.Any()) return new List<ExperienceDto>();

            var userIds = experiences.Where(x => x.CreatorId.HasValue)
                                     .Select(x => x.CreatorId.Value)
                                     .Distinct()
                                     .ToList();

            var users = await _userRepository.GetListAsync(u => userIds.Contains(u.Id));
            var userDict = users.ToDictionary(u => u.Id, u => u.UserName);

            var dtos = ObjectMapper.Map<List<Experience>, List<ExperienceDto>>(experiences);

            foreach (var dto in dtos)
            {
                if (dto.CreatorId.HasValue && userDict.ContainsKey(dto.CreatorId.Value))
                {
                    dto.UserName = userDict[dto.CreatorId.Value];
                }
                else
                {
                    dto.UserName = "Usuario Desconocido";
                }
            }

            return dtos;
        }
    }
}
