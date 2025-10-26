import { Home, Users, Plus } from 'lucide-react';

interface MainScreenProps {
  onCreateTeam: () => void;
  onBrowseTeams: () => void;
  onJoinTeam: () => void;
}

export function MainScreen({ onCreateTeam, onBrowseTeams, onJoinTeam }: MainScreenProps) {
  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-white mb-2">
          komcon<span className="text-yellow-400">.</span>
        </h1>
        <p className="text-slate-400 mb-12">Connect through music!</p>
        
        <div className="space-y-4">
          <button
            onClick={onCreateTeam}
            className="w-80 px-8 py-6 bg-yellow-500 text-black text-xl rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-3 font-semibold"
          >
            <Plus size={28} />
            Create Team
          </button>
          
          <button
            onClick={onBrowseTeams}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3 font-semibold"
          >
            <Users size={28} />
            Browse Public Teams
          </button>
          
          <button
            onClick={onJoinTeam}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3 font-semibold"
          >
            <Home size={28} />
            Join with Code
          </button>
        </div>
      </div>
    </div>
  );
}