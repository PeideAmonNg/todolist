using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoApi.Models;
using ToDoList.Data;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public TodoController(ApplicationDbContext context, IHttpContextAccessor contextAccessor, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _userManager = userManager;

            if (_context.TodoItems.Count() == 0)
            {
                // Create a new TodoItem if collection is empty,
                // which means you can't delete all TodoItems.
                _context.TodoItems.Add(new TodoItem { Name = "Item1" });
                _context.SaveChanges();
            }
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Unauthorized();
            }

            return await _context.TodoItems.Where(todo => todo.User == user).OrderByDescending(todo => todo.CreatedAt).ToListAsync();
            //return await _context.TodoItems.ToListAsync();
        }

        // GET: api/Todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Unauthorized();
            }
            
            var todoItem =  await _context.TodoItems.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            
            if (todoItem == null)
            {
                return NotFound();
            }

            Debug.WriteLine(todoItem.User);
            Debug.Write("---->");
            Debug.WriteLine(user.UserName);

            if (user.Id != todoItem.User.Id)
            {
                return Unauthorized();
            }

            return todoItem;
        }

        // POST: api/Todo
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem item)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Unauthorized();
            }

            item.User = user;
            item.CreatedAt = DateTime.Now;

            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, item);
        }

        // PUT: api/Todo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItem item)
        {
            Debug.WriteLine(item);

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Unauthorized();
            }

            if (id != item.Id)
            {
                return BadRequest();
            }

            var todoItem = _context.TodoItems
                       .Where(t => t.Id == id)
                       .Include(t => t.User)
                       .FirstOrDefault();

            if (todoItem.User.Id != user.Id)
            {
                return Unauthorized();
            }

            todoItem.Name = item.Name;
            todoItem.IsComplete = item.IsComplete;

            //var todoToUpdate = await _context.TodoItems
            //.SingleOrDefaultAsync(t => t.Id == id);

            //if (await TryUpdateModelAsync<TodoItem>(todoToUpdate,
            //    "",
            //    c => c.Name, c => c.User, c => c.IsComplete))
            //{
            //    try
            //    {
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateException /* ex */)
            //    {
            //        //Log the error (uncomment ex variable name and write a log.)
            //        ModelState.AddModelError("", "Unable to save changes. " +
            //            "Try again, and if the problem persists, " +
            //            "see your system administrator.");
            //    }
            //    return RedirectToAction(nameof(GetTodoItems));
            //}
            _context.Entry(todoItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}