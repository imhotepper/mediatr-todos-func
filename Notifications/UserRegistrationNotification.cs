using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace mediatr_todos.Notifications;


public class UserRegistrationNotification: INotification
{
    public UserRegistrationNotification(string email) => Email = email;
    public string Email { get; }
}


public class UserRegistrationNotificationAdmin: INotificationHandler<UserRegistrationNotification>
{
    public Task Handle(UserRegistrationNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Email sent to Admin for user:{ notification.Email }");
        return Task.CompletedTask;
    }
}


public class UserRegistrationNotificationUser: INotificationHandler<UserRegistrationNotification>
{
    public Task Handle(UserRegistrationNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Email sent to user:{ notification.Email }");
        return Task.CompletedTask;
    }
}
