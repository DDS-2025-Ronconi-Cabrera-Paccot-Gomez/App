using System;
using System.Collections.Generic; // Para List<T>
using System.Linq.Expressions; // Para Expression<Func<>>
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using TravelPro.Ratings;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using Volo.Abp.Validation; // Para AbpValidationException
using Xunit;


namespace TravelPro.Application.Tests.Ratings
{
    public class RatingAppService_UnitTests
    {
        private readonly RatingAppService _ratingAppService;
        private readonly IRepository<Rating, Guid> _repositoryMock;
        private readonly ICurrentUser _currentUserMock;
        private readonly IObjectMapper _objectMapperMock;

        // --- CONSTRUCTOR (Mockeando LazyServiceProvider) ---
        public RatingAppService_UnitTests()
        {
            _repositoryMock = Substitute.For<IRepository<Rating, Guid>>();
            _currentUserMock = Substitute.For<ICurrentUser>();
            _objectMapperMock = Substitute.For<IObjectMapper>();

            var lazyServiceProviderMock = Substitute.For<IAbpLazyServiceProvider>();

            lazyServiceProviderMock.LazyGetRequiredService<ICurrentUser>().Returns(_currentUserMock);
            lazyServiceProviderMock.LazyGetRequiredService<IObjectMapper>().Returns(_objectMapperMock);

            _ratingAppService = new RatingAppService(_repositoryMock)
            {
                // Asignamos el mock a la propiedad pública
                LazyServiceProvider = lazyServiceProviderMock
            };
        }

        // --- PRUEBA DE CREACIÓN (VALIDA SCORE Y COMENTARIO OPCIONAL) ---
        [Theory]
        [InlineData(1, "Con Comentario")]
        [InlineData(5, null)] // Sin Comentario
        public async Task CreateAsync_Should_Work_For_Valid_Input(int score, string comment)
        {
            // --- ARRANGE ---
            var testUserId = Guid.NewGuid();
            var inputDto = new CreateUpdateRatingDto
            {
                DestinationId = Guid.NewGuid(),
                Score = (byte)score,
                Comment = comment
            };

            var ratingEntity = new Rating();
            var ratingDto = new RatingDto();
            var emptyList = new List<Rating>(); // Lista vacía para simular que no hay duplicados

            // --- Configuración de Mocks ---
            _currentUserMock.Id.Returns(testUserId);

            // Mock del Repository (Simula que NO hay duplicados)
            _repositoryMock.GetListAsync(Arg.Any<Expression<Func<Rating, bool>>>())
               .Returns(emptyList); // O Task.FromResult(emptyList) si da error

            // Mock del ObjectMapper (con Arg.Any para robustez)
            _objectMapperMock.Map<CreateUpdateRatingDto, Rating>(Arg.Any<CreateUpdateRatingDto>()).Returns(ratingEntity);
            _objectMapperMock.Map<Rating, RatingDto>(Arg.Any<Rating>()).Returns(ratingDto);

            // --- ACT ---
            await _ratingAppService.CreateAsync(inputDto);

            // --- ASSERT ---
            ratingEntity.UserId.ShouldBe(testUserId); // Verifica asignación de UserId
            await _repositoryMock.Received(1).InsertAsync(ratingEntity, true); // Verifica guardado
        }

        // --- PRUEBA DE DUPLICADOS ---
        [Fact]
        public async Task CreateAsync_Should_Throw_When_Duplicate_Rating_Exists()
        {
            // --- ARRANGE ---
            var userId = Guid.NewGuid();
            var destinationId = Guid.NewGuid();
            _currentUserMock.Id.Returns(userId);

            var existingRating = new Rating { UserId = userId, DestinationId = destinationId };
            var existingList = new List<Rating> { existingRating };

            // Mock del Repository (Simula que SÍ encontró duplicados)
            _repositoryMock.GetListAsync(Arg.Any<Expression<Func<Rating, bool>>>())
                .Returns(existingList); // <-- Pasale la lista directamente; // O Task.FromResult(existingList)

            var input = new CreateUpdateRatingDto
            {
                DestinationId = destinationId, // Mismo destino
                Score = 5
            };

            // Mock del ObjectMapper (necesario aunque falle antes)
            _objectMapperMock.Map<CreateUpdateRatingDto, Rating>(Arg.Any<CreateUpdateRatingDto>())
                            .Returns(new Rating()); // Devolver una instancia vacía

            // --- ACT & ASSERT ---
            await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _ratingAppService.CreateAsync(input);
            });
        }

        // --- NO INCLUIR LA PRUEBA DE SCORE INVÁLIDO ---
    }
}