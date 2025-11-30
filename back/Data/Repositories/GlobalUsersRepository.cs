using Microsoft.EntityFrameworkCore;
using back.Models;

namespace back.Data.Repositories;

public class GlobalUsersRepository : IGlobalUsersRepository
{
  private readonly ApplicationDbContext _context;

  public GlobalUsersRepository(ApplicationDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public async Task<GlobalUser?> GetByIdAsync(int id)
  {
    return await _context.GlobalUsers.FindAsync(id);
  }

  public async Task<GlobalUser?> GetByNameAsync(string name)
  {
    return await _context.GlobalUsers
      .FirstOrDefaultAsync(u => u.Name == name);
  }

  public async Task<GlobalUser> CreateAsync(GlobalUser user)
  {
    _context.GlobalUsers.Add(user);
    await _context.SaveChangesAsync();
    return user;
  }

  public async Task<GlobalUser?> UpdateAsync(int id, GlobalUser user)
  {
    var existing = await _context.GlobalUsers.FindAsync(id);
    if (existing == null) return null;

    existing.Name = user.Name;
    existing.DeviceFingerprint = user.DeviceFingerprint;
    existing.LastSeenAt = user.LastSeenAt;
    existing.DeviceInfo = user.DeviceInfo;

    await _context.SaveChangesAsync();
    return existing;
  }

  public async Task<bool> DeleteAsync(int id)
  {
    var user = await _context.GlobalUsers.FindAsync(id);
    if (user == null) return false;

    _context.GlobalUsers.Remove(user);
    await _context.SaveChangesAsync();
    return true;
  }
}
