using MvApplication.Models;
using MvApplication.Ports.Security;
using MvInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MvInfrastructure.Adapters.Security;

public class UserService(AppDbContext context) : IUserService
{

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var applicationUser = await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        return applicationUser?.ToUser();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var applicationUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return applicationUser?.ToUser();
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
    {
        var applicationUsers = await context.Users.ToListAsync(ct);
        return applicationUsers.Select(u => u.ToUser());
    }
}
