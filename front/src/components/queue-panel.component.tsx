import { useState } from 'react';
import { Trash2 } from 'lucide-react';
import type { Song } from '../services/api.service';

interface QueuePanelProps {
  queue: Song[];
  currentIndex: number;
  onDeleteSong: (songId: number) => void;
}

export function QueuePanel({ queue, currentIndex, onDeleteSong }: QueuePanelProps) {
  const [showHistory, setShowHistory] = useState<boolean>(false);
  
  const playedSongs = queue.filter(song => song.index < currentIndex);
  const currentSong = queue.find(song => song.index === currentIndex);
  const upcomingSongs = queue.filter(song => song.index > currentIndex);
  
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
            {song.thumbnailUrl && (
              <img 
                src={song.thumbnailUrl} 
                alt={song.title}
                className="w-20 h-12 object-cover rounded"
              />
            )}
            
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
          
            {status === 'upcoming' && (
              <button
                onClick={() => onDeleteSong(song.id)}
                className="p-1 bg-red-600 text-white rounded hover:bg-red-700 transition h-fit"
              >
                <Trash2 size={14} />
              </button>
            )}
          </div>
          
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
        {currentSong && renderSongItem(currentSong, 'current')}
        
        {currentSong && upcomingSongs.length > 0 && (
          <div className="border-t border-slate-700 my-2"></div>
        )}
        
        {upcomingSongs.length > 0 ? (
          upcomingSongs.map(song => renderSongItem(song, 'upcoming'))
        ) : (
          <p className="text-slate-400 text-sm text-center py-4">
            No upcoming songs
          </p>
        )}
        
        {playedSongs.length > 0 && (
          <button
            onClick={() => setShowHistory(!showHistory)}
            className="w-full mt-4 px-3 py-2 bg-slate-700 text-white rounded text-sm hover:bg-slate-600"
          >
            {showHistory ? 'Hide' : 'Show'} History ({playedSongs.length} played)
          </button>
        )}
        
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