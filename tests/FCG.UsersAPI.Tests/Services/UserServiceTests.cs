using FCG.UsersAPI.Application.Abstractions.Messaging;
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
        private readonly Mock<IUserCreatedEventPublisher>
            _userCreatedEventPublisherMock;

        private readonly UserService _userService;

        public UserServiceTests()
        {
            _repositoryUoWMock = new Mock<IRepositoryUoW>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _transactionMock = new Mock<IDbContextTransaction>();

            _userCreatedEventPublisherMock =
                new Mock<IUserCreatedEventPublisher>();

            _repositoryUoWMock
                .Setup(repository => repository.BeginTransaction())
                .Returns(_transactionMock.Object);

            _repositoryUoWMock
                .Setup(repository => repository.UserRepository)
                .Returns(_userRepositoryMock.Object);

            _transactionMock
                .Setup(transaction =>
                    transaction.CommitAsync(
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _transactionMock
                .Setup(transaction =>
                    transaction.RollbackAsync(
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _userCreatedEventPublisherMock
                .Setup(publisher => publisher.Publish(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _userManagerMock = MockUserManager();
            _roleManagerMock = MockRoleManager();

            _userService = new UserService(
                _repositoryUoWMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _userCreatedEventPublisherMock.Object);
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
                .Setup(roleManager =>
                    roleManager.RoleExistsAsync("Usuario"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(userManager =>
                    userManager.CreateAsync(
                        It.IsAny<UserEntity>(),
                        request.Password))
                .Callback<UserEntity, string>(
                    (user, _) => user.Id = "user-1")
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(userManager =>
                    userManager.AddToRoleAsync(
                        It.IsAny<UserEntity>(),
                        "Usuario"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _userService.Add(request);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("user-1", result.Data.Id);
            Assert.Equal(request.Name, result.Data.Name);
            Assert.Equal(request.Email, result.Data.Email);

            _transactionMock.Verify(
                transaction => transaction.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userCreatedEventPublisherMock.Verify(
                publisher => publisher.Publish(
                    "user-1",
                    request.Name,
                    request.Email),
                Times.Once);
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
                .Setup(roleManager =>
                    roleManager.RoleExistsAsync("InvalidRole"))
                .ReturnsAsync(false);

            var result = await _userService.Add(request);

            Assert.False(result.Success);

            Assert.Equal(
                "Invalid role. Use only: Administrator or Usuario.",
                result.Message);

            _transactionMock.Verify(
                transaction => transaction.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _userCreatedEventPublisherMock.Verify(
                publisher => publisher.Publish(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Add_ShouldReturnError_WhenUserCreationFails()
        {
            var request = new CreateUserRequestDto
            {
                Name = "Pedro Ighor",
                Email = "pedro@email.com",
                Password = "Password@123",
                Role = "Usuario"
            };

            _roleManagerMock
                .Setup(roleManager =>
                    roleManager.RoleExistsAsync("Usuario"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(userManager =>
                    userManager.CreateAsync(
                        It.IsAny<UserEntity>(),
                        request.Password))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "User creation failed."
                        }));

            var result = await _userService.Add(request);

            Assert.False(result.Success);
            Assert.Equal("User creation failed.", result.Message);

            _transactionMock.Verify(
                transaction => transaction.RollbackAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userCreatedEventPublisherMock.Verify(
                publisher => publisher.Publish(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Add_ShouldReturnError_WhenAddingRoleFails()
        {
            var request = new CreateUserRequestDto
            {
                Name = "Pedro Ighor",
                Email = "pedro@email.com",
                Password = "Password@123",
                Role = "Usuario"
            };

            _roleManagerMock
                .Setup(roleManager =>
                    roleManager.RoleExistsAsync("Usuario"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(userManager =>
                    userManager.CreateAsync(
                        It.IsAny<UserEntity>(),
                        request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(userManager =>
                    userManager.AddToRoleAsync(
                        It.IsAny<UserEntity>(),
                        "Usuario"))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Failed to add role."
                        }));

            var result = await _userService.Add(request);

            Assert.False(result.Success);
            Assert.Equal("Failed to add role.", result.Message);

            _transactionMock.Verify(
                transaction => transaction.RollbackAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userCreatedEventPublisherMock.Verify(
                publisher => publisher.Publish(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
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
                .Setup(repository =>
                    repository.GetByIdCheck("1"))
                .ReturnsAsync(user);

            var result = await _userService.Delete("1");

            Assert.True(result.Success);
            Assert.False(user.IsActive);
            Assert.NotNull(user.ModificationDate);

            _userRepositoryMock.Verify(
                repository => repository.Update(user),
                Times.Once);

            _repositoryUoWMock.Verify(
                repository => repository.SaveAsync(),
                Times.Once);

            _transactionMock.Verify(
                transaction => transaction.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldReturnError_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByIdCheck("1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _userService.Delete("1");

            Assert.False(result.Success);

            _userRepositoryMock.Verify(
                repository =>
                    repository.Update(It.IsAny<UserEntity>()),
                Times.Never);

            _repositoryUoWMock.Verify(
                repository => repository.SaveAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Get_ShouldReturnUsers_WhenUsersExist()
        {
            var users = new List<UserEntity>
            {
                new()
                {
                    Id = "1",
                    Name = "Pedro",
                    Email = "pedro@email.com",
                    IsActive = true
                }
            };

            _userRepositoryMock
                .Setup(repository => repository.Get())
                .ReturnsAsync(users);

            var result = await _userService.Get();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Pedro", result[0].Name);

            _repositoryUoWMock.Verify(
                repository => repository.Commit(),
                Times.Once);
        }

        [Fact]
        public async Task Get_ShouldThrowException_WhenRepositoryFails()
        {
            _userRepositoryMock
                .Setup(repository => repository.Get())
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.Get());

            _transactionMock.Verify(
                transaction => transaction.Rollback(),
                Times.Once);
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
                .Setup(repository =>
                    repository.GetByIdCheck("1"))
                .ReturnsAsync(user);

            var result = await _userService.GetById("1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Pedro", result.Data.Name);
            Assert.Equal("pedro@email.com", result.Data.Email);
            Assert.True(result.Data.IsActive);

            _repositoryUoWMock.Verify(
                repository => repository.Commit(),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ShouldReturnError_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByIdCheck("1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _userService.GetById("1");

            Assert.False(result.Success);
            Assert.Null(result.Data);

            _repositoryUoWMock.Verify(
                repository => repository.Commit(),
                Times.Never);
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
                .Setup(repository =>
                    repository.GetByIdCheck("1"))
                .ReturnsAsync(user);

            var result = await _userService.Update("1", request);

            Assert.True(result.Success);
            Assert.Equal("Pedro Atualizado", user.Name);
            Assert.Equal("new@email.com", user.Email);
            Assert.True(user.IsActive);
            Assert.NotNull(user.ModificationDate);

            _userRepositoryMock.Verify(
                repository => repository.Update(user),
                Times.Once);

            _repositoryUoWMock.Verify(
                repository => repository.SaveAsync(),
                Times.Once);

            _transactionMock.Verify(
                transaction => transaction.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
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
                .Setup(repository =>
                    repository.GetByIdCheck("1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _userService.Update("1", request);

            Assert.False(result.Success);

            _userRepositoryMock.Verify(
                repository =>
                    repository.Update(It.IsAny<UserEntity>()),
                Times.Never);

            _repositoryUoWMock.Verify(
                repository => repository.SaveAsync(),
                Times.Never);
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