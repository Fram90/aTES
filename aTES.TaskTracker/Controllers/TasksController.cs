using System.Runtime.Intrinsics.X86;
using aTES.TaskTracker.Db;
using aTES.TaskTracker.Domain;
using aTES.TaskTracker.Domain.Services;
using aTES.TaskTracker.Dtos;
using aTES.TaskTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = aTES.TaskTracker.Domain.Task;

namespace aTES.TaskTracker.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TasksController : BasePopugController
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

        var chargePrice = -_priceProvider.GetTaskChargePrice();
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
            return NotFound();

        if (task.PopugPublicId != GetCurrentUser().PublicId)
            return Unauthorized("Ататат");

        task.Close();
        _context.SaveChanges();

        //опубликовать событие BE TaskClosed
        //опубликовать событие CUD TaskClosed
        return Ok();
    }

    [HttpPost("shuffleTasks")]
    [MustHaveAnyRole(AuthConsts.ROLE_ADMIN, AuthConsts.ROLE_MANAGER)]
    public async Task<IActionResult> ShuffleTasks()
    {
        //это супер не оптимально, но попуги ведь подождут, верно?
        var tasks = _context.Tasks.Where(x => x.Status == TaskState.Open).ToList();
        foreach (var task in tasks)
        {
            var popug = _popugSelector.SelectNext();
            task.AssignTo(popug.PublicId);

            //опубликовать событие BE TaskShuffled    
        }
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("list")]
    public async Task<List<Task>> GetTaskList() //пейджинг для слабых. Логика в контроллере для сильных
    {
        var tasks = _context.Tasks.AsNoTracking()
            .Where(x => x.PopugPublicId == GetCurrentUser().PublicId
                        && x.Status == TaskState.Open)
            .ToList();

        return tasks;
    }
}