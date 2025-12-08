import { useState } from 'react';
import { Music, Users, MessageSquare, Trophy, Plus, Zap } from 'lucide-react';
import type { Song, User } from '../services/api.service';
import { useToast } from '../contexts/toast-context';
import { QueuePanel } from './queue-panel.component';
import { UsersPanel } from './users-panel.component';
import { ChatPanel } from './chat-panel.component';
import { LeaderboardPanel } from './leaderboard-panel.component';

type PanelView = 'queue' | 'users' | 'chat' | 'leaderboard';

interface RightPanelProps {
  teamId: number;
  userId: number;
  userName: string;
  queue: Song[];
  currentIndex: number;
  users: User[];
  userRole: 'Member' | 'Moderator' | 'Owner';
  onDeleteSong: (songId: number) => void;
  onAddSong?: (url: string, addToBeginning: boolean) => Promise<void>;
  onRefreshTeam?: () => void;
}

const hideScrollbarStyle = `
  ::-webkit-scrollbar {
    display: none;
    width: 0;
    height: 0;
  }
`;

export function RightPanel({
  teamId,
  userId,
  userName,
  queue,
  currentIndex,
  users,
  userRole,
  onDeleteSong,
  onAddSong,
}: RightPanelProps) {
  const { showToast } = useToast();
  const [view, setView] = useState<PanelView>('queue');
  const [videoUrl, setVideoUrl] = useState<string>('');
  const [addToBeginning, setAddToBeginning] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  
  const canAddToBeginning = userRole === 'Owner' || userRole === 'Moderator';
  
  const handleAddSong = async () => {
    if (!videoUrl.trim() || !onAddSong || isSubmitting) return;
    
    try {
      setIsSubmitting(true);
      
      if (addToBeginning && !canAddToBeginning) {
        showToast('Only Moderators and Owners can add songs to beginning of queue', 'warning');
        setAddToBeginning(false);
        return;
      }
      
      await onAddSong(videoUrl, addToBeginning);
      setVideoUrl('');
      showToast('Song added to queue', 'success');
    } catch (error) {
      console.error('Error adding song:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to add song';
      showToast(errorMessage, 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="bg-slate-900 rounded-lg p-4 h-full flex flex-col">
      <style>{hideScrollbarStyle}</style>
      <div className="grid grid-cols-4 gap-2 mb-4">
        <button
          onClick={() => setView('queue')}
          className={`px-3 py-2 rounded-lg transition font-semibold text-sm flex flex-col items-center gap-1 ${
            view === 'queue'
              ? 'bg-yellow-500 text-black'
              : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
          }`}
        >
          <Music size={18} />
          <span className="text-xs">Queue</span>
        </button>
        
        <button
          onClick={() => setView('users')}
          className={`px-3 py-2 rounded-lg transition font-semibold text-sm flex flex-col items-center gap-1 ${
            view === 'users'
              ? 'bg-yellow-500 text-black'
              : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
          }`}
        >
          <Users size={18} />
          <span className="text-xs">Users</span>
        </button>
        
        <button
          onClick={() => setView('chat')}
          className={`px-3 py-2 rounded-lg transition font-semibold text-sm flex flex-col items-center gap-1 ${
            view === 'chat'
              ? 'bg-yellow-500 text-black'
              : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
          }`}
        >
          <MessageSquare size={18} />
          <span className="text-xs">Chat</span>
        </button>
        
        <button
          onClick={() => setView('leaderboard')}
          className={`px-3 py-2 rounded-lg transition font-semibold text-sm flex flex-col items-center gap-1 ${
            view === 'leaderboard'
              ? 'bg-yellow-500 text-black'
              : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
          }`}
        >
          <Trophy size={18} />
          <span className="text-xs">Board</span>
        </button>
      </div>

      <div className="flex-1 overflow-hidden">
        {view === 'queue' && (
          <QueuePanel 
            queue={queue}
            currentIndex={currentIndex}
            onDeleteSong={onDeleteSong}
          />
        )}
        
        {view === 'users' && (
          <UsersPanel 
            users={users} 
            teamId={teamId} 
            userRole={userRole}
            userId={userId}
          />
        )}
        
        {view === 'chat' && (
          <ChatPanel teamId={teamId} userName={userName} />
        )}
        
        {view === 'leaderboard' && (
          <LeaderboardPanel teamId={teamId} userId={userId} />
        )}
      </div>

      <div className="pt-4 mt-3 border-t border-slate-700">
        <div className="flex items-center justify-between mb-2">
          <h3 className="text-white font-semibold text-sm">Add Song to Queue</h3>
          <div className="flex items-center gap-2">
            <label className="flex items-center justify-between cursor-pointer gap-3">
              <span className={`text-xs flex items-center gap-1 ${
                !canAddToBeginning ? 'text-slate-500' : 'text-white'
              }`}>
                <Zap size={12} /> Play Next
              </span>
              <div className="relative">
                <input
                  type="checkbox"
                  checked={addToBeginning}
                  onChange={() => {
                    if (!canAddToBeginning) {
                      showToast('Only Moderators and Owners can add songs to beginning of queue', 'warning');
                      return;
                    }
                    setAddToBeginning(!addToBeginning);
                  }}
                  className="sr-only"
                />
                <div className={`block w-10 h-6 rounded-full transition-colors ${
                  addToBeginning ? 'bg-yellow-500' : 'bg-slate-700'
                } ${!canAddToBeginning ? 'opacity-50 cursor-not-allowed' : ''}`} />
                
                <div className={`absolute left-1 top-1 bg-white w-4 h-4 rounded-full transition-transform ${
                  addToBeginning ? 'transform translate-x-4' : ''
                }`} />
              </div>
            </label>
          </div>
        </div>
        
        <div className="flex gap-2">
          <input
            type="text"
            placeholder="YouTube URL"
            value={videoUrl}
            onChange={(e) => setVideoUrl(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && handleAddSong()}
            className="flex-1 px-3 py-2 bg-slate-800 text-white text-sm rounded-lg focus:outline-none focus:ring-1 focus:ring-yellow-500"
          />
          <button
            onClick={handleAddSong}
            disabled={!videoUrl.trim() || isSubmitting}
            className="px-3 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-medium disabled:opacity-50 disabled:cursor-not-allowed text-sm flex items-center gap-1.5"
          >
            <Plus size={16} />
            Add
          </button>
        </div>
      </div>
    </div>
  );
}