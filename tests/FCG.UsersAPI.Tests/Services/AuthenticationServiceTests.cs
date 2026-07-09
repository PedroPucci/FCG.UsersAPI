using FCG.UsersAPI.Application.Abstractions.Persistence;
using FCG.UsersAPI.Application.Abstractions.Repositories;
using FCG.UsersAPI.Application.Contracts.Dto;
using FCG.UsersAPI.Application.Services;
using FCG.UsersAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FCG.UsersAPI.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IRepositoryUoW> _repositoryUoWMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<UserEntity>> _userManagerMock;
        private readonly AuthenticationService _authenticationService;

        public AuthenticationServiceTests()
        {
            _repositoryUoWMock = new Mock<IRepositoryUoW>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _userManagerMock = MockUserManager();

            _repositoryUoWMock
                .Setup(x => x.UserRepository)
                .Returns(_userRepositoryMock.Object);

            _authenticationService = new AuthenticationService(
                _repositoryUoWMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var request = new UserForAuthenticationDTO
            {
                Email = "pedro@email.com",
                Password = "Password@123"
            };

            var user = new UserEntity
            {
                Id = "1",
                Email = "pedro@email.com",
                UserName = "pedro@email.com",
                Name = "Pedro"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmail(request.Email))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.CheckPassword(user, request.Password))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Usuario" });

            var result = await _authenticationService.Login(request);

            Assert.True(result.Success);
            Assert.False(string.IsNullOrWhiteSpace(result.Data));

            _userRepositoryMock.Verify(x => x.GetByEmail(request.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CheckPassword(user, request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldReturnError_WhenPasswordIsInvalid()
        {
            var request = new UserForAuthenticationDTO
            {
                Email = "pedro@email.com",
                Password = "WrongPassword"
            };

            var user = new UserEntity
            {
                Id = "1",
                Email = "pedro@email.com",
                UserName = "pedro@email.com",
                Name = "Pedro"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmail(request.Email))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.CheckPassword(user, request.Password))
                .ReturnsAsync(false);

            var result = await _authenticationService.Login(request);

            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);

            _userRepositoryMock.Verify(x => x.GetByEmail(request.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CheckPassword(user, request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<UserEntity>()), Times.Never);
        }

        private static Mock<UserManager<UserEntity>> MockUserManager()
        {
            var store = new Mock<IUserStore<UserEntity>>();

            return new Mock<UserManager<UserEntity>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
        }
    }
}