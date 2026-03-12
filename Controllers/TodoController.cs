using StackExchange.Redis;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;
using Microsoft.Extensions.Options;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDatabase _cache;
    private readonly CacheSettings _cacheSettings;

    public TodoController(AppDbContext context, IConnectionMultiplexer redis, IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _cache = redis.GetDatabase();
        _cacheSettings = cacheSettings.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Todo>>> GetAll()
    {
        string cacheKey = "todos:all";
        var cached = await _cache.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            var todos = JsonSerializer.Deserialize<List<Todo>>(cached)!;
            return Ok(todos);
        }

        var allTodos = await _context.Todos.ToListAsync();

        // TTL из конфигурации
        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(allTodos),
            TimeSpan.FromSeconds(_cacheSettings.TTLSeconds));

        return Ok(allTodos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Todo>> Get(int id)
    {
        string cacheKey = $"todos:{id}";
        var cached = await _cache.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            var todo = JsonSerializer.Deserialize<Todo>(cached)!;
            return Ok(todo);
        }

        var todoFromDb = await _context.Todos.FindAsync(id);
        if (todoFromDb == null) return NotFound();

        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(todoFromDb),
            TimeSpan.FromSeconds(_cacheSettings.TTLSeconds));

        return Ok(todoFromDb);
    }

    // POST: api/todo/suicide
    [HttpPost("suicide")]
    public IActionResult Suicide()
    {
        // Никаких логов, никаких задержек - мгновенная смерть
        Environment.Exit(1);
        
        // Этот код никогда не выполнится, но компилятор требует return
        return Ok();
    }

    // POST: api/todo
    [HttpPost]
    public async Task<ActionResult<Todo>> Create(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Сброс кэша для всего списка
        await _cache.KeyDeleteAsync("todos:all");

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

        // Сброс кэша для конкретного элемента и для всего списка
        await _cache.KeyDeleteAsync($"todos:{id}");
        await _cache.KeyDeleteAsync("todos:all");

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

        // Сброс кэша для конкретного элемента и для всего списка
        await _cache.KeyDeleteAsync($"todos:{id}");
        await _cache.KeyDeleteAsync("todos:all");

        return NoContent();
    }
}
