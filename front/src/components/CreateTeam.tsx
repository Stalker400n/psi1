import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import api from '../services/api.service';
import type { User } from '../services/api.service';
import { renderPulsingStar, floatingQuotesCSS } from '../utils/praises';
import { useToast } from '../contexts/ToastContext';

interface CreateTeamProps {
  userName: string;
  userId: number;
  onUserCreated: (user: User) => void;
}

export function CreateTeam({ userName, userId, onUserCreated }: CreateTeamProps) {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [teamName, setTeamName] = useState<string>('');
  const [isPrivate, setIsPrivate] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  const handleCreate = async () => {
    if (!teamName) return;
    setLoading(true);
    
    // Log the global user ID for debugging/tracking
    console.log(`Creating team with global user ID: ${userId}`);
    
    try {
      const team = await api.teamsApi.create({ 
        name: teamName, 
        isPrivate
      });
      
      // Check if user somehow already in team (shouldn't happen, but be safe)
      const existingUser = team.users?.find(u => u.name === userName);
      
      if (existingUser) {
        onUserCreated(existingUser);
      } else {
        // Create new user as Owner (using the name from global user with ID: userId)
        const user = await api.usersApi.add(team.id, { 
          name: userName, 
          score: 0, 
          isActive: true
        });
        onUserCreated(user);
      }
      
      navigate(`/teams/${team.id}`);
    } catch (error) {
      console.error('Error creating team:', error);
      showToast('Failed to create team', 'error');
    }
    setLoading(false);
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button 
        onClick={() => navigate('/menu')} 
        className="text-slate-400 hover:text-white mb-8 flex items-center gap-2"
      >
        <ArrowLeft size={20} />
        Back
      </button>
      
      <div className="max-w-md mx-auto">
        <h1 className="text-4xl font-bold text-white mb-8 text-center">
          Create Team{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        
        <input
          type="text"
          placeholder="Team name"
          value={teamName}
          onChange={(e) => setTeamName(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && !loading && teamName && handleCreate()}
          className="w-full px-4 py-3 bg-slate-800 text-white rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-yellow-500"
        />
        
        <label className="flex items-center gap-3 text-white mb-6 cursor-pointer">
          <input
            type="checkbox"
            checked={isPrivate}
            onChange={(e) => setIsPrivate(e.target.checked)}
            className="w-5 h-5 accent-yellow-500"
          />
          Private Team
        </label>
        
        <button
          onClick={handleCreate}
          disabled={loading || !teamName}
          className="w-full px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 font-semibold"
        >
          {loading ? 'Creating...' : 'Create Team'}
        </button>
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
