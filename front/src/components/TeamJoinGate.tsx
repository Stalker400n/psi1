import { useState, useEffect } from 'react';
import { Users, ArrowLeft } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api.service';
import type { Team, GlobalUser } from '../services/api.service';
import { fingerprintService } from '../services/fingerprint.service';
import { useToast } from '../contexts/ToastContext';
import { 
  getRandomPraises, 
  generatePraiseStyles, 
  floatingQuotesCSS,
  renderFloatingQuote,
  renderPulsingStar
} from '../utils/praises';

const praises = getRandomPraises();
const praiseStyles = generatePraiseStyles(praises);

interface TeamJoinGateProps {
  teamId: number;
  onLogin: (user: GlobalUser) => void;
}

export function TeamJoinGate({ teamId, onLogin }: TeamJoinGateProps) {
  const { showToast } = useToast();
  const navigate = useNavigate();
  const [team, setTeam] = useState<Team | null>(null);
  const [name, setName] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);
  const [teamLoading, setTeamLoading] = useState(true);

  useEffect(() => {
    fetchTeamInfo();
  }, [teamId]);

  const fetchTeamInfo = async () => {
    setTeamLoading(true);
    try {
      const teamData = await api.teamsApi.getById(teamId);
      setTeam(teamData);
    } catch (error) {
      console.error('Failed to fetch team:', error);
      showToast('Team not found or no longer exists', 'error');
    } finally {
      setTeamLoading(false);
    }
  };

  const handleSubmit = async () => {
    if (!name.trim()) return;
    
    setIsLoading(true);
    
    try {
      // Get device fingerprint
      const fingerprint = await fingerprintService.getFingerprint();
      const deviceInfo = fingerprintService.getDeviceInfo();
      
      // Authenticate user
      const globalUser = await api.globalUsersApi.registerOrLogin({
        name: name.trim(),
        deviceFingerprint: fingerprint,
        deviceInfo
      });
      
      // Pass to parent - parent will handle team join
      onLogin(globalUser);
      
    } catch (err) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Failed to connect';
      showToast(errorMessage, 'error');
      console.error('Failed to login:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Back button */}
      <button 
        onClick={() => navigate('/menu')}
        className="absolute top-8 right-8 text-slate-400 hover:text-white flex items-center gap-2 transition"
      >
        <ArrowLeft size={20} />
        Back
      </button>

      {/* Praises background */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>
      
      {/* Main content */}
      <div className="text-center relative z-10 w-full max-w-sm">
        <h1 className="text-6xl font-bold text-white mb-2">
          komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-8">Connect through music!</p>
        
        {/* Team preview card */}
        {teamLoading ? (
          <div className="mb-6 p-6 bg-slate-900 rounded-lg border border-slate-800">
            <div className="animate-pulse">
              <div className="h-6 bg-slate-700 rounded w-3/4 mx-auto mb-2"></div>
              <div className="h-4 bg-slate-700 rounded w-1/2 mx-auto"></div>
            </div>
          </div>
        ) : team ? (
          <div className="mb-6 p-6 bg-slate-900 rounded-lg border border-yellow-500/50">
            <div className="flex items-center justify-center gap-2 mb-2">
              <Users className="text-yellow-500" size={24} />
              <h2 className="text-xl font-bold text-white">{team.name}</h2>
            </div>
            <p className="text-slate-400 text-sm">
              {team.users?.length || 0} {team.users?.length === 1 ? 'member' : 'members'}
            </p>
            {team.isPrivate && (
              <span className="inline-block mt-2 px-3 py-1 bg-slate-800 text-slate-300 text-xs rounded-full">
                Private Team
              </span>
            )}
          </div>
        ) : (
          <div className="mb-6 p-6 bg-red-900/30 border border-red-600 rounded-lg">
            <p className="text-red-300">Team not found</p>
          </div>
        )}
        
        {/* Name input - only show if team exists */}
        {team && (
          <div className="space-y-4">
            <input
              type="text"
              placeholder="Enter your name to join"
              value={name}
              onChange={(e) => setName(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && !isLoading && name && handleSubmit()}
              className="px-6 py-3 bg-slate-800 text-white rounded-lg w-full focus:outline-none focus:ring-2 focus:ring-yellow-500"
              disabled={isLoading}
              maxLength={50}
            />
            
            <button
              onClick={handleSubmit}
              disabled={!name.trim() || isLoading}
              className="w-full px-5 py-4 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
            >
              {isLoading ? 'Connecting...' : 'Connect'}
            </button>
          </div>
        )}
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
