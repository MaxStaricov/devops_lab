using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Todo>>> GetAll()
    {
        return await _context.Todos.ToListAsync();
    }

    // GET: api/todo/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Todo>> Get(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
            return NotFound();

        return todo;
    }

    // POST: api/todo
    [HttpPost]
    public async Task<ActionResult<Todo>> Create(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }

    // PUT: api/todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Todo updatedTodo)
    {
        if (id != updatedTodo.Id)
            return BadRequest();

        var exists = await _context.Todos.AnyAsync(t => t.Id == id);
        if (!exists)
            return NotFound();

        _context.Entry(updatedTodo).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
            return NotFound();

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
