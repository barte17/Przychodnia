using Microsoft.AspNetCore.Identity;
using Backend.Models.Relational;

namespace Backend.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = serviceProvider.GetRequiredService<RelationalDbContext>();

        // Tworzenie ról jeśli nie istnieją
        string[] roles = { "Admin", "Doctor", "Patient" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ========== LEKARZE ==========
        
        // Lekarz 1
        var lekarz1Email = "lek@pans.pl";
        if (await userManager.FindByEmailAsync(lekarz1Email) == null)
        {
            var user1 = new User
            {
                UserName = lekarz1Email,
                Email = lekarz1Email,
                FirstName = "Jan",
                LastName = "Kowalski",
                Role = "Doctor",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user1, "Haslo!23");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user1, "Doctor");
                
                var lekarz1 = new Lekarz
                {
                    Imie = "Jan",
                    Nazwisko = "Kowalski",
                    Specjalizacja = "Kardiolog",
                    UserId = user1.Id
                };
                dbContext.Lekarze.Add(lekarz1);
            }
        }

        // Lekarz 2
        var lekarz2Email = "lek2@pans.pl";
        if (await userManager.FindByEmailAsync(lekarz2Email) == null)
        {
            var user2 = new User
            {
                UserName = lekarz2Email,
                Email = lekarz2Email,
                FirstName = "Anna",
                LastName = "Nowak",
                Role = "Doctor",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user2, "Haslo!23");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user2, "Doctor");
                
                var lekarz2 = new Lekarz
                {
                    Imie = "Anna",
                    Nazwisko = "Nowak",
                    Specjalizacja = "Neurolog",
                    UserId = user2.Id
                };
                dbContext.Lekarze.Add(lekarz2);
            }
        }

        // ========== PACJENCI ==========
        
        // Pacjent 1
        var pacjent1Email = "pac@pans.pl";
        if (await userManager.FindByEmailAsync(pacjent1Email) == null)
        {
            var user3 = new User
            {
                UserName = pacjent1Email,
                Email = pacjent1Email,
                FirstName = "Piotr",
                LastName = "Wiśniewski",
                Role = "Patient",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user3, "Haslo!23");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user3, "Patient");
                
                var pacjent1 = new Pacjent
                {
                    Imie = "Piotr",
                    Nazwisko = "Wiśniewski",
                    PESEL = "90010112345",
                    UserId = user3.Id
                };
                dbContext.Pacjenci.Add(pacjent1);
            }
        }

        // Pacjent 2
        var pacjent2Email = "pac2@pans.pl";
        if (await userManager.FindByEmailAsync(pacjent2Email) == null)
        {
            var user4 = new User
            {
                UserName = pacjent2Email,
                Email = pacjent2Email,
                FirstName = "Maria",
                LastName = "Zielińska",
                Role = "Patient",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user4, "Haslo!23");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user4, "Patient");
                
                var pacjent2 = new Pacjent
                {
                    Imie = "Maria",
                    Nazwisko = "Zielińska",
                    PESEL = "85052298765",
                    UserId = user4.Id
                };
                dbContext.Pacjenci.Add(pacjent2);
            }
        }

        await dbContext.SaveChangesAsync();
        
        Console.WriteLine("Seed data initialized successfully!");
        Console.WriteLine("Lekarze: lek@pans.pl, lek2@pans.pl (hasło: Haslo!23)");
        Console.WriteLine("Pacjenci: pac@pans.pl, pac2@pans.pl (hasło: Haslo!23)");
    }
}
