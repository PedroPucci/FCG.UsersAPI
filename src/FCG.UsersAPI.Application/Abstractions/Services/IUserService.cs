using FCG.UsersAPI.Application.Contracts.Dto;
using FCG.UsersAPI.Domain.Common;
using FCG.UsersAPI.Domain.Entities;

namespace FCG.UsersAPI.Application.Abstractions.Services
{
    public interface IUserService
    {
        Task<Result<UserEntity>> Add(CreateUserRequestDto createUserRequestDto);
        Task<Result<bool>> Update(string id, UpdateUserRequestDto updateUserRequestDto);
        Task<Result<bool>> Delete(string id);
        Task<List<UserEntity>> Get();
        Task<Result<UserResponseDto>> GetById(string id);
    }
}