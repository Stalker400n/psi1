import { useState, useEffect } from 'react';
import { Plus, SkipForward, SkipBack, Zap } from 'lucide-react';
import api from '../services/api.service';
import type { Song, User } from '../services/api.service';
import { extractYoutubeId } from '../utils/youtube';
import { HeatMeter } from '../components/HeatMeter';
import { RightPanel } from '../components/RightPanel';
import { useToast } from '../contexts/ToastContext';

interface PlaylistViewProps {
  teamId: number;
  userId: number;
  userName: string;
}

export function PlaylistView({ teamId, userId, userName }: PlaylistViewProps) {
  const { showToast } = useToast();
  const [queue, setQueue] = useState<Song[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [currentSong, setCurrentSong] = useState<Song | null>(null);
  const [currentRating, setCurrentRating] = useState<number>(0);
  const [showAdd, setShowAdd] = useState<boolean>(false);
  const [videoUrl, setVideoUrl] = useState<string>('');
  const [addMode, setAddMode] = useState<'end' | 'next'>('end');

  useEffect(() => {
    fetchQueueAndCurrent();
    fetchUsers();
    const interval = setInterval(() => {
      fetchQueueAndCurrent();
      fetchUsers();
    }, 3000);
    return () => clearInterval(interval);
  }, [teamId]);

  useEffect(() => {
    if (currentSong) {
      fetchCurrentRating();
    }
  }, [currentSong?.id, userId]);

  const fetchCurrentRating = async () => {
    if (!currentSong) return;
    
    try {
      const userRating = await api.ratingsApi.getUserRating(teamId, currentSong.id, userId);
      setCurrentRating(userRating?.rating || 0);
    } catch (error) {
      console.error('Error fetching current rating:', error);
      setCurrentRating(0);
    }
  };

  const fetchUsers = async () => {
    try {
      const userData = await api.usersApi.getAll(teamId);
      setUsers(userData);
    } catch (error) {
      console.error('Error fetching users:', error);
      setUsers([]);
    }
  };

  const fetchQueueAndCurrent = async () => {
    try {
      const queueData = await api.songsApi.getQueue(teamId);
      setQueue(queueData);
      
      if (queueData.length > 0) {
        setCurrentSong(queueData[0]);
      } else {
        setCurrentSong(null);
      }
    } catch (error) {
      console.error('Error fetching queue:', error);
      setQueue([]);
      setCurrentSong(null);
    }
  };

  const handleRatingSubmit = async (rating: number) => {
    if (!currentSong) return;

    try {
      await api.ratingsApi.submitRating(teamId, currentSong.id, userId, rating);
      setCurrentRating(rating);
      // Remove success alert - rating update is visible in the heat meter
    } catch (error) {
      console.error('Error submitting rating:', error);
      showToast('Failed to submit rating', 'error');
    }
  };

  const addSong = async () => {
    if (!videoUrl.trim()) return;

    try {
      await api.songsApi.add(
        teamId, 
        { 
          link: videoUrl,
          title: 'Song Name',
          artist: 'Artist',
          rating: 0,
          addedByUserId: userId,
          addedByUserName: userName
        },
        addMode === 'next'
      );
      setVideoUrl('');
      setShowAdd(false);
      fetchQueueAndCurrent();
      showToast('Song added to queue', 'success');
    } catch (error) {
      console.error('Error adding song:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to add song';
      showToast(errorMessage, 'error');
    }
  };

  const deleteSong = async (songId: number) => {
    try {
      await api.songsApi.delete(teamId, songId);
      fetchQueueAndCurrent();
      showToast('Song removed from queue', 'success');
    } catch (error) {
      console.error('Error deleting song:', error);
      showToast('Failed to remove song', 'error');
    }
  };

  const nextSong = async () => {
    try {
      await api.songsApi.next(teamId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error skipping to next song:', error);
      showToast('No more songs in queue', 'info');
    }
  };

  const previousSong = async () => {
    try {
      await api.songsApi.previous(teamId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error going to previous song:', error);
      showToast('Already at first song', 'info');
    }
  };

  const jumpToSong = async (index: number) => {
    try {
      await api.songsApi.jumpTo(teamId, index);
      fetchQueueAndCurrent();
      showToast('Jumped to song', 'success');
    } catch (error) {
      console.error('Error jumping to song:', error);
      showToast('Failed to jump to song', 'error');
    }
  };

  return (
    <div className="flex gap-4 h-[calc(100vh-200px)]">
      {/* Left: Slim Heat Meter */}
      <div className="w-20 flex-shrink-0">
        {currentSong && (
          <HeatMeter 
            currentRating={currentRating}
            onSubmit={handleRatingSubmit}
          />
        )}
      </div>

      {/* Center: Player - Large */}
      <div className="flex-1 flex flex-col gap-4">
        {/* Now Playing */}
        <div className="bg-slate-900 rounded-lg p-6 flex-1">
          <h2 className="text-xl font-semibold text-white mb-4">Now Playing</h2>
          {currentSong ? (
            <div className="h-full flex flex-col">
              {/* Video player */}
              <div className="bg-black aspect-video rounded mb-4 flex items-center justify-center flex-shrink-0">
                <iframe
                  width="100%"
                  height="100%"
                  src={`https://www.youtube.com/embed/${extractYoutubeId(currentSong.link)}`}
                  frameBorder="0"
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                  allowFullScreen
                  className="rounded"
                />
              </div>
              
              {/* Song info */}
              <div className="mb-4">
                <h3 className="text-white font-semibold text-lg">{currentSong.title}</h3>
                <p className="text-slate-400">{currentSong.artist}</p>
                <p className="text-slate-500 text-sm">Added by {currentSong.addedByUserName}</p>
                <p className="text-yellow-400 text-xs mt-1">Position: #{currentSong.index + 1}</p>
              </div>
              
              {/* Controls */}
              <div className="flex gap-2 mt-auto">
                <button
                  onClick={previousSong}
                  className="flex-1 px-4 py-3 bg-slate-700 text-white rounded-lg hover:bg-slate-600 transition flex items-center justify-center gap-2 font-semibold"
                >
                  <SkipBack size={18} />
                  Previous
                </button>
                <button
                  onClick={nextSong}
                  className="flex-1 px-4 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-2 font-semibold"
                >
                  <SkipForward size={18} />
                  Next
                </button>
              </div>
            </div>
          ) : (
            <div className="h-full flex items-center justify-center">
              <p className="text-slate-400">No songs in queue</p>
            </div>
          )}
        </div>

        {/* Add Song Button & Form */}
        <button
          onClick={() => setShowAdd(!showAdd)}
          className="w-full px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-2 font-semibold"
        >
          <Plus size={20} />
          Add Song to Queue
        </button>

        {showAdd && (
          <div className="bg-slate-900 rounded-lg p-4">
            <div className="mb-3">
              <label className="text-white text-sm mb-2 block">Add song to:</label>
              <div className="flex gap-2">
                <button
                  onClick={() => setAddMode('end')}
                  className={`flex-1 px-4 py-2 rounded-lg transition font-semibold ${
                    addMode === 'end' 
                      ? 'bg-yellow-500 text-black' 
                      : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                  }`}
                >
                  End of Queue
                </button>
                <button
                  onClick={() => setAddMode('next')}
                  className={`flex-1 px-4 py-2 rounded-lg transition font-semibold flex items-center justify-center gap-2 ${
                    addMode === 'next' 
                      ? 'bg-yellow-500 text-black' 
                      : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                  }`}
                >
                  <Zap size={16} />
                  Play Next
                </button>
              </div>
            </div>
            
            <input
              type="text"
              placeholder="YouTube Video URL"
              value={videoUrl}
              onChange={(e) => setVideoUrl(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && addSong()}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-yellow-500"
            />
            <button
              onClick={addSong}
              disabled={!videoUrl.trim()}
              className="w-full px-6 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-semibold disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {addMode === 'next' ? 'Add to Play Next' : 'Add to End'}
            </button>
          </div>
        )}
      </div>

      {/* Right: Tabbed Panel */}
      <div className="w-96 flex-shrink-0">
        <RightPanel
          teamId={teamId}
          userId={userId}
          userName={userName}
          queue={queue}
          users={users}
          userRole={users.find(u => u.id === userId)?.role || 'Member'}
          onJumpToSong={jumpToSong}
          onDeleteSong={deleteSong}
        />
      </div>
    </div>
  );
}
