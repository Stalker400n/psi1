using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models;
using back.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly ApplicationDbContext _context;

    public UsersRepository(ApplicationDbContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

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

        // If this is the first user in the team, make them the Owner
        if (team.Users == null || team.Users.Count == 0)
        {
            user.Role = back.Models.Enums.Role.Owner;
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        team.Users ??= new System.Collections.Generic.List<User>();
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
        // allow updating role as well
        existing.Role = user.Role;

        _context.Entry(existing).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<User?> ChangeUserRoleAsync(int teamId, int userId, back.Models.Enums.Role newRole)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        var existing = team.Users.FirstOrDefault(u => u.Id == userId);
        if (existing == null) return null;

        existing.Role = newRole;
        _context.Entry(existing).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<User?> ChangeUserRoleAsync(int teamId, int userId, back.Models.Enums.Role newRole, int requestingUserId)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        var requestingUser = team.Users.FirstOrDefault(u => u.Id == requestingUserId);
        if (requestingUser == null)
            throw new RoleChangeException("Requesting user not found in team");

        // Only owners and moderators can change roles
        if (requestingUser.Role != back.Models.Enums.Role.Owner &&
            requestingUser.Role != back.Models.Enums.Role.Moderator)
            throw new RoleChangeException("Only owners and moderators can change user roles");

        var userToUpdate = team.Users.FirstOrDefault(u => u.Id == userId);
        if (userToUpdate == null) return null;

        // Prevent removing the last owner
        if (userToUpdate.Role == back.Models.Enums.Role.Owner &&
            newRole != back.Models.Enums.Role.Owner)
        {
            var ownerCount = team.Users.Count(u => u.Role == back.Models.Enums.Role.Owner);
            if (ownerCount == 1)
                throw new RoleChangeException("A team must have at least one owner");
        }

        userToUpdate.Role = newRole;
        _context.Entry(userToUpdate).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return userToUpdate;
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