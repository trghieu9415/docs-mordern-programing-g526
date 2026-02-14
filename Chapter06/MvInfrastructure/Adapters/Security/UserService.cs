using MvApplication.Models;
using MvApplication.Ports.Security;
using MvInfrastructure.Identity;
using MvInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MvInfrastructure.Adapters.Security;

public class UserService(AppDbContext context) : IUserService
{

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var appUser = await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        return appUser?.ToUser();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var appUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return appUser?.ToUser();
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
    {
        var appUsers = await context.Users.ToListAsync(ct);
        return appUsers.Select(u => u.ToUser());
    }
}
