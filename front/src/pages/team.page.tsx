import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Settings, X, Check, Lock, Globe } from "lucide-react";
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
  const [users, setUsers] = useState<User[]>([]);
  
  // Settings modal state
  const [showSettings, setShowSettings] = useState(false);
  const [editedName, setEditedName] = useState("");
  const [editedIsPrivate, setEditedIsPrivate] = useState(false);
  const [saving, setSaving] = useState(false);

  // Get current user and check permissions
  const currentUser = users.find((u) => u.id === user.id);
  const canEditSettings = 
    currentUser?.role === "Owner" || currentUser?.role === "Moderator";

  useEffect(() => {
    if (teamId) {
      fetchTeam();
      fetchUsers();

      // Poll for changes every 3 seconds
      const interval = setInterval(() => {
        fetchTeam();
        fetchUsers();
      }, 3000);

      return () => clearInterval(interval);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teamId]);

  const fetchTeam = async () => {
    if (!teamId) return;
    try {
      const data = await api.teamsApi.getById(parseInt(teamId));
      setTeam(data);
      setEditedName(data.name);
      setEditedIsPrivate(data.isPrivate || false);
    } catch (error) {
      console.error("Error fetching team:", error);
      showToast("Team not found", "error");
      navigate("/");
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    if (!teamId) return;
    try {
      const userData = await api.usersApi.getAll(parseInt(teamId));
      setUsers(userData);
    } catch (error) {
      console.error("Error fetching users:", error);
      setUsers([]);
    }
  };

  const handleBack = () => {
    onLeave();
    navigate(-1);
  };

  const openSettings = () => {
    if (!team) return;
    if (!canEditSettings) {
      showToast("Only team owners and moderators can edit settings", "error");
      return;
    }
    setEditedName(team.name);
    setEditedIsPrivate(team.isPrivate || false);
    setShowSettings(true);
  };

  const closeSettings = () => {
    setShowSettings(false);
    if (team) {
      setEditedName(team.name);
      setEditedIsPrivate(team.isPrivate || false);
    }
  };

  const handleSaveSettings = async () => {
    if (!editedName.trim()) {
      showToast("Team name cannot be empty", "error");
      return;
    }

    if (!teamId) return;

    setSaving(true);
    try {
      const updated = await api.teamsApi.update(parseInt(teamId), {
        name: editedName.trim(),
        isPrivate: editedIsPrivate
      });
      
      setTeam(updated);
      showToast("Team settings updated successfully", "success");
      setShowSettings(false);
    } catch (error) {
      console.error("Error updating team:", error);
      showToast("Failed to update team settings", "error");
    } finally {
      setSaving(false);
    }
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
        className="absolute top-8 left-8 text-slate-400 hover:text-white flex items-center gap-2 transition z-10"
      >
        <ArrowLeft size={20} />
        Back
      </button>

      <button
        onClick={openSettings}
        className={`absolute top-8 right-8 flex items-center gap-2 transition z-10 ${
          canEditSettings
            ? "text-slate-400 hover:text-white cursor-pointer"
            : "text-slate-600 cursor-not-allowed"
        }`}
        disabled={!canEditSettings}
        title={canEditSettings ? "Team Settings" : "Only owners and moderators can edit settings"}
      >
        <Settings size={20} />
        Settings
      </button>

      <div className="bg-slate-900 border-b border-slate-800 p-4">
        <div className="max-w-7xl mx-auto flex justify-center items-center">
          <div className="text-center">
            <div className="flex items-center justify-center gap-2">
              <h1 className="text-2xl font-bold text-white">
                {team.name}
                {renderPulsingStar({
                  className: "text-yellow-400",
                  size: "1.2em",
                })}
              </h1>
              <div title={team.isPrivate ? "Private team" : "Public team"}>
                {team.isPrivate ? (
                  <Lock size={20} className="text-slate-400" />
                ) : (
                  <Globe size={20} className="text-slate-400" />
                )}
              </div>
            </div>
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

      {/* Settings Modal */}
      {showSettings && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 rounded-lg border border-slate-800 max-w-md w-full">
            <div className="flex items-center justify-between p-4 border-b border-slate-800">
              <h2 className="text-xl font-bold text-white">Team Settings</h2>
              <button
                onClick={closeSettings}
                className="text-slate-400 hover:text-white transition"
              >
                <X size={24} />
              </button>
            </div>

            <div className="p-6 space-y-6">
              {/* Team Name */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">
                  Team Name
                </label>
                <input
                  type="text"
                  value={editedName}
                  onChange={(e) => setEditedName(e.target.value)}
                  className="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Enter team name"
                  maxLength={50}
                />
              </div>

              {/* Privacy Setting */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-3">
                  Privacy
                </label>
                <div className="space-y-2">
                  <button
                    onClick={() => setEditedIsPrivate(false)}
                    className={`w-full flex items-center justify-between p-4 rounded-lg border transition ${
                      !editedIsPrivate
                        ? "bg-blue-500/10 border-blue-500 text-blue-300"
                        : "bg-slate-800 border-slate-700 text-slate-300 hover:border-slate-600"
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <Globe size={20} />
                      <div className="text-left">
                        <div className="font-medium">Public</div>
                        <div className="text-xs opacity-75">Anyone can join with the code</div>
                      </div>
                    </div>
                    {!editedIsPrivate && <Check size={20} />}
                  </button>

                  <button
                    onClick={() => setEditedIsPrivate(true)}
                    className={`w-full flex items-center justify-between p-4 rounded-lg border transition ${
                      editedIsPrivate
                        ? "bg-blue-500/10 border-blue-500 text-blue-300"
                        : "bg-slate-800 border-slate-700 text-slate-300 hover:border-slate-600"
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <Lock size={20} />
                      <div className="text-left">
                        <div className="font-medium">Private</div>
                        <div className="text-xs opacity-75">Only invited members can join</div>
                      </div>
                    </div>
                    {editedIsPrivate && <Check size={20} />}
                  </button>
                </div>
              </div>
            </div>

            <div className="flex gap-3 p-4 border-t border-slate-800">
              <button
                onClick={closeSettings}
                className="flex-1 px-4 py-2 bg-slate-800 text-slate-300 rounded-lg hover:bg-slate-700 transition"
                disabled={saving}
              >
                Cancel
              </button>
              <button
                onClick={handleSaveSettings}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-500 transition disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={saving}
              >
                {saving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </div>
        </div>
      )}

      <style>{floatingQuotesCSS}</style>
    </div>
  );
}