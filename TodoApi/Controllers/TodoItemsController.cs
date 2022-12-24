using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("TodoItemsController")]
    public class TodoItemsController : ControllerBase
    {
        private readonly string _connString = Environment.GetEnvironmentVariable("NPGSQL_CONN") ??
                                              throw new InvalidOperationException(
                                                  "NPGSQL database connection string environment variable is not defined.");

        [HttpGet]
        /*
         * Return all todo_items sorted by their id
         * 
         */
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT id, name, is_complete FROM todo_items ORDER BY id", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var todoItems = new List<TodoItem>();

            while (await reader.ReadAsync())
            {
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                var todoItem = new TodoItem(values);
                todoItems.Add(todoItem);
            }

            return todoItems;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT id, name, is_complete FROM todo_items WHERE id = $1", conn)
            {
                Parameters = { new() { Value = id } }
            };
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                var todoItem = new TodoItem(values);
                return todoItem;
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("DELETE FROM todo_items WHERE id = $1", conn)
            {
                Parameters = { new() { Value = id } }
            };
            var count = await cmd.ExecuteNonQueryAsync();
            if (count == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> ChangeTodoItem([FromBody] TodoItem todoItem)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT id FROM todo_items WHERE id = $1", conn)
            {
                Parameters = { new() { Value = todoItem.Id } }
            };
            await using var reader = await cmd.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.CloseAsync();
                // todoItem already exists
                await using var cmd2 =
                    new NpgsqlCommand("UPDATE todo_items SET name = $1, is_complete = $2 WHERE id = $3", conn)
                    {
                        Parameters =
                        {
                            new() { Value = todoItem.Name },
                            new() { Value = todoItem.IsComplete },
                            new() { Value = todoItem.Id },
                        }
                    };
                var count = await cmd2.ExecuteNonQueryAsync();
                if (count == 0)
                {
                    return BadRequest();
                }
            }
            else
            {
                // new todoItem
                var response = await PostTodoItem(todoItem);
                Console.WriteLine(response.Result);
            }

            return Ok();
        }


        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem([FromBody] TodoItem todoItem)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            NpgsqlCommand cmd;

            if (todoItem.Id != null)
            {
                // Insert todoItem with given ID
                cmd = new NpgsqlCommand(
                    "INSERT INTO todo_items (id, name, is_complete) VALUES ($1, $2, $3)", conn)
                {
                    Parameters =
                    {
                        new() { Value = todoItem.Id },
                        new() { Value = todoItem.Name },
                        new() { Value = todoItem.IsComplete }
                    }
                };
            }
            else
            {
                // Insert todoItem with default ID
                cmd = new NpgsqlCommand(
                    "INSERT INTO todo_items (name, is_complete) VALUES ($1, $2)", conn)
                {
                    Parameters =
                    {
                        new() { Value = todoItem.Name },
                        new() { Value = todoItem.IsComplete }
                    }
                };
            }

            var count = await cmd.ExecuteNonQueryAsync();
            if (count == 0)
            {
                return BadRequest();
            }

            return todoItem;
        }

        // Examples that take input from Body / Query String / Route / Header / Form respectively
        [HttpPost("body")]
        public ActionResult<JsonObject> PostDataFromBody([FromBody] JsonObject data)
        {
            if (data != null)
            {
                Console.WriteLine("data: " + data);
                return data;
            }

            return BadRequest();
        }

        [HttpPost("query")]
        public ActionResult<object> PostDataFromQuery([FromQuery] string name, [FromQuery] bool isMale)
        {
            var data = new
            {
                Name = name,
                IsMale = isMale
            };
            return data;
        }

        [HttpPost("route/{name}/{isMale}")]
        public ActionResult<object> PostDataFromRoute([FromRoute] string name, [FromRoute] bool isMale)
        {
            var data = new
            {
                Name = name,
                IsMale = isMale
            };
            return data;
        }

        [HttpPost("header")]
        public ActionResult<object> PostDataFromHeader([FromHeader] string name, [FromHeader] bool isMale = false)
        {
            var data = new
            {
                Name = name,
                IsMale = isMale
            };
            return data;
        }

        [HttpPost("form")]
        public ActionResult<object> PostDataFromForm([FromForm] string name, [FromForm] bool isMale)
        {
            var data = new
            {
                Name = name,
                IsMale = isMale
            };
            return data;
        }
    }
}