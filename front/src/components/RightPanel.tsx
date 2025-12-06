import { useState } from 'react';
import { Music, Users, MessageSquare, Trophy, Trash2, Plus, Zap } from 'lucide-react';
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
  currentIndex: number;
  users: User[];
  userRole: 'Member' | 'Moderator' | 'Owner';
  onDeleteSong: (songId: number) => void;
  onAddSong?: (url: string, addToBeginning: boolean) => Promise<void>;
  onRefreshTeam?: () => void;
}

// Add global style for webkit scrollbars
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
      {/* Add style tag for webkit scrollbar hiding */}
      <style>{hideScrollbarStyle}</style>
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

      {/* Add Song - Always visible at bottom */}
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
                {/* Toggle background */}
                <div className={`block w-10 h-6 rounded-full transition-colors ${
                  addToBeginning ? 'bg-yellow-500' : 'bg-slate-700'
                } ${!canAddToBeginning ? 'opacity-50 cursor-not-allowed' : ''}`} />
                
                {/* Toggle indicator */}
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

// Sub-components
function QueuePanel({ queue, currentIndex, onDeleteSong }: {
  queue: Song[];
  currentIndex: number;
  onDeleteSong: (songId: number) => void;
}) {
  const [showHistory, setShowHistory] = useState<boolean>(false);
  
  // Split queue into played, current, and upcoming
  const playedSongs = queue.filter(song => song.index < currentIndex);
  const currentSong = queue.find(song => song.index === currentIndex);
  const upcomingSongs = queue.filter(song => song.index > currentIndex);
  
  // Render a single song item
  const renderSongItem = (song: Song, status: 'played' | 'current' | 'upcoming') => {
    return (
      <div 
        key={song.id} 
        className={`p-3 rounded-lg transition ${
          status === 'current' 
            ? 'bg-yellow-500/20 border border-yellow-500' 
            : status === 'played'
            ? 'bg-slate-800/50 opacity-50'
            : 'bg-slate-800'
        }`}
      >
        <div className="flex flex-col">
          <div className="flex gap-3">
            {/* Thumbnail */}
            {song.thumbnailUrl && (
              <img 
                src={song.thumbnailUrl} 
                alt={song.title}
                className="w-20 h-12 object-cover rounded"
              />
            )}
            
            {/* Song info */}
            <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <p className="text-yellow-400 text-xs font-semibold">
                #{song.index + 1}
              </p>
              {status === 'current' && (
                <span className="text-xs bg-yellow-500 text-black px-2 py-0.5 rounded font-bold">
                  NOW PLAYING
                </span>
              )}
            </div>
            <h4 className="text-white font-semibold text-sm truncate">
              {song.title}
            </h4>
            <p className="text-slate-400 text-xs truncate">{song.artist}</p>
          </div>
          
            {/* Delete button - only for upcoming songs */}
            {status === 'upcoming' && (
              <button
                onClick={() => onDeleteSong(song.id)}
                className="p-1 bg-red-600 text-white rounded hover:bg-red-700 transition h-fit"
              >
                <Trash2 size={14} />
              </button>
            )}
          </div>
          
          {/* Added by - bottom right */}
          <div className="flex justify-end mt-1.5">
            <p className="text-slate-500 text-xs">Added by {song.addedByUserName}</p>
          </div>
        </div>
      </div>
    );
  };
  
  return (
    <div className="h-full flex flex-col">
      <h3 className="text-white font-semibold mb-3 text-sm">
        Queue ({upcomingSongs.length} upcoming)
      </h3>
      
      <div className="flex-1 overflow-y-auto space-y-2" style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}>
        {/* Current song */}
        {currentSong && renderSongItem(currentSong, 'current')}
        
        {/* Divider if we have a current song */}
        {currentSong && upcomingSongs.length > 0 && (
          <div className="border-t border-slate-700 my-2"></div>
        )}
        
        {/* Upcoming songs */}
        {upcomingSongs.length > 0 ? (
          upcomingSongs.map(song => renderSongItem(song, 'upcoming'))
        ) : (
          <p className="text-slate-400 text-sm text-center py-4">
            No upcoming songs
          </p>
        )}
        
        {/* Show history button */}
        {playedSongs.length > 0 && (
          <button
            onClick={() => setShowHistory(!showHistory)}
            className="w-full mt-4 px-3 py-2 bg-slate-700 text-white rounded text-sm hover:bg-slate-600"
          >
            {showHistory ? 'Hide' : 'Show'} History ({playedSongs.length} played)
          </button>
        )}
        
        {/* History */}
        {showHistory && playedSongs.length > 0 && (
          <>
            <div className="border-t border-slate-700 my-2"></div>
            <p className="text-slate-500 text-xs mb-2">Previously played:</p>
            {playedSongs.reverse().map(song => renderSongItem(song, 'played'))}
          </>
        )}
      </div>
    </div>
  );
}

function UsersPanel({ users, teamId, userRole, userId }: { 
  users: User[]; 
  teamId: number; 
  userRole: 'Member' | 'Moderator' | 'Owner';
  userId: number;
}) {
  const { showToast } = useToast();
  const [error, setError] = useState<string | null>(null);
  const [isChangingRole, setIsChangingRole] = useState(false);
  
  // Find current user and sort other users by role (Owner -> Moderator -> Member) and then alphabetically
  const currentUser = users.find(user => user.id === userId);
  const otherUsers = users
    .filter(user => user.id !== userId)
    .sort((a, b) => {
      // Sort by role first: Owner > Moderator > Member
      const roleOrder = { Owner: 0, Moderator: 1, Member: 2 };
      const roleComparison = roleOrder[a.role] - roleOrder[b.role];
      
      // If roles are the same, sort alphabetically
      if (roleComparison === 0) {
        return a.name.localeCompare(b.name);
      }
      
      return roleComparison;
    });
  
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
      
      <div className="flex-1 overflow-y-auto space-y-2" style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}>
        {/* Display current user first */}
        {currentUser && (
          <>
            <div className="bg-yellow-500/20 border border-yellow-500 p-3 rounded-lg">
              <div className="flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-white font-medium text-sm">{currentUser.name}</p>
                    <span className="text-yellow-400 text-xs font-bold">(You)</span>
                  </div>
                  <p className="text-slate-400 text-xs">
                    Joined: {new Date(currentUser.joinedAt).toLocaleString()}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <span className={`text-xs px-2 py-1 rounded ${
                    currentUser.role === 'Owner' ? 'bg-yellow-500 text-black' :
                    currentUser.role === 'Moderator' ? 'bg-indigo-600 text-white' :
                    'bg-slate-700 text-slate-200'
                  }`}>
                    {currentUser.role}
                  </span>
                  
                  {(userRole === 'Owner' || userRole === 'Moderator') && (
                    <select
                      value={currentUser.role}
                      onChange={(e) => {
                        const newRole = e.target.value as 'Member' | 'Moderator' | 'Owner';
                        handleRoleChange(currentUser.id, newRole);
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

            {/* Separator line */}
            <div className="border-t border-slate-700 my-2"></div>
          </>
        )}
        
        {/* Display other users alphabetically */}
        {otherUsers.map((user) => (
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
