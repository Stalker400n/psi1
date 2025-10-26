import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import api from '../services/api.service';
import type { Team, User } from '../services/api.service';

interface BrowseTeamsProps {
  userName: string;
  onUserCreated: (user: User) => void;
}

export function BrowseTeams({ userName, onUserCreated }: BrowseTeamsProps) {
  const navigate = useNavigate();
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
      onUserCreated(user);
      navigate(`/teams/${team.id}`);
    } catch (error) {
      console.error('Error joining team:', error);
      alert('Failed to join team');
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button 
        onClick={() => navigate('/')} 
        className="text-slate-400 hover:text-white mb-8 flex items-center gap-2"
      >
        <ArrowLeft size={20} />
        Back
      </button>
      
      <h1 className="text-4xl font-bold text-white mb-8">
        Public Teams<span className="text-yellow-400">.</span>
      </h1>
      
      {loading ? (
        <p className="text-slate-400">Loading...</p>
      ) : teams.length === 0 ? (
        <p className="text-slate-400">No public teams available</p>
      ) : (
        <div className="grid gap-4 max-w-2xl">
          {teams.map(team => (
            <div key={team.id} className="bg-slate-800 p-6 rounded-lg flex justify-between items-center hover:bg-slate-750 transition">
              <div>
                <h3 className="text-xl text-white font-semibold">{team.name}</h3>
                <p className="text-slate-400 text-sm">Code: {team.id}</p>
                <p className="text-slate-500 text-xs mt-1">{team.users?.length || 0} members</p>
              </div>
              <button
                onClick={() => handleJoin(team)}
                className="px-6 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-semibold"
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