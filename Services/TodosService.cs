using System.Collections.Generic;

namespace mediatr_todos;

public record TodosService
{
    List<Todo> _todos = new List<Todo>();

    public Todo Add(string title)
    {
        var todo = new Todo(_todos.Count + 1, title);
        _todos.Add(todo);
        return todo;
    }

    public List<Todo> GetTodos() => _todos;
}