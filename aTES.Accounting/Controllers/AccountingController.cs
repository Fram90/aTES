using aTES.Accounting.ApiModels;
using aTES.Accounting.Db;
using aTES.Common.Shared.Api;
using aTES.Common.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aTES.Accounting.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AccountingController : BasePopugController
{
    private readonly AccountingDbContext _context;

    public AccountingController(AccountingDbContext context)
    {
        _context = context;
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
            Transactions = transactions.Select(x => new TransactionView()
            {
                Description = x.Description,
                Issued = x.Issued
            }).ToList()
        });
    }

    /// <summary>
    /// Получить статистику за последние forDays дней
    /// </summary>
    /// <param name="forDays"></param>
    /// <returns></returns>
    [HttpGet("stats")]
    [MustHaveAnyRole(AuthConsts.ROLE_ACCOUNTER, AuthConsts.ROLE_ADMIN)]
    public async Task<IActionResult> GetStats(int forDays) //ужас, но попугам и так сойдет
    {
        var billingCycles = _context.BillingCycles.AsNoTracking()
            .OrderByDescending(x => x.StartDate)
            .Include(x => x.Transactions)
            .Take(forDays).ToList();


        var result = new List<DailyTotal>();
        foreach (var billingCycle in billingCycles)
        {
            var dailyTotal = billingCycle.Transactions.Sum(c => c.CreditValue) - billingCycle.Transactions.Sum(c => c.DebitValue);
            result.Add(new DailyTotal()
            {
                Profit = dailyTotal,
                Date = new DateOnly(billingCycle.StartDate.Year, billingCycle.StartDate.Month, billingCycle.StartDate.Day)
            });
        }

        return Ok(result);
    }
}