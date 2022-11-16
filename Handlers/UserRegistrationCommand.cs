using System.Threading;
using System.Threading.Tasks;
using MediatR;
using mediatr_todos.Notifications;

namespace mediatr_todos;


public class UserRegistrationCommand:IRequest<bool>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserRegistrationHandler: IRequestHandler<UserRegistrationCommand, bool>
{
    private readonly IMediator _mediator;

    public UserRegistrationHandler(IMediator mediator) => _mediator = mediator;

    public Task<bool> Handle(UserRegistrationCommand request, CancellationToken cancellationToken)
    {
        // register user
        _mediator.Publish(new UserRegistrationNotification(request.Email));
        
        return Task.FromResult(true);
    }
}