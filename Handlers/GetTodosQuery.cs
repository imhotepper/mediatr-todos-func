using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace mediatr_todos;


public class GetTodosQuery : BaseRequest, IRequest<List<Todo>>
{
}
record GetTodosQueryHandler : IRequestHandler<GetTodosQuery, List<Todo>>
{
    private readonly TodosService _todosService;
    public GetTodosQueryHandler(TodosService service) => _todosService = service;

    public Task<List<Todo>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_todosService.GetTodos());
    }
}