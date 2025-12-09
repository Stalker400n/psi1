using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace back.Data.Repositories;

public class TeamsRepository : ITeamsRepository
{
	private readonly ApplicationDbContext _context;

	public TeamsRepository(ApplicationDbContext context)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		_context = context;
	}

	public async Task<IEnumerable<Team>> GetAllAsync()
	{
		return await _context.Teams
			.Include(t => t.Users)
			.Include(t => t.Songs)
			.Include(t => t.Messages)
			.ToListAsync();
	}

	public async Task<Team?> GetByIdAsync(int id)
	{
		return await _context.Teams
			.Include(t => t.Users)
			.Include(t => t.Songs)
			.Include(t => t.Messages)
			.FirstOrDefaultAsync(t => t.Id == id);
	}

	public async Task<Team> CreateAsync(Team team)
	{
		int newId;
		do
		{
			newId = RandomNumberGenerator.GetInt32(100000, 1000000);
		}
		while (await _context.Teams.AnyAsync(t => t.Id == newId));

		team.Id = newId;
		_context.Teams.Add(team);
		await _context.SaveChangesAsync();
		return team;
	}

	public async Task<Team?> UpdateAsync(int id, Team team)
	{
		var current = await _context.Teams.FindAsync(id);
		if (current == null) return null;

		current.Name = team.Name;
		current.IsPrivate = team.IsPrivate;
		current.IsPlaying = team.IsPlaying;
		current.CurrentSongIndex = team.CurrentSongIndex;

		_context.Entry(current).State = EntityState.Modified;
		await _context.SaveChangesAsync();

		return current;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var team = await _context.Teams.FindAsync(id);
		if (team == null) return false;

		_context.Teams.Remove(team);
		await _context.SaveChangesAsync();
		return true;
	}
}

