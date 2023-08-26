using aTES.Common;
using aTES.Common.Shared.Api;
using aTES.Common.Shared.Auth;
using aTES.Common.Shared.Kafka;
using aTES.TaskTracker.Db;
using aTES.TaskTracker.Domain;
using aTES.TaskTracker.Domain.Services;
using aTES.TaskTracker.Dtos;
using aTES.TaskTracker.Kafka.Models;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    private readonly KafkaDependentProducer<string, string> _kafkaDependentProducer;

    private readonly JsonSerializerSettings _serializer = new();

    public TasksController(TaskTrackerDbContext context, IPriceProvider priceProvider, IPopugSelector popugSelector,
        KafkaDependentProducer<string, string> kafkaDependentProducer)
    {
        _context = context;
        _priceProvider = priceProvider;
        _popugSelector = popugSelector;
        _kafkaDependentProducer = kafkaDependentProducer;

        _serializer.Converters.Add(new StringEnumConverter());
    }

    [HttpPost("createTask")]
    public async Task<Task> CreateTask([FromBody] CreateTaskDto model)
    {
        var assignee = _popugSelector.SelectNext();

        var chargePrice = -_priceProvider.GetTaskChargePrice();
        var taskPaymentPrice = _priceProvider.GetTaskPaymentPrice();
        var task = new Task(Guid.NewGuid(), model.Description, assignee.PublicId, chargePrice,
            taskPaymentPrice);
        _context.Add(task);
        _context.SaveChanges();

        var streamTaskModel = TaskChangedStreamingModel.FromDomain(task);
        var taskChangedEvent = BaseMessage<TaskChangedStreamingModel>.Create(Guid.NewGuid().ToString(), "stream.task.changed.v1", streamTaskModel);
        _context.Produce("stream-task-lifecycle", taskChangedEvent);

        var business = TaskCreatedBusinessModel.FromDomain(task);
        var taskCreatedEvent = BaseMessage<TaskCreatedBusinessModel>.Create(Guid.NewGuid().ToString(), "task.created.v1", business);
        //да, точно такое же событие. Пока не понял зачем что-то менять
        _context.Produce("be-task-created", taskCreatedEvent);

        return task;
    }

    [HttpPost("compelteTask")]
    public async Task<IActionResult> CompleteTask([FromBody] CompleteTaskDto model)
    {
        var task = _context.Tasks.FirstOrDefault(x => x.PublicId == model.PublicId);
        if (task == null)
            return NotFound();

        if (task.PopugPublicId != GetCurrentUser().PublicId)
            return Unauthorized("Ататат");

        task.Close();
        await _context.SaveChangesAsync();

        var streamTaskModel = TaskChangedStreamingModel.FromDomain(task);
        var streamEvent = BaseMessage<TaskChangedStreamingModel>.Create(Guid.NewGuid().ToString(), "stream.task.changed.v1", streamTaskModel);
        _context.Produce("stream-task-lifecycle", streamEvent);

        //да, точно такое же событие. Пока не понял зачем что-то менять
        var business = TaskClosedBusinessModel.FromDomain(task);
        var beTaskClosedEvent = BaseMessage<TaskClosedBusinessModel>.Create(Guid.NewGuid().ToString(), "task.closed.v1", business);
        _context.Produce("be-task-closed", beTaskClosedEvent);

        return Ok();
    }

    //да, надо сделать асинхронно, я знаю. Не успеваю переделать до дедлайна, сорян. Исправлю позже.
    [HttpPost("shuffleTasks")]
    [MustHaveAnyRole(AuthConsts.ROLE_ADMIN, AuthConsts.ROLE_MANAGER)]
    public async Task<IActionResult> ShuffleTasks()
    {
        var tasks = _context.Tasks.Where(x => x.Status == TaskState.Open).ToList();
        foreach (var task in tasks)
        {
            var popug = _popugSelector.SelectNext();
            task.AssignTo(popug.PublicId);

            var streamTaskModel = TaskChangedStreamingModel.FromDomain(task);

            await _kafkaDependentProducer.ProduceAsync("stream-task-lifecycle", new Message<string, string>()
            {
                Key = task.PublicId.ToString(),
                Value = BasePayload<TaskChangedStreamingModel>.Create("stream.task.changed.v1", streamTaskModel)
                    .ToJson()
            });

            var taskShuffled = TaskShuffledBusinessModel.FromDomain(task);

            _kafkaDependentProducer.Produce("be-task-shuffled", new Message<string, string>()
            {
                Key = task.PublicId.ToString(),
                Value = BasePayload<TaskShuffledBusinessModel>.Create("task.shuffled.v1", taskShuffled).ToJson()
            }, report => Console.WriteLine("Sent TaskShuffled message"));
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