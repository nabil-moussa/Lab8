﻿namespace Lab8.Auth.Common.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public string Role { get; set; }
    public string BranchId { get; set; }
}