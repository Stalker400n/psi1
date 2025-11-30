import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import api from '../services/api.service';
import type { User } from '../services/api.service';
import { renderPulsingStar, floatingQuotesCSS } from '../utils/praises';
import { useToast } from '../contexts/ToastContext';

interface JoinTeamProps {
  userName: string;
  onUserCreated: (user: User) => void;
}

export function JoinTeam({ userName, onUserCreated }: JoinTeamProps) {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [code, setCode] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  const handleJoin = async () => {
    if (!code) return;
    setLoading(true);

    try {
      const team = await api.teamsApi.getById(parseInt(code));
      
      // Check if user already in team
      const existingUser = team.users?.find(u => u.name === userName);
      
      if (existingUser) {
        // User already exists - use existing user
        onUserCreated(existingUser);
        navigate(`/teams/${team.id}`);
        setLoading(false);
        return;
      }
      
      // User doesn't exist - create new
      const user = await api.usersApi.add(team.id, { 
        name: userName, 
        score: 0, 
        isActive: true 
      });
      onUserCreated(user);
      navigate(`/teams/${team.id}`);
    } catch (error) {
      console.error('Error joining team:', error);
      showToast('Failed to join team. Check the code.', 'error');
    }
    setLoading(false);
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Fixed button at the top left */}
      <button 
        onClick={() => navigate('/menu')} 
        className="fixed top-8 left-8 text-slate-400 hover:text-white flex items-center gap-2 z-20"
      >
        <ArrowLeft size={20} />
        Back
      </button>
      
      <div className="text-center relative z-10 w-full max-w-sm">
        <h1 className="text-4xl font-bold text-white mb-8 text-center">
          Connect with Code{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        
        <input
          type="text"
          placeholder="Enter team code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          className="w-full px-5 py-4 bg-slate-800 text-white rounded-lg mb-6 focus:outline-none focus:ring-2 focus:ring-yellow-500"
          onKeyPress={(e) => e.key === 'Enter' && handleJoin()}
        />
        
        <button
          onClick={handleJoin}
          disabled={loading || !code}
          className="w-full px-5 py-4 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 font-semibold flex justify-center"
        >
          {loading ? 'Connecting...' : 'Connect'}
        </button>
      </div>
      
      {/* Add back the CSS needed for the pulsing star */}
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
