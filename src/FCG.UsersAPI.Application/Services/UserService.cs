using FCG.UsersAPI.Application.Abstractions.Messaging;
using FCG.UsersAPI.Application.Abstractions.Persistence;
using FCG.UsersAPI.Application.Abstractions.Services;
using FCG.UsersAPI.Application.Contracts.Dto;
using FCG.UsersAPI.Application.Validators;
using FCG.UsersAPI.Domain.Common;
using FCG.UsersAPI.Domain.Entities;
using FCG.UsersAPI.Shared.Logging;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace FCG.UsersAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepositoryUoW _repositoryUoW;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<ProfileEntity> _roleManager;
        private readonly IUserCreatedEventPublisher _userCreatedEventPublisher;

        public UserService(
            IRepositoryUoW repositoryUoW,
            UserManager<UserEntity> userManager,
            RoleManager<ProfileEntity> roleManager,
            IUserCreatedEventPublisher userCreatedEventPublisher)
        {
            _repositoryUoW = repositoryUoW;
            _userManager = userManager;
            _roleManager = roleManager;
            _userCreatedEventPublisher = userCreatedEventPublisher;
        }

        public async Task<Result<UserEntity>> Add(CreateUserRequestDto createUserRequestDto)
        {
            using var transaction = _repositoryUoW.BeginTransaction();

            try
            {
                var validationResult =
                    await IsValidUserRequest(createUserRequestDto);

                if (!validationResult.Success)
                {
                    Log.Information(validationResult.Message);

                    return Result<UserEntity>.Error(
                        validationResult.Message);
                }

                if (string.IsNullOrWhiteSpace(createUserRequestDto.Role))
                {
                    const string message =
                        "'Role' can not be null or empty!";

                    Log.Information(message);

                    return Result<UserEntity>.Error(message);
                }

                var role = createUserRequestDto.Role.Trim();

                var roleExists =
                    await _roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    const string message =
                        "Invalid role. Use only: Administrator or Usuario.";

                    Log.Information(message);

                    return Result<UserEntity>.Error(message);
                }

                var userEntity = new UserEntity
                {
                    Email = createUserRequestDto.Email,
                    Name = createUserRequestDto.Name,
                    UserName = createUserRequestDto.Email,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult =
                    await _userManager.CreateAsync(
                        userEntity,
                        createUserRequestDto.Password!);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        " ",
                        createResult.Errors.Select(
                            error => error.Description));

                    await transaction.RollbackAsync();

                    Log.Information(errors);

                    return Result<UserEntity>.Error(errors);
                }

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        userEntity,
                        role);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        " ",
                        roleResult.Errors.Select(
                            error => error.Description));

                    await transaction.RollbackAsync();

                    Log.Information(errors);

                    return Result<UserEntity>.Error(errors);
                }

                await transaction.CommitAsync();

                await _userCreatedEventPublisher.Publish(
                    userEntity.Id,
                    userEntity.Name ?? string.Empty,
                    userEntity.Email ?? string.Empty);

                Log.Information(
                    LogMessages.AddUserSuccess(userEntity));

                return Result<UserEntity>.Ok(userEntity);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Log.Error(
                    LogMessages.AddUserError(ex));

                throw;
            }
        }

        public async Task<Result<bool>> Delete(string id)
        {
            using var transaction = _repositoryUoW.BeginTransaction();

            try
            {
                var user = await _repositoryUoW.UserRepository.GetByIdCheck(id);

                if (user is null)
                {
                    transaction.Rollback();

                    var message = LogMessages.CannotPerformActionOnUser("retrieve", id);
                    Log.Error(message);

                    return Result<bool>.Error(message);
                }

                user.IsActive = false;
                user.ModificationDate = DateTime.UtcNow;

                _repositoryUoW.UserRepository.Update(user);
                await _repositoryUoW.SaveAsync();
                await transaction.CommitAsync();

                Log.Information(LogMessages.DeleteUserSuccess(user));
                return Result<bool>.Ok();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(LogMessages.DeleteUserError(ex));
                throw new InvalidOperationException($"Failed to delete user with id {id}. See logs for details.", ex);
            }
        }

        public async Task<List<UserEntity>> Get()
        {
            using var transaction = _repositoryUoW.BeginTransaction();

            try
            {
                List<UserEntity> userEntities = await _repositoryUoW.UserRepository.Get();
                _repositoryUoW.Commit();

                Log.Information(LogMessages.GetAllUsersSuccess());
                return userEntities;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(LogMessages.GetAllUsersError(ex));
                throw new InvalidOperationException("Error to loading the list User. See logs for details.", ex);
            }
        }

        public async Task<Result<UserResponseDto>> GetById(string id)
        {
            using var transaction = _repositoryUoW.BeginTransaction();

            try
            {
                var user = await _repositoryUoW.UserRepository.GetByIdCheck(id);

                if (user is null)
                {
                    transaction.Rollback();

                    var message = LogMessages.CannotPerformActionOnUser("retrieve", id);
                    Log.Error(message);

                    return Result<UserResponseDto>.Error(message);
                }

                var userResponse = new UserResponseDto
                {
                    Email = user?.Email,
                    Name = user?.Name,
                    IsActive = user?.IsActive ?? false
                };

                _repositoryUoW.Commit();

                Log.Information(LogMessages.GetUserByIdSuccess(user));
                return Result<UserResponseDto>.Ok(userResponse);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(LogMessages.GetUserByIdError(ex));
                throw new InvalidOperationException("Error retrieving the user. See inner exception for details.", ex);
            }
        }

        public async Task<Result<bool>> Update(string id, UpdateUserRequestDto updateUserRequestDto)
        {
            using var transaction = _repositoryUoW.BeginTransaction();

            try
            {
                var user = await _repositoryUoW.UserRepository.GetByIdCheck(id);

                if (user is null)
                {
                    var message = LogMessages.CannotPerformActionOnUser("update", id);
                    Log.Error(message);
                    return Result<bool>.Error(message);
                }

                user.Email = updateUserRequestDto.Email;
                user.Name = updateUserRequestDto.Name;
                user.IsActive = updateUserRequestDto.IsActive;
                user.ModificationDate = DateTime.UtcNow;

                _repositoryUoW.UserRepository.Update(user);
                await _repositoryUoW.SaveAsync();
                await transaction.CommitAsync();

                Log.Information(LogMessages.UpdateUserSuccess(user));
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(LogMessages.UpdateUserError(ex));
                throw new InvalidOperationException($"Failed to update user with id. See logs for details.", ex);
            }
        }

        private async Task<Result<CreateUserRequestDto>> IsValidUserRequest(CreateUserRequestDto createUserRequestDto)
        {
            var requestValidator = await new UserRequestValidator().ValidateAsync(createUserRequestDto);

            if (!requestValidator.IsValid)
            {
                string errorMessage = string.Join(" ", requestValidator.Errors.Select(e => e.ErrorMessage));
                errorMessage = errorMessage.Replace(Environment.NewLine, "");
                return Result<CreateUserRequestDto>.Error(errorMessage);
            }

            return Result<CreateUserRequestDto>.Ok();
        }
    }
}