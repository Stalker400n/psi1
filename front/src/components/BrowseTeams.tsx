import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Users } from 'lucide-react';
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
        className="text-slate-400 hover:text-white mb-8 flex items-center gap-2 transition"
      >
        <ArrowLeft size={20} />
        Back
      </button>
      
      <h1 className="text-4xl font-bold text-white mb-2 text-center">
        Public Teams<span className="text-yellow-400">.</span>
      </h1>
      <p className="text-slate-400 text-center mb-8">
        {loading ? 'Loading...' : `${teams.length} ${teams.length === 1 ? 'team' : 'teams'} available`}
      </p>
      
      <div className="max-w-6xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {loading ? (
            Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-slate-800/50 p-6 rounded-lg animate-pulse">
                <div className="h-8 bg-slate-700 rounded mb-3"></div>
                <div className="h-4 bg-slate-700 rounded w-2/3 mb-2"></div>
                <div className="h-4 bg-slate-700 rounded w-1/2 mb-4"></div>
                <div className="h-10 bg-slate-700 rounded"></div>
              </div>
            ))
          ) : teams.length === 0 ? (
            Array.from({ length: 6 }).map((_, i) => (
              <div 
                key={i} 
                className="bg-slate-900/30 border-2 border-dashed border-slate-800 p-6 rounded-lg flex flex-col items-center justify-center min-h-[200px]"
              >
                <Users size={32} className="text-slate-700 mb-2" />
                <p className="text-slate-600 text-sm">No team yet</p>
              </div>
            ))
          ) : (
            <>
              {teams.map(team => (
                <div 
                  key={team.id} 
                  className="bg-slate-800 p-6 rounded-lg hover:bg-slate-750 transition flex flex-col border border-slate-700 hover:border-yellow-500/50"
                >
                  <div className="flex-1 mb-4">
                    <h3 className="text-2xl text-white font-bold mb-2">{team.name}</h3>
                    <p className="text-slate-400 text-sm mb-1">Code: {team.id}</p>
                    <div className="flex items-center gap-2 text-slate-500 text-xs">
                      <Users size={14} />
                      <span>{team.users?.length || 0} {team.users?.length === 1 ? 'member' : 'members'}</span>
                    </div>
                  </div>
                  <button
                    onClick={() => handleJoin(team)}
                    className="w-full px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-semibold"
                  >
                    Join Team
                  </button>
                </div>
              ))}
              
              {Array.from({ length: Math.max(0, 6 - teams.length) }).map((_, i) => (
                <div 
                  key={`empty-${i}`} 
                  className="bg-slate-900/30 border-2 border-dashed border-slate-800 p-6 rounded-lg flex flex-col items-center justify-center min-h-[200px]"
                >
                  <Users size={32} className="text-slate-700 mb-2" />
                  <p className="text-slate-600 text-sm">No team yet</p>
                </div>
              ))}
            </>
          )}
        </div>
      </div>
    </div>
  );
}