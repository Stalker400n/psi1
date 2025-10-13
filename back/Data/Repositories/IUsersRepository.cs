using back.Models;

namespace back.Data.Repositories;

public interface IUsersRepository
{
	Task<IEnumerable<User>> GetUsersAsync(int teamId);

	Task<User?> GetUserAsync(int teamId, int userId);

	Task<User> CreateUserAsync(int teamId, User user);

	Task<User?> UpdateUserAsync(int teamId, int userId, User user);

	Task<bool> DeleteUserAsync(int teamId, int userId);
}

