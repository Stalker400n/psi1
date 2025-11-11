using back.Models.Enums;

namespace back.Models;

public class RoleChangeRequest
{
    public Role Role { get; set; }
    public int RequestingUserId { get; set; }
}
