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
          Create Team{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        
        <input
          type="text"
          placeholder="Team name"
          value={teamName}
          onChange={(e) => setTeamName(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && !loading && teamName && handleCreate()}
          className="w-full px-5 py-4 bg-slate-800 text-white rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-yellow-500"
        />
        
        <div className="flex items-center justify-between text-white mb-6">
          <span>Private Team</span>
          <button
            type="button"
            className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-yellow-500 focus:ring-offset-2 ${
              isPrivate ? 'bg-yellow-500' : 'bg-slate-700'
            }`}
            onClick={() => setIsPrivate(!isPrivate)}
            role="switch"
            aria-checked={isPrivate}
          >
            <span className="sr-only">Private team toggle</span>
            <span
              className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                isPrivate ? 'translate-x-6' : 'translate-x-1'
              }`}
            />
          </button>
        </div>
        
        <button
          onClick={handleCreate}
          disabled={loading || !teamName}
          className="w-full px-5 py-4 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 font-semibold"
        >
          {loading ? 'Creating...' : 'Create Team'}
        </button>
      </div>
      
      {/* Add back the CSS needed for the pulsing star */}
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
