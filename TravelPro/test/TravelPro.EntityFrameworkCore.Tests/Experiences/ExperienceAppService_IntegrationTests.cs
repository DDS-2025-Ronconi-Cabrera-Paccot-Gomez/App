using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelPro.Destinations;
using TravelPro.Experiences;
using TravelPro.Experiences.Dtos;
using TravelPro.EntityFrameworkCore.Tests.Fakes; // Asegúrate de tener tus fakes aquí
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Xunit;

namespace TravelPro.EntityFrameworkCore.Tests.Experiences
{
    public class ExperienceAppService_IntegrationTests : TravelProEntityFrameworkCoreTestBase
    {
        private readonly IExperienceAppService _experienceAppService;
        private readonly IRepository<Experience, Guid> _experienceRepository;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly ICurrentUser _currentUser;

        public ExperienceAppService_IntegrationTests()
        {
            _experienceAppService = GetRequiredService<IExperienceAppService>();
            _experienceRepository = GetRequiredService<IRepository<Experience, Guid>>();
            _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
            _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
            _currentUser = GetRequiredService<ICurrentUser>();
        }

        // --- HELPERS ---
        private async Task<IdentityUser> CreateUserAsync(string userName, string email)
        {
            var user = new IdentityUser(Guid.NewGuid(), userName, email);
            await WithUnitOfWorkAsync(async () => await _userRepository.InsertAsync(user, true));
            return user;
        }

        private async Task<Destination> CreateDestinationAsync()
        {
            var dest = new Destination
            {
                Name = "Destino " + Guid.NewGuid().ToString().Substring(0, 5),
                Country = "Pais Test",
                Coordinates = new Coordinate("0", "0"), // Ajusta según tu namespace
                Photo = "",
                Region = "Region",
                LastUpdated = DateTime.Now,
                Population = 1000
            };
            await WithUnitOfWorkAsync(async () => await _destinationRepository.InsertAsync(dest, true));
            return dest;
        }

        private void Login(Guid userId)
        {
            var fakeUser = GetRequiredService<FakeCurrentUser>();
            fakeUser.SetId(userId);
        }

        // --- 4.1 CREAR EXPERIENCIA ---
        [Fact]
        public async Task Should_Create_Experience_Successfully()
        {
            // Arrange
            var user = await CreateUserAsync("viajero1", "v1@test.com");
            var dest = await CreateDestinationAsync();
            Login(user.Id);

            var input = new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Mi viaje increíble",
                Description = "Fue genial todo.",
                Sentiment = ExperienceSentiment.Positive,
                Cost = 100,
                ExperienceDate = DateTime.Now,
                Tags = "viaje, feliz"
            };

            // Act
            var result = await _experienceAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("Mi viaje increíble");

            // Verificamos persistencia en BD
            await WithUnitOfWorkAsync(async () =>
            {
                var experience = await _experienceRepository.GetAsync(result.Id);
                experience.UserId.ShouldBe(user.Id);
                experience.Title.ShouldBe("Mi viaje increíble");
            });
        }

