namespace FCG.UsersAPI.Application.Abstractions.Messaging
{
    public interface IUserCreatedEventPublisher
    {
        Task Publish(
            string userId,
            string name,
            string email);
    }
}