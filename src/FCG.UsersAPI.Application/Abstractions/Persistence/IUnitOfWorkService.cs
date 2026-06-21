using FCG.UsersAPI.Application.Services;

namespace FCG.UsersAPI.Application.Abstractions.Persistence
{
    public interface IUnitOfWorkService
    {
        UserService UserService { get; }
        AuthenticationService AuthenticationService { get; }
    }
}