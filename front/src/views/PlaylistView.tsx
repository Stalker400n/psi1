import { useState, useEffect } from 'react';
import { Plus, Trash2, SkipForward, SkipBack, Zap } from 'lucide-react';
import api from '../services/api.service';
import type { Song } from '../services/api.service';
import { extractYoutubeId } from '../utils/youtube';

interface PlaylistViewProps {
  teamId: number;
  userId: number;
  userName: string;
}

export function PlaylistView({ teamId, userId, userName }: PlaylistViewProps) {
  const [queue, setQueue] = useState<Song[]>([]);
  const [currentSong, setCurrentSong] = useState<Song | null>(null);
  const [showAdd, setShowAdd] = useState<boolean>(false);
  const [videoUrl, setVideoUrl] = useState<string>('');
  const [addMode, setAddMode] = useState<'end' | 'next'>('end');

  useEffect(() => {
    fetchQueueAndCurrent();
    const interval = setInterval(fetchQueueAndCurrent, 3000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teamId]);

  const fetchQueueAndCurrent = async () => {
    try {
      // Fetch queue (songs from current index onwards)
      const queueData = await api.songsApi.getQueue(teamId);
      setQueue(queueData);
      
      // Current song is the first in queue
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

  const addSong = async () => {
    if (!videoUrl.trim()) return;

    console.log('Adding song with:', { 
      teamId, 
      userId, 
      userName, 
      videoUrl,
      insertAfterCurrent: addMode === 'next'
    });

    try {
      const result = await api.songsApi.add(
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
      console.log('Song added successfully:', result);
      setVideoUrl('');
      setShowAdd(false);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error adding song:', error);
      alert('Failed to add song. Check console for details.');
    }
  };

  const deleteSong = async (songId: number) => {
    try {
      await api.songsApi.delete(teamId, songId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error deleting song:', error);
    }
  };

  const nextSong = async () => {
    try {
      await api.songsApi.next(teamId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error skipping to next song:', error);
      alert('No more songs in queue');
    }
  };

  const previousSong = async () => {
    try {
      await api.songsApi.previous(teamId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error going to previous song:', error);
      alert('Already at first song');
    }
  };

  const jumpToSong = async (index: number) => {
    try {
      await api.songsApi.jumpTo(teamId, index);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error('Error jumping to song:', error);
    }
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div>
        <div className="bg-slate-900 rounded-lg p-6 mb-6">
          <h2 className="text-xl font-semibold text-white mb-4">Now Playing</h2>
          {currentSong ? (
            <div>
              <div className="bg-black aspect-video rounded mb-4 flex items-center justify-center">
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
              <div className="flex justify-between items-start mb-2">
                <div>
                  <h3 className="text-white font-semibold">{currentSong.title}</h3>
                  <p className="text-slate-400">{currentSong.artist}</p>
                  <p className="text-slate-500 text-sm">Added by {currentSong.addedByUserName}</p>
                  <p className="text-yellow-400 text-xs mt-1">Position: #{currentSong.index + 1}</p>
                </div>
              </div>
              <div className="flex gap-2 mt-4">
                <button
                  onClick={previousSong}
                  className="flex-1 px-4 py-2 bg-slate-700 text-white rounded-lg hover:bg-slate-600 transition flex items-center justify-center gap-2"
                >
                  <SkipBack size={18} />
                  Previous
                </button>
                <button
                  onClick={nextSong}
                  className="flex-1 px-4 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-2"
                >
                  <SkipForward size={18} />
                  Next
                </button>
              </div>
            </div>
          ) : (
            <p className="text-slate-400">No songs in queue</p>
          )}
        </div>

        <button
          onClick={() => setShowAdd(!showAdd)}
          className="w-full px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-2 font-semibold"
        >
          <Plus size={20} />
          Add Song to Queue
        </button>

        {showAdd && (
          <div className="mt-4 bg-slate-900 rounded-lg p-6">
            <div className="mb-4">
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

      <div className="bg-slate-900 rounded-lg p-6">
        <h2 className="text-xl font-semibold text-white mb-4">
          Queue ({Math.max(0, queue.length - 1)} {queue.length - 1 === 1 ? 'song' : 'songs'})
        </h2>
        <div className="space-y-3 max-h-[600px] overflow-y-auto">
          {queue.slice(1).map((song) => (
            <div key={song.id} className="bg-slate-800 p-4 rounded-lg hover:bg-slate-750 transition">
              <div className="flex justify-between items-start">
                <div 
                  className="flex-1 cursor-pointer"
                  onClick={() => jumpToSong(song.index)}
                >
                  <p className="text-yellow-400 text-sm font-semibold">#{song.index + 1}</p>
                  <h4 className="text-white font-semibold hover:text-yellow-400 transition">{song.title}</h4>
                  <p className="text-slate-400 text-sm">{song.artist}</p>
                  <p className="text-slate-500 text-xs mt-1">Added by {song.addedByUserName}</p>
                </div>
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    deleteSong(song.id);
                  }}
                  className="ml-2 p-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            </div>
          ))}
          {queue.length <= 1 && <p className="text-slate-400">No songs in queue</p>}
        </div>
      </div>
    </div>
  );
}