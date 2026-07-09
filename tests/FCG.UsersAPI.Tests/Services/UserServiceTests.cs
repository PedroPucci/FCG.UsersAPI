using FCG.UsersAPI.Application.Abstractions.Persistence;
using FCG.UsersAPI.Application.Abstractions.Repositories;
using FCG.UsersAPI.Application.Contracts.Dto;
using FCG.UsersAPI.Application.Services;
using FCG.UsersAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace FCG.UsersAPI.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IRepositoryUoW> _repositoryUoWMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<UserEntity>> _userManagerMock;
        private readonly Mock<RoleManager<ProfileEntity>> _roleManagerMock;
        private readonly Mock<IDbContextTransaction> _transactionMock;

        private readonly UserService _userService;

        public UserServiceTests()
        {
            _repositoryUoWMock = new Mock<IRepositoryUoW>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _transactionMock = new Mock<IDbContextTransaction>();

            _repositoryUoWMock
                .Setup(x => x.BeginTransaction())
                .Returns(_transactionMock.Object);

            _repositoryUoWMock
                .Setup(x => x.UserRepository)
                .Returns(_userRepositoryMock.Object);

            _userManagerMock = MockUserManager();
            _roleManagerMock = MockRoleManager();

            _userService = new UserService(
                _repositoryUoWMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object);
        }

        [Fact]
        public async Task Add_ShouldReturnSuccess_WhenUserIsValid()
        {
            var request = new CreateUserRequestDto
            {
                Name = "Pedro Ighor",
                Email = "pedro@email.com",
                Password = "Password@123",
                Role = "Usuario"
            };

            _roleManagerMock
                .Setup(x => x.RoleExistsAsync("Usuario"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), "Usuario"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _userService.Add(request);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            _transactionMock.Verify(x => x.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldReturnError_WhenRoleIsInvalid()
        {
            var request = new CreateUserRequestDto
            {
                Name = "Pedro Ighor",
                Email = "pedro@email.com",
                Password = "Password@123",
                Role = "InvalidRole"
            };

            _roleManagerMock
                .Setup(x => x.RoleExistsAsync("InvalidRole"))
                .ReturnsAsync(false);

            var result = await _userService.Add(request);

            Assert.False(result.Success);
            Assert.Equal("Invalid role. Use only: Administrator or Usuario.", result.Message);
        }

        [Fact]
        public async Task Delete_ShouldReturnSuccess_WhenUserExists()
        {
            var user = new UserEntity
            {
                Id = "1",
                Name = "Pedro",
                Email = "pedro@email.com",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdCheck("1"))
                .ReturnsAsync(user);

            var result = await _userService.Delete("1");

            Assert.True(result.Success);

            _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
            _repositoryUoWMock.Verify(x => x.SaveAsync(), Times.Once);
            _transactionMock.Verify(x => x.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldReturnError_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdCheck("1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _userService.Delete("1");

            Assert.False(result.Success);

            _userRepositoryMock.Verify(x => x.Update(It.IsAny<UserEntity>()), Times.Never);
            _repositoryUoWMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task Get_ShouldReturnUsers_WhenUsersExist()
        {
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Id = "1",
                    Name = "Pedro",
                    Email = "pedro@email.com",
                    IsActive = true
                }
            };

            _userRepositoryMock
                .Setup(x => x.Get())
                .ReturnsAsync(users);

            var result = await _userService.Get();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Pedro", result[0].Name);
        }

        [Fact]
        public async Task Get_ShouldThrowException_WhenRepositoryFails()
        {
            _userRepositoryMock
                .Setup(x => x.Get())
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.Get());
        }

        [Fact]
        public async Task GetById_ShouldReturnUser_WhenUserExists()
        {
            var user = new UserEntity
            {
                Id = "1",
                Name = "Pedro",
                Email = "pedro@email.com",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdCheck("1"))
                .ReturnsAsync(user);

            var result = await _userService.GetById("1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Pedro", result.Data.Name);
            Assert.Equal("pedro@email.com", result.Data.Email);
        }

        [Fact]
        public async Task GetById_ShouldReturnError_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdCheck("1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _userService.GetById("1");

            Assert.False(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Update_ShouldReturnSuccess_WhenUserExists()
        {
            var user = new UserEntity
            {
                Id = "1",
                Name = "Pedro",
                Email = "old@email.com",
                IsActive = true
            };

            var request = new UpdateUserRequestDto
            {
                Name = "Pedro Atualizado",
                Email = "new@email.com",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdCheck("1"))
                .ReturnsAsync(user);

            var result = await _userService.Update("1", request);

            Assert.True(result.Success);

            Assert.Equal("Pedro Atualizado", user.Name);
            Assert.Equal("new@email.com", user.Email);

            _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
            _repositoryUoWMock.Verify(x => x.SaveAsync(), Times.Once);
            _transactionMock.Verify(x => x.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldReturnError_WhenUserDoesNotExist()
        {
            var request = new UpdateUserRequestDto
            {
                Name = "Pedro",
                Email = "pedro@email.com",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdCheck("1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _userService.Update("1", request);

            Assert.False(result.Success);

            _userRepositoryMock.Verify(x => x.Update(It.IsAny<UserEntity>()), Times.Never);
            _repositoryUoWMock.Verify(x => x.SaveAsync(), Times.Never);
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

        private static Mock<RoleManager<ProfileEntity>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<ProfileEntity>>();

            return new Mock<RoleManager<ProfileEntity>>(
                store.Object,
                null!,
                null!,
                null!,
                null!);
        }
    }
}