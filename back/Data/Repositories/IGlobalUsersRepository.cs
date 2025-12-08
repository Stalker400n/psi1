using back.Models;

namespace back.Data.Repositories;

public interface IGlobalUsersRepository
{
  Task<GlobalUser?> GetByIdAsync(int id);
  Task<GlobalUser?> GetByNameAsync(string name);
  Task<GlobalUser> CreateAsync(GlobalUser user);
  Task<GlobalUser?> UpdateAsync(int id, GlobalUser user);
  Task<bool> DeleteAsync(int id);
}
