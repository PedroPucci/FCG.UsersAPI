using FCG.UsersAPI.Application.Contracts.Dto;
using FCG.UsersAPI.Application.Validators;

namespace FCG.UsersAPI.Tests.Validators
{
    public class UserRequestValidatorTests
    {
        private readonly UserRequestValidator _validator;

        public UserRequestValidatorTests()
        {
            _validator = new UserRequestValidator();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateUserRequestDto
            {
                Name = "Pedro Ighor",
                Email = "pedro@email.com",
                Password = "Password@123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenRequestIsInvalid()
        {
            // Arrange
            var request = new CreateUserRequestDto
            {
                Name = "",
                Email = "email-invalido",
                Password = "123"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);

            Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateUserRequestDto.Name));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateUserRequestDto.Email));
            Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateUserRequestDto.Password));
        }
    }
}