import { useState, useEffect } from 'react';
import { Plus, Star } from 'lucide-react';
import api from '../services/api.service';
import type { Song } from '../services/api.service';
import { extractYoutubeId } from '../utils/youtube';

interface PlaylistViewProps {
  teamId: number;
  userId: number;
}

export function PlaylistView({ teamId, userId }: PlaylistViewProps) {
  const [songs, setSongs] = useState<Song[]>([]);
  const [showAdd, setShowAdd] = useState<boolean>(false);
  const [newSong, setNewSong] = useState({ link: '', title: '', artist: '' });
  const [userName, setUserName] = useState<string>('');

  useEffect(() => {
    fetchSongs();
    fetchUserName();
    const interval = setInterval(fetchSongs, 3000);
    return () => clearInterval(interval);
  }, [teamId]);

  const fetchUserName = async () => {
    try {
      const user = await api.usersApi.getById(teamId, userId);
      setUserName(user.name);
    } catch (error) {
      console.error('Error fetching user:', error);
    }
  };

  const fetchSongs = async () => {
    try {
      const data = await api.songsApi.getAll(teamId);
      setSongs(data);
    } catch (error) {
      console.error('Error fetching songs:', error);
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
      fetchSongs();
    } catch (error) {
      console.error('Error adding song:', error);
    }
  };

  const currentSong = songs[0];

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
              <h3 className="text-white font-semibold">{currentSong.title}</h3>
              <p className="text-slate-400">{currentSong.artist}</p>
            </div>
          ) : (
            <p className="text-slate-400">No songs in queue</p>
          )}
        </div>

        <button
          onClick={() => setShowAdd(!showAdd)}
          className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition flex items-center justify-center gap-2"
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
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Song Title"
              value={newSong.title}
              onChange={(e) => setNewSong({ ...newSong, title: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Artist"
              value={newSong.artist}
              onChange={(e) => setNewSong({ ...newSong, artist: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              onClick={addSong}
              className="w-full px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition"
            >
              Add
            </button>
          </div>
        )}
      </div>

      <div className="bg-slate-900 rounded-lg p-6">
        <h2 className="text-xl font-semibold text-white mb-4">Queue</h2>
        <div className="space-y-3">
          {songs.slice(1).map((song, idx) => (
            <div key={song.id} className="bg-slate-800 p-4 rounded-lg">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-slate-400 text-sm">#{idx + 2}</p>
                  <h4 className="text-white font-semibold">{song.title}</h4>
                  <p className="text-slate-400 text-sm">{song.artist}</p>
                </div>
                <div className="flex items-center gap-1">
                  <Star size={16} className="text-yellow-500" />
                  <span className="text-white">{song.rating || 0}</span>
                </div>
              </div>
            </div>
          ))}
          {songs.length <= 1 && <p className="text-slate-400">No songs in queue</p>}
        </div>
      </div>
    </div>
  );
}
