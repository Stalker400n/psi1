import { useState, useEffect } from 'react';
import { Trophy, Flame, TrendingUp, User } from 'lucide-react';
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
  const [view, setView] = useState<'team' | 'personal'>('team');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchLeaderboard();
    const interval = setInterval(fetchLeaderboard, 5000);
    return () => clearInterval(interval);
  }, [teamId, userId, view]);

  const fetchLeaderboard = async () => {
    try {
      const data = await api.songsApi.getAll(teamId);
      
      // Fetch ratings for all songs
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

      // Sort by the appropriate rating
      const sorted = songsWithRatings.sort((a, b) => {
        const ratingA = view === 'team' ? (a.averageRating || 0) : (a.userRating || 0);
        const ratingB = view === 'team' ? (b.averageRating || 0) : (b.userRating || 0);
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

  const getHeatMessage = (rating: number) => {
    if (rating >= 80) return 'FIRE! 🔥🔥🔥';
    if (rating >= 60) return 'Hot! 🔥🔥';
    if (rating >= 40) return 'Warm 🔥';
    if (rating >= 20) return 'Cool ❄️';
    return 'Ice cold! ❄️❄️';
  };

  return (
    <div className="bg-slate-900 rounded-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <Trophy className="text-yellow-500" size={28} />
          <h2 className="text-2xl font-bold text-white">Leaderboard</h2>
        </div>

        <div className="flex gap-2">
          <button
            onClick={() => setView('team')}
            className={`px-6 py-2 rounded-lg transition font-semibold flex items-center gap-2 ${
              view === 'team'
                ? 'bg-yellow-500 text-black'
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            <TrendingUp size={18} />
            Team Average
          </button>
          <button
            onClick={() => setView('personal')}
            className={`px-6 py-2 rounded-lg transition font-semibold flex items-center gap-2 ${
              view === 'personal'
                ? 'bg-yellow-500 text-black'
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            <User size={18} />
            My Ratings
          </button>
        </div>
      </div>

      {loading ? (
        <div className="text-slate-400 text-center py-8">Loading leaderboard...</div>
      ) : songs.length === 0 ? (
        <div className="text-slate-400 text-center py-8">No songs rated yet</div>
      ) : (
        <div className="space-y-3 max-h-[600px] overflow-y-auto">
          {songs.map((song, index) => {
            const rating = view === 'team' ? (song.averageRating || 0) : (song.userRating || 0);
            const displayRating = Math.round(rating);
            
            return (
              <div
                key={song.id}
                className="bg-slate-800 p-4 rounded-lg hover:bg-slate-750 transition"
              >
                <div className="flex items-center gap-4">
                  {/* Rank */}
                  <div className="flex-shrink-0 w-12 h-12 flex items-center justify-center">
                    {index === 0 && <Trophy className="text-yellow-500" size={32} />}
                    {index === 1 && <Trophy className="text-slate-400" size={28} />}
                    {index === 2 && <Trophy className="text-orange-700" size={24} />}
                    {index > 2 && (
                      <span className="text-slate-500 text-xl font-bold">#{index + 1}</span>
                    )}
                  </div>

                  {/* Song info */}
                  <div className="flex-1">
                    <h4 className="text-white font-semibold">{song.title}</h4>
                    <p className="text-slate-400 text-sm">{song.artist}</p>
                    <p className="text-slate-500 text-xs">Added by {song.addedByUserName}</p>
                  </div>

                  {/* Heat display */}
                  <div className="flex flex-col items-end gap-1">
                    <div className="flex items-center gap-2">
                      <Flame 
                        size={20} 
                        style={{ color: getHeatColor(displayRating) }}
                      />
                      <span
                        className="text-2xl font-bold"
                        style={{ color: getHeatColor(displayRating) }}
                      >
                        {displayRating}%
                      </span>
                    </div>
                    <span className="text-xs text-slate-400">
                      {getHeatMessage(displayRating)}
                    </span>
                    {view === 'team' && (
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