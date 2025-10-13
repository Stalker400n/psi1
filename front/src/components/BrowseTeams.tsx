import { useState, useEffect } from 'react';
import api from '../services/api.service';
import type { Team, User } from '../services/api.service';

interface BrowseTeamsProps {
  onBack: () => void;
  onJoinTeam: (team: Team, user: User) => void;
  userName: string;
}

export function BrowseTeams({ onBack, onJoinTeam, userName }: BrowseTeamsProps) {
  const [teams, setTeams] = useState<Team[]>([]);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    fetchTeams();
  }, []);

  const fetchTeams = async () => {
    try {
      const data = await api.teamsApi.getAll();
      setTeams(data.filter((t: Team) => !t.isPrivate));
    } catch (error) {
      console.error('Error fetching teams:', error);
    }
    setLoading(false);
  };

  const handleJoin = async (team: Team) => {
    try {
      const user = await api.usersApi.add(team.id, { name: userName, score: 0, isActive: true });
      onJoinTeam(team, user);
    } catch (error) {
      console.error('Error joining team:', error);
      alert('Failed to join team');
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button onClick={onBack} className="text-slate-400 hover:text-white mb-8">← Back</button>
      
      <h1 className="text-4xl font-bold text-white mb-8">Public Teams</h1>
      
      {loading ? (
        <p className="text-slate-400">Loading...</p>
      ) : teams.length === 0 ? (
        <p className="text-slate-400">No public teams available</p>
      ) : (
        <div className="grid gap-4 max-w-2xl">
          {teams.map(team => (
            <div key={team.id} className="bg-slate-800 p-6 rounded-lg flex justify-between items-center">
              <div>
                <h3 className="text-xl text-white font-semibold">{team.name}</h3>
                <p className="text-slate-400 text-sm">Code: {team.id}</p>
              </div>
              <button
                onClick={() => handleJoin(team)}
                className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
              >
                Join
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
