using Backend.Models.Relational;

namespace Backend.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    string? ValidateToken(string token);
}