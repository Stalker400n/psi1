import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import api from '../services/api.service';
import type { User } from '../services/api.service';

interface JoinTeamProps {
  userName: string;
  onUserCreated: (user: User) => void;
}

export function JoinTeam({ userName, onUserCreated }: JoinTeamProps) {
  const navigate = useNavigate();
  const [code, setCode] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  const handleJoin = async () => {
    if (!code) return;
    setLoading(true);

    try {
      const team = await api.teamsApi.getById(parseInt(code));
      const user = await api.usersApi.add(team.id, { name: userName, score: 0, isActive: true });
      onUserCreated(user);
      navigate(`/teams/${team.id}`);
    } catch (error) {
      console.error('Error joining team:', error);
      alert('Failed to join team. Check the code.');
    }
    setLoading(false);
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
      
      <div className="max-w-md mx-auto">
        <h1 className="text-4xl font-bold text-white mb-8">
          Join Team<span className="text-yellow-400">.</span>
        </h1>
        
        <input
          type="text"
          placeholder="Enter team code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          className="w-full px-4 py-3 bg-slate-800 text-white rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-yellow-500"
          onKeyPress={(e) => e.key === 'Enter' && handleJoin()}
        />
        
        <button
          onClick={handleJoin}
          disabled={loading || !code}
          className="w-full px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 font-semibold"
        >
          {loading ? 'Joining...' : 'Join Team'}
        </button>
      </div>
    </div>
  );
}