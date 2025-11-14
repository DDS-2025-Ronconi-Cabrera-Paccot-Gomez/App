using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TravelPro.Ratings;
using Volo.Abp.Validation;
using Xunit;

namespace TravelPro.Ratings
{
    public class RatingTests
    {
        //Prueba 1.1: Crear una calificación con un puntaje válido
        [Fact]
        public void CreateRating_WithValidScore_ShouldCreateSuccessfully()
        {
            // Arrange
            var destinationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var rating = new Rating(destinationId, userId, 4, "Muy buen destino");

            // Assert
            Assert.Equal(destinationId, rating.DestinationId);
            Assert.Equal(userId, rating.UserId);
            Assert.Equal(4, rating.Score);
            Assert.Equal("Muy buen destino", rating.Comment);
        }
        //Prueba 1.2: Crear una calificación con un puntaje inválido
        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-3)]
        public void CreateRating_WithInvalidScore_ShouldThrowArgumentException(int invalidScore)
        {
            // Arrange
            var destinationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                new Rating(destinationId, userId, invalidScore, "comentario")
            );

            Assert.Equal("El puntaje debe estar entre 1 y 5.", exception.Message);
        }
        //Prueba 3 Permite comentarios opcionales
        [Fact]
        public void Constructor_Should_Allow_Null_Comment()
        {
            // ARRANGE
            var destinationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var score = 4; // Un puntaje válido (entre 1 y 5)

            // ACT
            // Llamamos al constructor OMITIENDO el parámetro 'comment'
            var rating = new Rating(destinationId, userId, score);

            // ASSERT

            // 1. Verificar que el objeto se creó correctamente
            rating.ShouldNotBeNull();

            // 2. Verificar que los valores obligatorios se asignaron correctamente
            rating.DestinationId.ShouldBe(destinationId);
            rating.UserId.ShouldBe(userId);
            rating.Score.ShouldBe(score);

            // 3. Verificar la condición clave: el comentario debe ser null
            rating.Comment.ShouldBeNull();
        }
       
    }
}
