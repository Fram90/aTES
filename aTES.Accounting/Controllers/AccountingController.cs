using aTES.Accounting.Db;
using aTES.Accounting.Domain;
using aTES.Accounting.Domain.Services;
using aTES.Accounting.Dtos;
using aTES.Common.Shared.Api;
using aTES.Common.Shared.Auth;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace aTES.Accounting.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AccountingController : BasePopugController
{
    private readonly AccountingDbContext _context;
    private readonly IPriceProvider _priceProvider;
    private readonly IPopugSelector _popugSelector;
    private readonly KafkaDependentProducer<string, string> _kafkaDependentProducer;

    private readonly JsonSerializerSettings _serializer = new();

    public AccountingController(AccountingDbContext context, IPriceProvider priceProvider, IPopugSelector popugSelector,
        KafkaDependentProducer<string, string> kafkaDependentProducer)
    {
        _context = context;
        _priceProvider = priceProvider;
        _popugSelector = popugSelector;
        _kafkaDependentProducer = kafkaDependentProducer;

        _serializer.Converters.Add(new StringEnumConverter());
    }

    [HttpPost("createTask")]
    public async Task<StreamedTask> CreateTask([FromBody] CreateTaskDto model)
    {
        var assignee = _popugSelector.SelectNext();

        var chargePrice = -_priceProvider.GetTaskChargePrice();
        var taskPaymentPrice = _priceProvider.GetTaskPaymentPrice();
        var task = new StreamedTask(Guid.NewGuid(), model.Description, assignee.PublicId, chargePrice,
            taskPaymentPrice);
        _context.Add(task);
        _context.SaveChanges();

        await _kafkaDependentProducer.ProduceAsync("stream-task-created", new Message<string, string>()
        {
            Key = task.PublicId.ToString(),
            Value = JsonConvert.SerializeObject(task, _serializer)
        });

        //да, точно такое же событие. Пока не понял зачем что-то менять
        await _kafkaDependentProducer.ProduceAsync("be-task-created", new Message<string, string>()
        {
            Key = task.PublicId.ToString(),
            Value = JsonConvert.SerializeObject(task, _serializer)
        });

        return task;
    }


    [HttpGet("list")]
    public async Task<List<StreamedTask>> GetTaskList() //пейджинг для слабых. Логика в контроллере для сильных
    {
        var tasks = _context.Tasks.AsNoTracking()
            .Where(x => x.PopugPublicId == GetCurrentUser().PublicId
                        && x.Status == TaskState.Open)
            .ToList();

        return tasks;
    }
}