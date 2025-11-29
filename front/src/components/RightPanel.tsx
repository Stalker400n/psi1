import { useState } from 'react';
import { Music, Users, MessageSquare, Trophy, Trash2 } from 'lucide-react';
import api from '../services/api.service';
import type { Song, User } from '../services/api.service';
import { ChatView } from '../views/ChatView';
import { Leaderboard } from './Leaderboard';
import { useToast } from '../contexts/ToastContext';

type PanelView = 'queue' | 'users' | 'chat' | 'leaderboard';

interface RightPanelProps {
  teamId: number;
  userId: number;
  userName: string;
  queue: Song[];
  users: User[];
  userRole: 'Member' | 'Moderator' | 'Owner';
  onJumpToSong: (index: number) => void;
  onDeleteSong: (songId: number) => void;
  onRefreshTeam?: () => void;
}

export function RightPanel({
  teamId,
  userId,
  userName,
  queue,
  users,
  userRole,
  onJumpToSong,
  onDeleteSong,
}: RightPanelProps) {
  const [view, setView] = useState<PanelView>('queue');

  return (
    <div className="bg-slate-900 rounded-lg p-4 h-full flex flex-col">
      {/* Tab buttons */}
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

      {/* Content area */}
      <div className="flex-1 overflow-hidden">
        {view === 'queue' && (
          <QueuePanel 
            queue={queue} 
            onJumpToSong={onJumpToSong}
            onDeleteSong={onDeleteSong}
          />
        )}
        
        {view === 'users' && (
          <UsersPanel 
            users={users} 
            teamId={teamId} 
            userRole={userRole} 
          />
        )}
        
        {view === 'chat' && (
          <ChatPanel teamId={teamId} userName={userName} />
        )}
        
        {view === 'leaderboard' && (
          <LeaderboardPanel teamId={teamId} userId={userId} />
        )}
      </div>
    </div>
  );
}

// Sub-components
function QueuePanel({ queue, onJumpToSong, onDeleteSong }: {
  queue: Song[];
  onJumpToSong: (index: number) => void;
  onDeleteSong: (songId: number) => void;
}) {
  return (
    <div className="h-full flex flex-col">
      <h3 className="text-white font-semibold mb-3 text-sm">
        Queue ({Math.max(0, queue.length - 1)} {queue.length - 1 === 1 ? 'song' : 'songs'})
      </h3>
      <div className="flex-1 overflow-y-auto space-y-2">
        {queue.slice(1).map((song) => (
          <div key={song.id} className="bg-slate-800 p-3 rounded-lg hover:bg-slate-750 transition">
            <div className="flex justify-between items-start">
              <div 
                className="flex-1 cursor-pointer"
                onClick={() => onJumpToSong(song.index)}
              >
                <p className="text-yellow-400 text-xs font-semibold">#{song.index + 1}</p>
                <h4 className="text-white font-semibold text-sm hover:text-yellow-400 transition">{song.title}</h4>
                <p className="text-slate-400 text-xs">{song.artist}</p>
                <p className="text-slate-500 text-xs mt-1">Added by {song.addedByUserName}</p>
              </div>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onDeleteSong(song.id);
                }}
                className="ml-2 p-1 bg-red-600 text-white rounded hover:bg-red-700 transition"
              >
                <Trash2 size={14} />
              </button>
            </div>
          </div>
        ))}
        {queue.length <= 1 && (
          <p className="text-slate-400 text-sm text-center py-8">No songs in queue</p>
        )}
      </div>
    </div>
  );
}

function UsersPanel({ users, teamId, userRole }: { 
  users: User[]; 
  teamId: number; 
  userRole: 'Member' | 'Moderator' | 'Owner' 
}) {
  const { showToast } = useToast();
  const [error, setError] = useState<string | null>(null);
  const [isChangingRole, setIsChangingRole] = useState(false);
  
  const handleRoleChange = async (userId: number, newRole: 'Member' | 'Moderator' | 'Owner') => {
    if (isChangingRole) return;
    
    try {
      setIsChangingRole(true);
      setError(null);
      
      await api.usersApi.changeRole(
        teamId,
        userId,
        newRole,
        userId // Current user's ID as the requesting user
      );
      
      showToast(`Role updated to ${newRole}`, 'success');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to change role';
      console.error('Failed to change role', err);
      setError(errorMessage);
      showToast(`Error: ${errorMessage}`, 'error');
    } finally {
      setIsChangingRole(false);
    }
  };

  return (
    <div className="h-full flex flex-col">
      <h3 className="text-white font-semibold mb-3 text-sm">Team Members ({users.length})</h3>
      
      {error && (
        <div className="mb-3 p-2 bg-red-900/30 border border-red-600 rounded-lg text-red-300 text-xs">
          {error}
          <button
            onClick={() => setError(null)}
            className="ml-2 underline hover:text-red-200"
          >
            Dismiss
          </button>
        </div>
      )}
      
      <div className="flex-1 overflow-y-auto space-y-2">
        {users.map((user) => (
          <div key={user.id} className="bg-slate-800 p-3 rounded-lg">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-white font-medium text-sm">{user.name}</p>
                <p className="text-slate-400 text-xs">
                  Joined: {new Date(user.joinedAt).toLocaleString()}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <span className={`text-xs px-2 py-1 rounded ${
                  user.role === 'Owner' ? 'bg-yellow-500 text-black' :
                  user.role === 'Moderator' ? 'bg-indigo-600 text-white' :
                  'bg-slate-700 text-slate-200'
                }`}>
                  {user.role}
                </span>
                
                {(userRole === 'Owner' || userRole === 'Moderator') && (
                  <select
                    value={user.role}
                    onChange={(e) => {
                      const newRole = e.target.value as 'Member' | 'Moderator' | 'Owner';
                      handleRoleChange(user.id, newRole);
                    }}
                    disabled={isChangingRole}
                    className="bg-slate-700 text-white rounded px-2 py-1 text-xs"
                  >
                    <option value="Member">Member</option>
                    <option value="Moderator">Moderator</option>
                    <option value="Owner">Owner</option>
                  </select>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function ChatPanel({ teamId, userName }: { teamId: number; userName: string }) {
  return <ChatView teamId={teamId} userName={userName} />;
}

function LeaderboardPanel({ teamId, userId }: { teamId: number; userId: number }) {
  return <Leaderboard teamId={teamId} userId={userId} />;
}
