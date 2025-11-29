import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { LogOut } from "lucide-react";
import api from "../services/api.service";
import type { Team, User } from "../services/api.service";
import { PlaylistView } from "./PlaylistView";
import { renderPulsingStar, floatingQuotesCSS } from "../utils/praises";
import { useToast } from "../contexts/ToastContext";

interface TeamViewProps {
  user: User;
  onLeave: () => void;
}

export function TeamView({ user, onLeave }: TeamViewProps) {
  const { teamId } = useParams<{ teamId: string }>();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [team, setTeam] = useState<Team | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
      showToast("Team not found", "error");
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
        {error && (
          <div className="mb-4 p-4 bg-red-900/30 border border-red-600 rounded-lg text-red-300">
            {error}
            <button
              onClick={() => setError(null)}
              className="ml-2 underline hover:text-red-200"
            >
              Dismiss
            </button>
          </div>
        )}
        <PlaylistView
          teamId={parseInt(teamId)}
          userId={user.id}
          userName={user.name}
        />
      </div>

      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
