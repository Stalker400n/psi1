import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Music, MessageSquare, LogOut } from 'lucide-react';
import api from '../services/api.service';
import type { Team, User } from '../services/api.service';
import { PlaylistView } from './PlaylistView';
import { ChatView } from './ChatView';

interface TeamViewProps {
  user: User;
  onLeave: () => void;
}

export function TeamView({ user, onLeave }: TeamViewProps) {
  const { teamId } = useParams<{ teamId: string }>();
  const navigate = useNavigate();
  const [team, setTeam] = useState<Team | null>(null);
  const [view, setView] = useState<'playlist' | 'chat'>('playlist');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (teamId) {
      fetchTeam();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teamId]);

  const fetchTeam = async () => {
    if (!teamId) return;
    
    try {
      const data = await api.teamsApi.getById(parseInt(teamId));
      setTeam(data);
    } catch (error) {
      console.error('Error fetching team:', error);
      alert('Team not found');
      navigate('/');
    } finally {
      setLoading(false);
    }
  };

  const handleLeave = () => {
    onLeave();
    navigate('/');
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex items-center justify-center">
        <p className="text-slate-400">Loading team...</p>
      </div>
    );
  }

  if (!team || !teamId) {
    return null;
  }
  
  return (
    <div className="min-h-screen bg-slate-950">
      <div className="bg-slate-900 border-b border-slate-800 p-4">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-white">
              {team.name}<span className="text-yellow-400">.</span>
            </h1>
            <p className="text-slate-400 text-sm">Code: {team.id} • {user.name}</p>
          </div>
          <button
            onClick={handleLeave}
            className="px-4 py-2 bg-slate-800 text-white rounded-lg hover:bg-slate-700 transition flex items-center gap-2"
          >
            <LogOut size={18} />
            Leave
          </button>
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-4">
        <div className="flex gap-2 mb-6">
          <button
            onClick={() => setView('playlist')}
            className={`px-6 py-2 rounded-lg transition font-semibold ${
              view === 'playlist' 
                ? 'bg-yellow-500 text-black' 
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            <Music size={18} className="inline mr-2" />
            Playlist
          </button>
          <button
            onClick={() => setView('chat')}
            className={`px-6 py-2 rounded-lg transition font-semibold ${
              view === 'chat' 
                ? 'bg-yellow-500 text-black' 
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            <MessageSquare size={18} className="inline mr-2" />
            Chat
          </button>
        </div>

        {view === 'playlist' && <PlaylistView teamId={parseInt(teamId)} userId={user.id} userName={user.name} />}
        {view === 'chat' && <ChatView teamId={parseInt(teamId)} userName={user.name} />}
      </div>
    </div>
  );
}