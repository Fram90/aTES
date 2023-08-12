using System.ComponentModel.DataAnnotations;

namespace aTES.Auth.Models.Dtos;

public class UserLoginDto 
{
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; init; }
}