using Farmora.Application.Users;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Users;

public sealed class UserRepository(FarmoraDbContext db) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => db.Users.AnyAsync(x => x.Email == email, cancellationToken);
    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken) => db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task AddAsync(User user, CancellationToken cancellationToken) => db.Users.AddAsync(user, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
