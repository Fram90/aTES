using System.ComponentModel.DataAnnotations;

namespace aTES.Auth.Models.Dtos;

public class ChangeRoleDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; init; }
    
    public PopugRoles NewRole { get; init; }
}