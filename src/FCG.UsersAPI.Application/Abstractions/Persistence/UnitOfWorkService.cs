using FCG.UsersAPI.Application.Abstractions.Messaging;
using FCG.UsersAPI.Application.Services;
using FCG.UsersAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FCG.UsersAPI.Application.Abstractions.Persistence
{
    public class UnitOfWorkService : IUnitOfWorkService
    {
        private readonly IRepositoryUoW _repositoryUoW;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<ProfileEntity> _roleManager;
        private UserService userService;
        private AuthenticationService authenticationService;
        private readonly IUserCreatedEventPublisher _userCreatedEventPublisher;

        public UnitOfWorkService(
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

        public UserService UserService
        {
            get
            {
                if (userService is null)
                    userService = new UserService(
                        _repositoryUoW,
                        _userManager,
                        _roleManager,
                        _userCreatedEventPublisher);
                return userService;
            }
        }

        public AuthenticationService AuthenticationService
        {
            get
            {
                if (authenticationService is null)
                    authenticationService = new AuthenticationService(
                        _repositoryUoW,
                        _userManager);
                return authenticationService;
            }
        }
    }
}