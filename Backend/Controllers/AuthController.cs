using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Models.Relational;
using Backend.DTOs;
using Backend.Services;
using Backend.Data;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly RelationalDbContext _context;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        RelationalDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            return BadRequest(new { message = "Użytkownik o podanym adresie email już istnieje" });

        // Validate role-specific requirements
        if (registerDto.Role == "Patient" && string.IsNullOrEmpty(registerDto.PESEL))
            return BadRequest(new { message = "PESEL jest wymagany dla pacjentów" });

        if (registerDto.Role == "Doctor" && string.IsNullOrEmpty(registerDto.Specjalizacja))
            return BadRequest(new { message = "Specjalizacja jest wymagana dla lekarzy" });

        // Check if PESEL is already used (for patients)
        if (registerDto.Role == "Patient" && !string.IsNullOrEmpty(registerDto.PESEL))
        {
            if (await _context.Pacjenci.AnyAsync(p => p.PESEL == registerDto.PESEL))
                return BadRequest(new { message = "Pacjent o podanym PESEL już istnieje" });
        }

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Role = registerDto.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Błąd podczas tworzenia konta", errors = result.Errors });
        }

        // Create corresponding Pacjent or Lekarz record
        if (registerDto.Role == "Patient" && !string.IsNullOrEmpty(registerDto.PESEL))
        {
            var pacjent = new Pacjent
            {
                Imie = registerDto.FirstName,
                Nazwisko = registerDto.LastName,
                PESEL = registerDto.PESEL,
                UserId = user.Id
            };
            _context.Pacjenci.Add(pacjent);
        }
        else if (registerDto.Role == "Doctor" && !string.IsNullOrEmpty(registerDto.Specjalizacja))
        {
            var lekarz = new Lekarz
            {
                Imie = registerDto.FirstName,
                Nazwisko = registerDto.LastName,
                Specjalizacja = registerDto.Specjalizacja,
                UserId = user.Id
            };
            _context.Lekarze.Add(lekarz);
        }

        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Expires = DateTime.UtcNow.AddHours(24)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return BadRequest(new { message = "Nieprawidłowe dane logowania" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
            return BadRequest(new { message = "Nieprawidłowe dane logowania" });

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Expires = DateTime.UtcNow.AddHours(24)
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Pacjent)
            .Include(u => u.Lekarz)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

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

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Pomyślnie wylogowano" });
    }
}