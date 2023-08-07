using aTES.TaskTracker.Db;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace aTES.TaskTracker.Domain.Services;

public interface IPopugSelector
{
    User SelectNext();
}

class RandomPopugSelector : IPopugSelector
{
    private readonly TaskTrackerDbContext _context;

    public RandomPopugSelector(TaskTrackerDbContext context)
    {
        _context = context;
    }

    public User SelectNext()
    {
        var user = _context.Database
            .GetDbConnection()
            .QueryFirst<User>($"select * from users where role not in ('manager', 'admin') order by random()");
        if (user == null)
            throw new Exception("В бд нет ни одного подходящего пользователя");
        return user;
    }
}