        // --- 4.2 EDITAR EXPERIENCIA PROPIA ---
        [Fact]
        public async Task Should_Update_Own_Experience()
        {
            // Arrange
            var user = await CreateUserAsync("editor", "edit@test.com");
            var dest = await CreateDestinationAsync();
            Login(user.Id);

            // Crear
            var created = await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Original",
                Description = "Desc",
                Sentiment = ExperienceSentiment.Neutral,
                ExperienceDate = DateTime.Now,
                Tags = "tag1"
            });

            // Act (Editar)
            var updated = await _experienceAppService.UpdateAsync(created.Id, new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Editado",
                Description = "Nueva descripción",
                Sentiment = ExperienceSentiment.Positive,
                ExperienceDate = DateTime.Now,
                Tags = "tag-editado"
            });

            // Assert
            updated.Title.ShouldBe("Editado");
            updated.Sentiment.ShouldBe(ExperienceSentiment.Positive);
        }

        [Fact]
        public async Task Should_Prevent_Updating_Others_Experience()
        {
            // Arrange
            var userA = await CreateUserAsync("userA", "a@test.com");
            var userB = await CreateUserAsync("userB", "b@test.com");
            var dest = await CreateDestinationAsync();

            // A crea experiencia
            Login(userA.Id);
            Guid experienceId = Guid.Empty;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
                {
                    DestinationId = dest.Id,
                    Title = "Experiencia de A",
                    Description = "Privado",
                    Sentiment = ExperienceSentiment.Positive,
                    ExperienceDate = DateTime.Now,
                    Tags = "privado"
                });
                experienceId = result.Id;
            });

            // Act & Assert (B intenta editar)
            Login(userB.Id);

            // Como usamos IUserOwned o filtros de seguridad, puede lanzar EntityNotFound o UserFriendlyException
            // Aceptamos cualquiera de las dos como "Acceso Denegado".
            await Should.ThrowAsync<Exception>(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    await _experienceAppService.UpdateAsync(experienceId, new CreateUpdateExperienceDto
                    {
                        DestinationId = dest.Id,
                        Title = "Hackeado por B",
                        Description = "Mal",
                        Sentiment = ExperienceSentiment.Negative,
                        ExperienceDate = DateTime.Now,
                        Tags = "hacked"
                    });
                });
            });
        }

        // --- 4.3 ELIMINAR EXPERIENCIA PROPIA ---
        [Fact]
        public async Task Should_Delete_Own_Experience()
        {
            // Arrange
            var user = await CreateUserAsync("deleter", "del@test.com");
            var dest = await CreateDestinationAsync();
            Login(user.Id);

            var created = await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Para borrar",
                Description = "...",
                Sentiment = ExperienceSentiment.Neutral,
                ExperienceDate = DateTime.Now,
                Tags = "borrar"
            });

            // Act
            await _experienceAppService.DeleteAsync(created.Id);

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var exists = await _experienceRepository.FirstOrDefaultAsync(e => e.Id == created.Id);
                exists.ShouldBeNull();
            });
        }

        // --- 4.4 LISTAR EXPERIENCIAS DE UN DESTINO ---
        [Fact]
        public async Task Should_List_Experiences_By_Destination()
        {
            // Arrange
            var dest1 = await CreateDestinationAsync(); // Queremos estas
            var dest2 = await CreateDestinationAsync(); // NO queremos estas
            var user = await CreateUserAsync("lister", "list@test.com");
            Login(user.Id);

            // Creamos 2 en Destino 1
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto { DestinationId = dest1.Id, Title = "D1 Exp 1", Description = ".", Sentiment = 0, ExperienceDate = DateTime.Now, Tags = "" });
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto { DestinationId = dest1.Id, Title = "D1 Exp 2", Description = ".", Sentiment = 0, ExperienceDate = DateTime.Now, Tags = "" });

            // Creamos 1 en Destino 2
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto { DestinationId = dest2.Id, Title = "D2 Exp 1", Description = ".", Sentiment = 0, ExperienceDate = DateTime.Now, Tags = "" });

            // Act
            var list = await _experienceAppService.GetListByDestinationAsync(dest1.Id);

            // Assert
            list.Count.ShouldBe(2);
            list.ShouldAllBe(e => e.DestinationId == dest1.Id);
            list.ShouldContain(e => e.Title == "D1 Exp 1");
        }

        // --- 4.5 FILTRAR POR SENTIMIENTO ---
        [Fact]
        public async Task Should_Filter_By_Sentiment()
        {
            // Arrange
            var dest = await CreateDestinationAsync();
            var user = await CreateUserAsync("sentimental", "s@test.com");
            Login(user.Id);

            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto { DestinationId = dest.Id, Title = "Malo", Description = ".", Sentiment = ExperienceSentiment.Negative, ExperienceDate = DateTime.Now, Tags = "" });
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto { DestinationId = dest.Id, Title = "Bueno 1", Description = ".", Sentiment = ExperienceSentiment.Positive, ExperienceDate = DateTime.Now, Tags = "" });
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto { DestinationId = dest.Id, Title = "Bueno 2", Description = ".", Sentiment = ExperienceSentiment.Positive, ExperienceDate = DateTime.Now , Tags = "" });

            // Act: Buscamos solo POSITIVAS
            var list = await _experienceAppService.GetListBySentimentAsync(dest.Id, ExperienceSentiment.Positive);

            // Assert
            list.Count.ShouldBe(2);
            list.ShouldAllBe(e => e.Sentiment == ExperienceSentiment.Positive);
        }

        // --- 4.6 BUSCAR POR PALABRAS CLAVE ---
        [Fact]
        public async Task Should_Search_By_Keyword()
        {
            // Arrange
            var dest = await CreateDestinationAsync();
            var user = await CreateUserAsync("searcher", "find@test.com");
            Login(user.Id);

            // Exp 1: Coincide en Título
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Gran Pizza Italiana",
                Description = "Comida normal",
                Sentiment = 0,
                ExperienceDate = DateTime.Now,
                Tags = ""
            });

            // Exp 2: Coincide en Descripción
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Cena aburrida",
                Description = "Pero la pizza estaba rica",
                Sentiment = 0,
                ExperienceDate = DateTime.Now,
                Tags = ""
            });

            // Exp 3: Coincide en Tags
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Noche de amigos",
                Description = "Bebidas",
                Sentiment = 0,
                ExperienceDate = DateTime.Now,
                Tags = "fiesta, pizza, noche"
            });

            // Exp 4: No tiene nada que ver
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = dest.Id,
                Title = "Ensalada",
                Description = "Verde",
                Sentiment = 0,
                ExperienceDate = DateTime.Now,
                Tags = "saludable"
            });

            // Act: Buscamos "Pizza" (debería traer 1, 2 y 3)
            var results = await _experienceAppService.SearchByKeywordAsync("Pizza");

            // Assert
            results.Count.ShouldBe(3);
            results.ShouldNotContain(e => e.Title == "Ensalada");
        }
    }
}
