import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import api from '../services/api.service';
import type { User } from '../services/api.service';
import { renderPulsingStar, floatingQuotesCSS } from '../utils/praise.util';
import { useToast } from '../contexts/toast-context';

interface CreateTeamPageProps {
  userName: string;
  userId: number;
  onUserCreated: (user: User) => void;
}

export function CreateTeamPage({ userName, userId, onUserCreated }: CreateTeamPageProps) {
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

      const existingUser = team.users?.find(u => u.name === userName);

      if (existingUser) {
        onUserCreated(existingUser);
      } else {
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
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-8 relative overflow-hidden">
      {/* Back button */}
      <button 
        onClick={() => navigate('/menu')} 
        className="absolute top-8 left-8 text-slate-400 hover:text-white flex items-center gap-2 transition"
      >
        <ArrowLeft size={20} />
        Back
      </button>

      <div className="w-full max-w-md flex flex-col items-center text-center z-10">
        {/* Title */}
        <h1 className="text-4xl font-bold text-white mb-6 leading-tight">
          Create Team {renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>

        {/* Settings card (centered) */}
        <div className="w-full max-w-md bg-slate-800 rounded-lg p-4 shadow-lg mb-6 border border-slate-700">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-white text-base font-semibold">Settings</h3>
            <span className="text-slate-400 text-xs">Configure team options</span>
          </div>

          <div className="flex flex-col gap-3">
            {/* Private toggle */}
            <div className="flex items-center justify-between">
              <span className="text-white text-sm">Private Team</span>
              <button
                onClick={() => setIsPrivate(!isPrivate)}
                className={`relative inline-flex h-6 w-12 items-center rounded-full transition-colors focus:outline-none ${
                  isPrivate ? 'bg-yellow-500' : 'bg-slate-700'
                }`}
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

            {/* (You can add other centered settings here) */}
          </div>
        </div>

        {/* Input + Button (centered under settings) */}
        <div className="w-full max-w-md flex flex-col items-center gap-3">
          <input
            type="text"
            placeholder="Team name"
            value={teamName}
            onChange={(e) => setTeamName(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && !loading && teamName && handleCreate()}
            className="w-full px-4 py-3 text-base bg-slate-800 text-white rounded-lg border border-slate-700 focus:outline-none"
          />

          <button
            onClick={handleCreate}
            disabled={loading || !teamName}
            className="w-full px-4 py-3 text-base font-semibold rounded-lg bg-yellow-500 text-black hover:bg-yellow-400 transition disabled:opacity-50"
          >
            {loading ? 'Creating...' : 'Create Team'}
          </button>
        </div>
      </div>

      {/* floating/pulsing CSS */}
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
