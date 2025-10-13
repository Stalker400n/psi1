using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly ApplicationDbContext _context;

    public UsersRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>?> GetUsersAsync(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        return team.Users.OrderBy(u => u.JoinedAt).ToList();
    }

    public async Task<User?> GetUserAsync(int teamId, int userId)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        return team.Users.FirstOrDefault(u => u.Id == userId);
    }

    public async Task<User?> CreateUserAsync(int teamId, User user)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        user.JoinedAt = DateTime.UtcNow;

        // Mirror controller: add user, save, then associate to team and save
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        team.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(int teamId, int userId, User user)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        var existing = team.Users.FirstOrDefault(u => u.Id == userId);
        if (existing == null) return null;

        existing.Name = user.Name;
        existing.Score = user.Score;
        existing.IsActive = user.IsActive;

        _context.Entry(existing).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteUserAsync(int teamId, int userId)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);
        if (team == null) return false;

        var user = team.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return false;

        team.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }
}