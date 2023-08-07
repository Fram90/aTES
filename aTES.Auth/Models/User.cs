using Microsoft.AspNetCore.Identity;

namespace aTES.Auth.Models;

public class PopugUser
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public PopugRoles Role { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
}

public enum PopugRoles
{
    Worker,
    Manager,
    Admin,
    Accounter
}