import { Home, Users, Plus } from 'lucide-react';

interface MainScreenProps {
  onCreateTeam: () => void;
  onBrowseTeams: () => void;
  onJoinTeam: () => void;
}

export function MainScreen({ onCreateTeam, onBrowseTeams, onJoinTeam }: MainScreenProps) {
  // Add console logs to debug button clicks
  const handleCreateClick = () => {
    console.log("Create Team button clicked");
    onCreateTeam();
  };

  const handleBrowseClick = () => {
    console.log("Browse Teams button clicked");
    onBrowseTeams();
  };

  const handleJoinClick = () => {
    console.log("Join Team button clicked");
    onJoinTeam();
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-white mb-2">komcon.</h1>
        <p className="text-slate-400 mb-12">Connect through music!</p>
        
        <div className="space-y-4">
          <button
            onClick={handleCreateClick}
            className="w-80 px-8 py-6 bg-blue-600 text-white text-xl rounded-lg hover:bg-blue-700 transition flex items-center justify-center gap-3"
          >
            <Plus size={28} />
            Create Team
          </button>
          
          <button
            onClick={handleBrowseClick}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3"
          >
            <Users size={28} />
            Browse Public Teams
          </button>
          
          <button
            onClick={handleJoinClick}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3"
          >
            <Home size={28} />
            Join with Code
          </button>
        </div>
      </div>
    </div>
  );
}
