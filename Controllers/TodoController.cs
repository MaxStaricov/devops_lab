using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;
using Services;
using Microsoft.Extensions.Options;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;

    public TodoController(AppDbContext context, ICacheService cache, IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Todo>>> GetAll()
    {
        string cacheKey = "todos:all";
        var cached = await _cache.GetAsync(cacheKey);

        if (cached != null)
        {
            var todos = JsonSerializer.Deserialize<List<Todo>>(cached)!;
            return Ok(todos);
        }

        var allTodos = await _context.Todos.ToListAsync();

        await _cache.SetAsync(cacheKey, JsonSerializer.Serialize(allTodos),
            TimeSpan.FromSeconds(_cacheSettings.TTLSeconds));

        return Ok(allTodos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Todo>> Get(int id)
    {
        string cacheKey = $"todos:{id}";
        var cached = await _cache.GetAsync(cacheKey);

        if (cached != null)
        {
            var todo = JsonSerializer.Deserialize<Todo>(cached)!;
            return Ok(todo);
        }

        var todoFromDb = await _context.Todos.FindAsync(id);
        if (todoFromDb == null) return NotFound();

        await _cache.SetAsync(cacheKey, JsonSerializer.Serialize(todoFromDb),
            TimeSpan.FromSeconds(_cacheSettings.TTLSeconds));

        return Ok(todoFromDb);
    }

    [HttpPost("suicide")]
    public IActionResult Suicide()
    {
        Environment.Exit(1);

        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<Todo>> Create(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync("todos:all");

        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }

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

        await _cache.RemoveAsync($"todos:{id}");
        await _cache.RemoveAsync("todos:all");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
            return NotFound();

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"todos:{id}");
        await _cache.RemoveAsync("todos:all");

        return NoContent();
    }
}
