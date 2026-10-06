using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class UserRepository(FarmoraDbContext db) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var emailExist = db.Users.AnyAsync(x => x.Email == email, cancellationToken);
        return emailExist;
    }
    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.Users.AsNoTracking().OrderBy(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
    }
    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        return db.Users.AddAsync(user, cancellationToken).AsTask();
    }
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
