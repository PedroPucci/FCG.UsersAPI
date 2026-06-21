using FCG.UsersAPI.Application.Contracts.Dto;
using FCG.UsersAPI.Domain.Common;

namespace FCG.UsersAPI.Application.Abstractions.Services
{
    public interface IAuthenticationUserService
    {
        Task<Result<string>> Login(UserForAuthenticationDTO userEntity);
    }
}