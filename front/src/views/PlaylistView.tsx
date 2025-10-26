import { useState, useEffect } from 'react';
import { Plus, Trash2, SkipForward } from 'lucide-react';
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
  const [showAdd, setShowAdd] = useState<boolean>(false);
  const [newSong, setNewSong] = useState({ link: '', title: '', artist: '' });

  useEffect(() => {
    fetchQueue();
    const interval = setInterval(fetchQueue, 3000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teamId]);

  const fetchQueue = async () => {
    try {
      const data = await api.songsApi.getQueue(teamId);
      setQueue(data);
    } catch (error) {
      console.error('Error fetching queue:', error);
    }
  };

  const addSong = async () => {
    if (!newSong.link || !newSong.title) return;

    try {
      await api.songsApi.add(teamId, { 
        ...newSong, 
        rating: 0,
        addedByUserId: userId,
        addedByUserName: userName
      });
      setNewSong({ link: '', title: '', artist: '' });
      setShowAdd(false);
      fetchQueue();
    } catch (error) {
      console.error('Error adding song:', error);
    }
  };

  const deleteSong = async (songId: number) => {
    try {
      await api.songsApi.delete(teamId, songId);
      fetchQueue();
    } catch (error) {
      console.error('Error deleting song:', error);
    }
  };

  const skipSong = async () => {
    try {
      await api.songsApi.dequeue(teamId);
      fetchQueue();
    } catch (error) {
      console.error('Error skipping song:', error);
    }
  };

  const currentSong = queue[0];

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
                </div>
                <button
                  onClick={skipSong}
                  className="px-4 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition flex items-center gap-2"
                >
                  <SkipForward size={18} />
                  Skip
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
            <input
              type="text"
              placeholder="YouTube Link"
              value={newSong.link}
              onChange={(e) => setNewSong({ ...newSong, link: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-yellow-500"
            />
            <input
              type="text"
              placeholder="Song Title"
              value={newSong.title}
              onChange={(e) => setNewSong({ ...newSong, title: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-yellow-500"
            />
            <input
              type="text"
              placeholder="Artist"
              value={newSong.artist}
              onChange={(e) => setNewSong({ ...newSong, artist: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-yellow-500"
            />
            <button
              onClick={addSong}
              className="w-full px-6 py-2 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-semibold"
            >
              Add
            </button>
          </div>
        )}
      </div>

      <div className="bg-slate-900 rounded-lg p-6">
        <h2 className="text-xl font-semibold text-white mb-4">Queue ({queue.length - 1} songs)</h2>
        <div className="space-y-3 max-h-[600px] overflow-y-auto">
          {queue.slice(1).map((song, idx) => (
            <div key={song.id} className="bg-slate-800 p-4 rounded-lg">
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <p className="text-yellow-400 text-sm font-semibold">#{idx + 2}</p>
                  <h4 className="text-white font-semibold">{song.title}</h4>
                  <p className="text-slate-400 text-sm">{song.artist}</p>
                  <p className="text-slate-500 text-xs mt-1">Added by {song.addedByUserName}</p>
                </div>
                <button
                  onClick={() => deleteSong(song.id)}
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