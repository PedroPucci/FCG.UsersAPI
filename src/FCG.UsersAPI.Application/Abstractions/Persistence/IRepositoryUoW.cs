using FCG.UsersAPI.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace FCG.UsersAPI.Application.Abstractions.Persistence
{
    public interface IRepositoryUoW
    {
        IUserRepository UserRepository { get; }

        Task SaveAsync();
        void Commit();
        IDbContextTransaction BeginTransaction();
    }
}