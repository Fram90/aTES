using aTES.Accounting.Db;

namespace aTES.Accounting.Domain.Services;

public class AccountService
{
    private readonly AccountingDbContext _ctx;
    private readonly ILogger _logger;

    public AccountService(AccountingDbContext ctx, ILogger logger)
    {
        this._ctx = ctx;
        _logger = logger;
    }

    public void Charge(Guid popugId, Guid taskId, decimal chargeAmount)
    {
        var account = _ctx.Accounts.FirstOrDefault(c => c.PopugPublicId == popugId);
        if (account == null)
        {
            //todo retry
            throw new Exception("пришло событие на списание средств, а счет для попуга еще не создан");
        }

        var billingCycle = _ctx.GetOrAddCurrentBillingCycle();

        account.Charge(billingCycle, taskId, chargeAmount);
        _ctx.SaveChanges();
        
        _logger.LogInformation($"Списали средства с попуга {account.PopugPublicId} за назначение задачи {taskId}");
    }
}