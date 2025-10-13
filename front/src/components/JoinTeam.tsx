import { useState } from 'react';
import api from '../services/api.service';
import type { Team, User } from '../services/api.service';

interface JoinTeamProps {
  userName: string;
  onBack: () => void;
  onTeamJoined: (team: Team, user: User) => void;
}

export function JoinTeam({ userName, onBack, onTeamJoined }: JoinTeamProps) {
  const [code, setCode] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  const handleJoin = async () => {
    if (!code) return;
    setLoading(true);

    try {
      const team = await api.teamsApi.getById(parseInt(code));
      const user = await api.usersApi.add(team.id, { name: userName, score: 0, isActive: true });
      onTeamJoined(team, user);
    } catch (error) {
      console.error('Error joining team:', error);
      alert('Failed to join team. Check the code.');
    }
    setLoading(false);
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button onClick={onBack} className="text-slate-400 hover:text-white mb-8">← Back</button>
      
      <div className="max-w-md mx-auto">
        <h1 className="text-4xl font-bold text-white mb-8">Join Team</h1>
        
        <input
          type="text"
          placeholder="Enter team code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          className="w-full px-4 py-3 bg-slate-800 text-white rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
          onKeyPress={(e) => e.key === 'Enter' && handleJoin()}
        />
        
        <button
          onClick={handleJoin}
          disabled={loading || !code}
          className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50"
        >
          {loading ? 'Joining...' : 'Join Team'}
        </button>
      </div>
    </div>
  );
}
