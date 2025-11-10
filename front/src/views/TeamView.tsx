import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Music, MessageSquare, LogOut, Trophy } from "lucide-react";
import api from "../services/api.service";
import type { Team, User } from "../services/api.service";
import { PlaylistView } from "./PlaylistView";
import { ChatView } from "./ChatView";
import { Leaderboard } from "../components/Leaderboard";
import { renderPulsingStar, floatingQuotesCSS } from "../utils/praises";

interface TeamViewProps {
  user: User;
  onLeave: () => void;
}

export function TeamView({ user, onLeave }: TeamViewProps) {
  const { teamId } = useParams<{ teamId: string }>();
  const navigate = useNavigate();
  const [team, setTeam] = useState<Team | null>(null);
  const [view, setView] = useState<"playlist" | "chat" | "leaderboard">("playlist");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (teamId) {
      fetchTeam();
    }
  }, [teamId]);

  const fetchTeam = async () => {
    if (!teamId) return;

    try {
      const data = await api.teamsApi.getById(parseInt(teamId));
      setTeam(data);
    } catch (error) {
      console.error("Error fetching team:", error);
      alert("Team not found");
      navigate("/");
    } finally {
      setLoading(false);
    }
  };

  const handleLeave = () => {
    onLeave();
    navigate("/");
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex items-center justify-center">
        <p className="text-slate-400">Loading team...</p>
      </div>
    );
  }

  if (!team || !teamId) {
    return null;
  }

  return (
    <div className="min-h-screen bg-slate-950">
      <div className="bg-slate-900 border-b border-slate-800 p-4">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-white">
              {team.name}
              {renderPulsingStar({
                className: "text-yellow-400",
                size: "1.2em",
              })}
            </h1>
            <p className="text-slate-400 text-sm">
              Code: {team.id} • {user.name}
            </p>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-slate-300 text-sm">Users:</div>
            <div className="flex items-center gap-3">
              {team.users.slice(0, 5).map((u) => (
                <div
                  key={u.id}
                  className="px-3 py-1 bg-slate-800 rounded-md text-sm text-white flex items-center gap-2"
                >
                  <span className="font-semibold">{u.name}</span>
                  <span className="text-xs text-yellow-300">{u.role}</span>
                </div>
              ))}
            </div>
          </div>
          <button
            onClick={handleLeave}
            className="px-4 py-2 bg-slate-800 text-white rounded-lg hover:bg-slate-700 transition flex items-center gap-2"
          >
            <LogOut size={18} />
            Leave
          </button>
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-4">
        <div className="flex gap-2 mb-6">
          <button
            onClick={() => setView("playlist")}
            className={`px-6 py-2 rounded-lg transition font-semibold ${
              view === "playlist"
                ? "bg-yellow-500 text-black"
                : "bg-slate-800 text-slate-400 hover:bg-slate-700"
            }`}
          >
            <Music size={18} className="inline mr-2" />
            Playlist
          </button>
          <button
            onClick={() => setView("leaderboard")}
            className={`px-6 py-2 rounded-lg transition font-semibold ${
              view === "leaderboard"
                ? "bg-yellow-500 text-black"
                : "bg-slate-800 text-slate-400 hover:bg-slate-700"
            }`}
          >
            <Trophy size={18} className="inline mr-2" />
            Leaderboard
          </button>
          <button
            onClick={() => setView("chat")}
            className={`px-6 py-2 rounded-lg transition font-semibold ${
              view === "chat"
                ? "bg-yellow-500 text-black"
                : "bg-slate-800 text-slate-400 hover:bg-slate-700"
            }`}
          >
            <MessageSquare size={18} className="inline mr-2" />
            Chat
          </button>
        </div>

        <div className="mb-6">
          <h3 className="text-white font-semibold mb-2">Team Members</h3>
          <div className="grid grid-cols-1 gap-2 max-w-md">
            {team.users.map((u) => (
              <div
                key={u.id}
                className="flex items-center justify-between bg-slate-800 p-2 rounded-md"
              >
                <div>
                  <div className="text-white font-medium">{u.name}</div>
                  <div className="text-slate-400 text-xs">
                    Joined: {new Date(u.joinedAt).toLocaleString()}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <div className="px-2 py-1 bg-slate-700 text-slate-200 rounded text-xs">
                    {u.role}
                  </div>
                  {user.role === "Owner" && (
                    <select
                      value={u.role}
                      onChange={async (e) => {
                        const newRole = e.target.value as
                          | "Member"
                          | "Moderator"
                          | "Owner";
                        try {
                          await api.usersApi.changeRole(team.id, u.id, newRole);
                          await fetchTeam();
                        } catch (err) {
                          console.error("Failed to change role", err);
                          alert("Failed to change role");
                        }
                      }}
                      className="bg-slate-700 text-white rounded px-2 py-1 text-sm"
                    >
                      <option value="Member">Member</option>
                      <option value="Moderator">Moderator</option>
                      <option value="Owner">Owner</option>
                    </select>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>

        {view === "playlist" && (
          <PlaylistView
            teamId={parseInt(teamId)}
            userId={user.id}
            userName={user.name}
          />
        )}
        {view === "leaderboard" && (
          <Leaderboard
            teamId={parseInt(teamId)}
            userId={user.id}
          />
        )}
        {view === "chat" && (
          <ChatView teamId={parseInt(teamId)} userName={user.name} />
        )}
      </div>

      <style>{floatingQuotesCSS}</style>
    </div>
  );
}