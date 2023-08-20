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
    private readonly KafkaDependentProducer<string, string> _kafkaDependentProducer;

    private readonly JsonSerializerSettings _serializer = new();

    public AccountingController(AccountingDbContext context,
        KafkaDependentProducer<string, string> kafkaDependentProducer)
    {
        _context = context;
        _kafkaDependentProducer = kafkaDependentProducer;

        _serializer.Converters.Add(new StringEnumConverter());
    }


    [HttpGet("")]
    [ProducesResponseType(null, 404)]
    public async Task<IActionResult> GetAccountInfo()
    {
        var currentUser = GetCurrentUser();
        var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.PopugPublicId == currentUser.PublicId);
        if (account == null)
        {
            return NotFound();
        }

        var currentBillingCycle = _context.GetOrAddCurrentBillingCycle();
        var transactions = await _context.Transactions.AsNoTracking().Where(x => x.AccountId == account.Id &&
                                                                                 x.BillingCycleId == currentBillingCycle.Id)
            .ToListAsync();

        var balance = transactions.Sum(x => x.CreditValue) - transactions.Sum(x => x.DebitValue);

        return Ok(new AccountInfo()
        {
            Balance = balance,
            AuditLog = transactions.Select(x => new AuditLogItemView()
            {
                Description = x.Description,
                Issued = x.Issued
            }).ToList()
        });
    }
}

public class AccountInfo
{
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public List<AuditLogItemView> AuditLog { get; set; }
}

public class AuditLogItemView
{
    public DateTimeOffset Issued { get; set; }
    public string Description { get; set; }
}