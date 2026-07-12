using FCG.Contracts.Events;
using FCG.UsersAPI.Application.Abstractions.Messaging;
using MassTransit;

namespace FCG.UsersAPI.Messaging
{
    public class UserCreatedEventPublisher
        : IUserCreatedEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public UserCreatedEventPublisher(
            IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Publish(
            string userId,
            string name,
            string email)
        {
            var userCreatedEvent = new UserCreatedEvent(
                userId,
                name,
                email);

            await _publishEndpoint.Publish(userCreatedEvent);
        }
    }
}