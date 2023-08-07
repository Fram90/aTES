using System.Runtime.Intrinsics.X86;
using aTES.TaskTracker.Db;
using aTES.TaskTracker.Domain;
using aTES.TaskTracker.Domain.Services;
using aTES.TaskTracker.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = aTES.TaskTracker.Domain.Task;

namespace aTES.TaskTracker.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskTrackerDbContext _context;
    private readonly IPriceProvider _priceProvider;
    private readonly IPopugSelector _popugSelector;

    public TasksController(TaskTrackerDbContext context, IPriceProvider priceProvider, IPopugSelector popugSelector)
    {
        _context = context;
        _priceProvider = priceProvider;
        _popugSelector = popugSelector;
    }

    [HttpPost("createTask")]
    public void CreateTask([FromBody] CreateTaskDto model)
    {
        var assignee = _popugSelector.SelectNext();

        var chargePrice = _priceProvider.GetTaskChargePrice();
        var taskPaymentPrice = _priceProvider.GetTaskPaymentPrice();
        var task = new Task(Guid.NewGuid(), model.Description, assignee.PublicId, chargePrice,
            taskPaymentPrice);
        _context.Add(task);
        _context.SaveChanges();

        //опубликовать событие BE TaskCreated
        //опубликовать событие CUD TaskCreated
    }

    [HttpPost("compelteTask")]
    public IActionResult CompleteTask([FromBody] CompleteTaskDto model)
    {
        var task = _context.Tasks.FirstOrDefault(x => x.PublicId == model.PublicId);
        if (task == null)
        {
            return NotFound();
        }

        task.Close();
        _context.SaveChanges();

        //опубликовать событие BE TaskClosed
        //опубликовать событие CUD TaskClosed
        return Ok();
    }
    
    [HttpPost("shuffleTasks")]
    public async Task<IActionResult> ShuffleTasks()
    {
        if (!User.IsInRole(AuthConsts.ROLE_ADMIN) &&
            !User.IsInRole(AuthConsts.ROLE_MANAGER))
        {
            return Unauthorized();
        }
        
        await foreach (var task in _context.Tasks.Where(x => x.Status == TaskState.Open).AsAsyncEnumerable())
        {
            var popug = _popugSelector.SelectNext();
            task.AssignTo(popug.PublicId);
            await _context.SaveChangesAsync();

            //опубликовать событие BE TaskShuffled    
        }

        return Ok();
    }

    [HttpGet("list")]
    public async Task<List<Task>> GetTaskList() //пейджинг для слабых
    {
        var currentPublicId = User.Claims.FirstOrDefault(x => x.Type == AuthConsts.CLAIMS_PUBLIC_ID)?.Value;
        if (currentPublicId == null)
            throw new Exception("No PublicId in user claims");

        var typedId = Guid.Parse(currentPublicId);
        
        var tasks = _context.Tasks.AsNoTracking()
            .Where(x => x.PopugPublicId == typedId
                        && x.Status == TaskState.Open)
            .ToList();

        return tasks;
    }
}