﻿using System.ComponentModel.DataAnnotations;

namespace aTES.Auth.Models.Dtos;

public class UserRegistrationDto
{
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; init; }

    [Required(ErrorMessage = "Email is required")]
    public string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; }

    public PopugRoles Role { get; init; }
}