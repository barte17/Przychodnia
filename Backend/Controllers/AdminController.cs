using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Models.Relational;
using Backend.DTOs;
using Backend.Data;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RelationalDbContext _context;

    public AdminController(
        UserManager<User> userManager,
        RelationalDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("debug-auth")]
    [AllowAnonymous]
    public IActionResult DebugAuth()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var token = authHeader?.Replace("Bearer ", "");
        
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            UserClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            AuthHeader = authHeader,
            Token = token != null ? token.Substring(0, Math.Min(20, token.Length)) + "..." : null
        });
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserListItemDto>>> GetAllUsers()
    {
        var users = await _context.Users
            .Include(u => u.Pacjent)
            .Include(u => u.Lekarz)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var userList = users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            PESEL = u.Pacjent?.PESEL,
            Specjalizacja = u.Lekarz?.Specjalizacja
        }).ToList();

        return Ok(userList);
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserInfoDto>> GetUser(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Pacjent)
            .Include(u => u.Lekarz)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "Użytkownik nie został znaleziony" });

        var userInfo = new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        if (user.Pacjent != null)
        {
            userInfo.Pacjent = new PacjentDto
            {
                IdPacjenta = user.Pacjent.IdPacjenta,
                Imie = user.Pacjent.Imie,
                Nazwisko = user.Pacjent.Nazwisko,
                PESEL = user.Pacjent.PESEL
            };
        }

        if (user.Lekarz != null)
        {
            userInfo.Lekarz = new LekarzDto
            {
                IdLekarza = user.Lekarz.IdLekarza,
                Imie = user.Lekarz.Imie,
                Nazwisko = user.Lekarz.Nazwisko,
                Specjalizacja = user.Lekarz.Specjalizacja
            };
        }

        return Ok(userInfo);
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserInfoDto>> CreateUser(CreateUserByAdminDto createUserDto)
    {
        // Check if user already exists
        if (await _userManager.FindByEmailAsync(createUserDto.Email) != null)
            return BadRequest(new { message = "Użytkownik o podanym adresie email już istnieje" });

        // Validate role-specific requirements
        if (createUserDto.Role == "Patient" && string.IsNullOrEmpty(createUserDto.PESEL))
            return BadRequest(new { message = "PESEL jest wymagany dla pacjentów" });

        if (createUserDto.Role == "Doctor" && string.IsNullOrEmpty(createUserDto.Specjalizacja))
            return BadRequest(new { message = "Specjalizacja jest wymagana dla lekarzy" });

        // Check if PESEL is already used (for patients)
        if (createUserDto.Role == "Patient" && !string.IsNullOrEmpty(createUserDto.PESEL))
        {
            if (await _context.Pacjenci.AnyAsync(p => p.PESEL == createUserDto.PESEL))
                return BadRequest(new { message = "Pacjent o podanym PESEL już istnieje" });
        }

        var user = new User
        {
            UserName = createUserDto.Email,
            Email = createUserDto.Email,
            EmailConfirmed = true, // Admin tworzy konto - automatycznie potwierdzone
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Role = createUserDto.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create user with provided password
        Console.WriteLine($"Creating user: {createUserDto.Email} with role: {createUserDto.Role}");
        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            Console.WriteLine($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return BadRequest(new { message = "Błąd podczas tworzenia konta", errors = result.Errors });
        }
        
        Console.WriteLine($"User created successfully: {user.Email}, ID: {user.Id}, EmailConfirmed: {user.EmailConfirmed}");

        // Create corresponding Pacjent or Lekarz record
        if (createUserDto.Role == "Patient" && !string.IsNullOrEmpty(createUserDto.PESEL))
        {
            var pacjent = new Pacjent
            {
                Imie = createUserDto.FirstName,
                Nazwisko = createUserDto.LastName,
                PESEL = createUserDto.PESEL,
                UserId = user.Id
            };
            _context.Pacjenci.Add(pacjent);
            await _context.SaveChangesAsync();
        }
        else if (createUserDto.Role == "Doctor" && !string.IsNullOrEmpty(createUserDto.Specjalizacja))
        {
            var lekarz = new Lekarz
            {
                Imie = createUserDto.FirstName,
                Nazwisko = createUserDto.LastName,
                Specjalizacja = createUserDto.Specjalizacja,
                UserId = user.Id
            };
            _context.Lekarze.Add(lekarz);
            await _context.SaveChangesAsync();
        }

        return await GetUser(user.Id);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult<UserInfoDto>> UpdateUserRole(string userId, UpdateUserRoleDto updateRoleDto)
    {
        var user = await _context.Users
            .Include(u => u.Pacjent)
            .Include(u => u.Lekarz)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "Użytkownik nie został znaleziony" });

        var oldRole = user.Role;
        var newRole = updateRoleDto.NewRole;

        // Validate role-specific requirements
        if (newRole == "Patient" && string.IsNullOrEmpty(updateRoleDto.PESEL))
            return BadRequest(new { message = "PESEL jest wymagany dla roli pacjent" });

        if (newRole == "Doctor" && string.IsNullOrEmpty(updateRoleDto.Specjalizacja))
            return BadRequest(new { message = "Specjalizacja jest wymagana dla roli lekarz" });

        // Check if PESEL is already used (when changing to patient)
        if (newRole == "Patient" && !string.IsNullOrEmpty(updateRoleDto.PESEL))
        {
            var existingPacjent = await _context.Pacjenci
                .FirstOrDefaultAsync(p => p.PESEL == updateRoleDto.PESEL && p.UserId != userId);
            
            if (existingPacjent != null)
                return BadRequest(new { message = "Pacjent o podanym PESEL już istnieje" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove old role-specific data
            if (oldRole == "Patient" && user.Pacjent != null)
            {
                _context.Pacjenci.Remove(user.Pacjent);
            }
            else if (oldRole == "Doctor" && user.Lekarz != null)
            {
                _context.Lekarze.Remove(user.Lekarz);
            }

            // Update user role
            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;

            // Add new role-specific data
            if (newRole == "Patient" && !string.IsNullOrEmpty(updateRoleDto.PESEL))
            {
                var pacjent = new Pacjent
                {
                    Imie = user.FirstName,
                    Nazwisko = user.LastName,
                    PESEL = updateRoleDto.PESEL,
                    UserId = user.Id
                };
                _context.Pacjenci.Add(pacjent);
            }
            else if (newRole == "Doctor" && !string.IsNullOrEmpty(updateRoleDto.Specjalizacja))
            {
                var lekarz = new Lekarz
                {
                    Imie = user.FirstName,
                    Nazwisko = user.LastName,
                    Specjalizacja = updateRoleDto.Specjalizacja,
                    UserId = user.Id
                };
                _context.Lekarze.Add(lekarz);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetUser(userId);
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Błąd podczas aktualizacji roli użytkownika" });
        }
    }

    [HttpPut("users/{userId}")]
    public async Task<ActionResult<UserInfoDto>> UpdateUser(string userId, UpdateUserByAdminDto updateUserDto)
    {
        var user = await _context.Users
            .Include(u => u.Pacjent)
            .Include(u => u.Lekarz)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "Użytkownik nie został znaleziony" });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update user basic info
            if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(updateUserDto.Email) != null)
                    return BadRequest(new { message = "Użytkownik o podanym adresie email już istnieje" });
                
                user.Email = updateUserDto.Email;
                user.UserName = updateUserDto.Email;
            }

            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;
            
            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;

            user.UpdatedAt = DateTime.UtcNow;

            // Update role-specific data
            if (user.Pacjent != null)
            {
                if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                    user.Pacjent.Imie = updateUserDto.FirstName;
                
                if (!string.IsNullOrEmpty(updateUserDto.LastName))
                    user.Pacjent.Nazwisko = updateUserDto.LastName;

                if (!string.IsNullOrEmpty(updateUserDto.PESEL) && updateUserDto.PESEL != user.Pacjent.PESEL)
                {
                    var existingPacjent = await _context.Pacjenci
                        .FirstOrDefaultAsync(p => p.PESEL == updateUserDto.PESEL && p.UserId != userId);
                    
                    if (existingPacjent != null)
                        return BadRequest(new { message = "Pacjent o podanym PESEL już istnieje" });
                    
                    user.Pacjent.PESEL = updateUserDto.PESEL;
                }
            }

            if (user.Lekarz != null)
            {
                if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                    user.Lekarz.Imie = updateUserDto.FirstName;
                
                if (!string.IsNullOrEmpty(updateUserDto.LastName))
                    user.Lekarz.Nazwisko = updateUserDto.LastName;

                if (!string.IsNullOrEmpty(updateUserDto.Specjalizacja))
                    user.Lekarz.Specjalizacja = updateUserDto.Specjalizacja;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetUser(userId);
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Błąd podczas aktualizacji użytkownika" });
        }
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == currentUserId)
            return BadRequest(new { message = "Nie możesz usunąć swojego własnego konta" });

        var user = await _context.Users
            .Include(u => u.Pacjent)
            .Include(u => u.Lekarz)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "Użytkownik nie został znaleziony" });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove role-specific data first
            if (user.Pacjent != null)
                _context.Pacjenci.Remove(user.Pacjent);
            
            if (user.Lekarz != null)
                _context.Lekarze.Remove(user.Lekarz);

            // Remove the user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Błąd podczas usuwania użytkownika", errors = result.Errors });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Użytkownik został pomyślnie usunięty" });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Błąd podczas usuwania użytkownika" });
        }
    }
}