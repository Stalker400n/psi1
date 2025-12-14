import { useState } from "react";
import api from "../services/api.service";
import type { User } from "../services/api.service";
import { useToast } from "../contexts/toast-context";
import { getTimeAgo } from "../utils/time.utils";

interface UsersPanelProps {
  users: User[];
  teamId: number;
  userRole: "Member" | "Moderator" | "Owner";
  userId: number;
}

export function UsersPanel({
  users,
  teamId,
  userRole,
  userId,
}: UsersPanelProps) {
  const { showToast } = useToast();
  const [isChangingRole, setIsChangingRole] = useState(false);

  const currentUser = users.find((user) => user.id === userId);
  const otherUsers = users
    .filter((user) => user.id !== userId)
    .sort((a, b) => {
      const roleOrder = { Owner: 0, Moderator: 1, Member: 2 };
      const roleComparison = roleOrder[a.role] - roleOrder[b.role];

      if (roleComparison === 0) {
        return a.name.localeCompare(b.name);
      }

      return roleComparison;
    });

  const handleRoleChange = async (
    targetUserId: number,
    newRole: "Member" | "Moderator" | "Owner"
  ) => {
    if (isChangingRole) return;

    try {
      setIsChangingRole(true);

      await api.usersApi.changeRole(teamId, targetUserId, newRole, userId);

      showToast(`Role updated to ${newRole}`, "success");
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to change role";
      console.error("Failed to change role", err);
      showToast(`Error: ${errorMessage}`, "error");
    } finally {
      setIsChangingRole(false);
    }
  };

  return (
    <div className="h-full flex flex-col">
      <h3 className="text-white font-semibold mb-3 text-sm">
        Team Members ({users.length})
      </h3>

      <div
        className="flex-1 overflow-y-auto space-y-2"
        style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
      >
        {currentUser && (
          <>
            <div className="bg-yellow-500/20 border border-yellow-500 p-3 rounded-lg">
              <div className="flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-white font-medium text-sm">
                      {currentUser.name}
                    </p>
                    <span className="text-yellow-400 text-xs font-bold">
                      (You)
                    </span>
                  </div>
                  <p className="text-slate-400 text-xs">
                    {getTimeAgo(currentUser.joinedAt)}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <span
                    className={`text-xs px-2 py-1 rounded ${
                      currentUser.role === "Owner"
                        ? "bg-yellow-500 text-black"
                        : currentUser.role === "Moderator"
                        ? "bg-indigo-600 text-white"
                        : "bg-slate-700 text-slate-200"
                    }`}
                  >
                    {currentUser.role}
                  </span>

                  {(userRole === "Owner" || userRole === "Moderator") &&
                    currentUser.role !== "Owner" && (
                      <select
                        value={currentUser.role}
                        onChange={(e) => {
                          const newRole = e.target.value as
                            | "Member"
                            | "Moderator"
                            | "Owner";
                          handleRoleChange(currentUser.id, newRole);
                        }}
                        disabled={isChangingRole}
                        className="bg-slate-700 text-white rounded px-2 py-1 text-xs"
                      >
                        <option value="Member">Member</option>
                        <option value="Moderator">Moderator</option>
                      </select>
                    )}
                </div>
              </div>
            </div>

            <div className="border-t border-slate-700 my-2"></div>
          </>
        )}

        {otherUsers.map((user) => (
          <div key={user.id} className="bg-slate-800 p-3 rounded-lg">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-white font-medium text-sm">{user.name}</p>
                <p className="text-slate-400 text-xs">
                  {getTimeAgo(user.joinedAt)}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <span
                  className={`text-xs px-2 py-1 rounded ${
                    user.role === "Owner"
                      ? "bg-yellow-500 text-black"
                      : user.role === "Moderator"
                      ? "bg-indigo-600 text-white"
                      : "bg-slate-700 text-slate-200"
                  }`}
                >
                  {user.role}
                </span>

                {(userRole === "Owner" || userRole === "Moderator") &&
                  user.role !== "Owner" && (
                    <select
                      value={user.role}
                      onChange={(e) => {
                        const newRole = e.target.value as
                          | "Member"
                          | "Moderator"
                          | "Owner";
                        handleRoleChange(user.id, newRole);
                      }}
                      disabled={isChangingRole}
                      className="bg-slate-700 text-white rounded px-2 py-1 text-xs"
                    >
                      <option value="Member">Member</option>
                      <option value="Moderator">Moderator</option>
                    </select>
                  )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
