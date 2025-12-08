import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import api from "../services/api.service";
import type { Team, User } from "../services/api.service";
import { PlaylistPage } from "./playlist.page";
import { renderPulsingStar, floatingQuotesCSS } from "../utils/praise.utils";
import { useToast } from "../contexts/toast-context";

interface TeamPageProps {
  user: User;
  onLeave: () => void;
}

export function TeamPage({ user, onLeave }: TeamPageProps) {
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
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

  const handleBack = () => {
    onLeave();
    navigate(-1);
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
    <div className="min-h-screen bg-slate-950 relative">
      <button 
        onClick={handleBack} 
        className="absolute top-8 left-8 text-slate-400 hover:text-white flex items-center gap-2 transition"
      >
        <ArrowLeft size={20} />
        Back
      </button>

      <div className="bg-slate-900 border-b border-slate-800 p-4">
        <div className="max-w-7xl mx-auto flex justify-center items-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-white">
              {team.name}
              {renderPulsingStar({
                className: "text-yellow-400",
                size: "1.2em",
              })}
            </h1>
            <p className="text-slate-400 text-sm">
              Code: {team.id}
            </p>
          </div>
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
        <PlaylistPage
          teamId={parseInt(teamId)}
          userId={user.id}
          userName={user.name}
        />
      </div>

      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
