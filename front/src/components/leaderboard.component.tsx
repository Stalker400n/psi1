import { useState, useEffect } from 'react';
import { Trophy, TrendingUp, User } from 'lucide-react';
import api from '../services/api.service';
import type { Song } from '../services/api.service';

interface LeaderboardProps {
  teamId: number;
  userId: number;
}

interface SongWithRating extends Song {
  averageRating?: number;
  userRating?: number;
  ratingCount?: number;
}

export function Leaderboard({ teamId, userId }: LeaderboardProps) {
  const [songs, setSongs] = useState<SongWithRating[]>([]);
  const [view, setView] = useState<'average' | 'personal'>('average');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchLeaderboard();
    const interval = setInterval(fetchLeaderboard, 5000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teamId, userId, view]);

  const fetchLeaderboard = async () => {
    try {
      const data = await api.songsApi.getAll(teamId);
      
      const songsWithRatings = await Promise.all(
        data.map(async (song) => {
          try {
            const ratings = await api.ratingsApi.getSongRatings(teamId, song.id);
            const userRating = ratings.find(r => r.userId === userId);
            
            const averageRating = ratings.length > 0
              ? ratings.reduce((sum, r) => sum + r.rating, 0) / ratings.length
              : 0;

            return {
              ...song,
              averageRating,
              userRating: userRating?.rating || 0,
              ratingCount: ratings.length,
            };
          } catch {
            return {
              ...song,
              averageRating: 0,
              userRating: 0,
              ratingCount: 0,
            };
          }
        })
      );

      const sorted = songsWithRatings.sort((a, b) => {
        const ratingA = view === 'personal' ? (a.userRating || 0) : (a.averageRating || 0);
        const ratingB = view === 'personal' ? (b.userRating || 0) : (b.averageRating || 0);
        return ratingB - ratingA;
      });

      setSongs(sorted);
    } catch (error) {
      console.error('Error fetching leaderboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const getHeatColor = (rating: number) => {
    const hue = 240 - (rating / 100) * 240;
    return `hsl(${hue}, 80%, 50%)`;
  };

  return (
    <div className="h-full flex flex-col">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-white font-semibold text-sm">Leaderboard</h3>
        
        <div className="flex gap-1">
          <button
            onClick={() => setView('average')}
            className={`px-2 py-1 rounded text-xs transition ${
              view === 'average'
                ? 'bg-yellow-500 text-black'
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            <span className="flex items-center gap-1">
              <TrendingUp size={12} />
              Average
            </span>
          </button>
          
          <button
            onClick={() => setView('personal')}
            className={`px-2 py-1 rounded text-xs transition ${
              view === 'personal'
                ? 'bg-yellow-500 text-black'
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            <span className="flex items-center gap-1">
              <User size={12} />
              Personal
            </span>
          </button>
        </div>
      </div>

      {loading ? (
        <div className="text-slate-400 text-sm text-center py-8">Loading leaderboard...</div>
      ) : songs.length === 0 ? (
        <div className="text-slate-400 text-sm text-center py-8">No songs rated yet</div>
      ) : (
        <div className="flex-1 overflow-y-auto space-y-2" style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}>
          {songs.map((song, index) => {
            const rating = view === 'personal' ? (song.userRating || 0) : (song.averageRating || 0);
            const displayRating = Math.round(rating);
            
            return (
              <div
                key={song.id}
                className="bg-slate-800 p-3 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  <div className="flex-shrink-0 w-8 text-center">
                    {index === 0 && <Trophy className="text-yellow-500" size={20} />}
                    {index === 1 && <Trophy className="text-slate-400" size={18} />}
                    {index === 2 && <Trophy className="text-orange-700" size={16} />}
                    {index > 2 && (
                      <span className="text-slate-500 text-sm">#{index + 1}</span>
                    )}
                  </div>

                  <div className="flex-1 min-w-0">
                    <h4 className="text-white font-semibold text-sm truncate">{song.title}</h4>
                    <p className="text-slate-400 text-xs">{song.artist}</p>
                    <p className="text-slate-500 text-xs">Added by {song.addedByUserName}</p>
                  </div>

                  <div className="flex flex-col items-end gap-1">
                    <span
                      className="text-lg font-bold"
                      style={{ color: getHeatColor(displayRating) }}
                    >
                      {displayRating}%
                    </span>
                    {view !== 'personal' && (
                      <span className="text-xs text-slate-500">
                        {song.ratingCount} {song.ratingCount === 1 ? 'rating' : 'ratings'}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
