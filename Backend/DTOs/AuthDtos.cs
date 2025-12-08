namespace Backend.DTOs;

public record RegisterDto(string Email, string Name, string Password);
public record LoginDto(string Email, string Password);
