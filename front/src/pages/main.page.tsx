import { useNavigate } from 'react-router-dom';
import { Home, Users, Plus, LogOut, User } from 'lucide-react';
import { 
  getRandomPraises, 
  generatePraiseStyles, 
  floatingQuotesCSS,
  renderFloatingQuote,
  renderPulsingStar
} from '../utils/praise.util';

const praises = getRandomPraises();
const praiseStyles = generatePraiseStyles(praises);

interface MainPageProps {
  onLogout: () => void;
  profileName: string;
}

export function MainPage({ onLogout, profileName }: MainPageProps) {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      
      {/* Praises */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>

      {/* Profile in top left corner */}
      <div className="absolute top-4 left-4 z-10">
        <div className="flex items-center gap-3 px-4 py-2 bg-slate-800 rounded-lg border border-slate-700 hover:border-slate-600 transition">
          <div className="flex items-center gap-2">
            <User className="text-yellow-500" size={18} />
            <span className="text-white font-medium">{profileName}</span>
          </div>
          <button
            onClick={onLogout}
            className="flex items-center gap-1 px-3 py-1 bg-slate-700 text-white rounded hover:bg-slate-600 transition text-sm font-medium"
          >
            <LogOut size={14} />
            Log out
          </button>
        </div>
      </div>

      {/* Main content */}
      <div className="text-center relative z-10">
        <h1 className="text-6xl font-bold text-white mb-2 drop-shadow-2xl">
          komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-12 drop-shadow-lg">Connect through music!</p>
        
        <div className="space-y-4">
          <button
            onClick={() => navigate('/create')}
            className="w-80 px-8 py-6 bg-yellow-500 text-black text-xl rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-3 font-semibold shadow-xl hover:shadow-2xl"
          >
            <Plus size={28} />
            Create Team
          </button>
          
          <button
            onClick={() => navigate('/teams')}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3 font-semibold shadow-xl hover:shadow-2xl"
          >
            <Users size={28} />
            Browse Teams
          </button>
          
          <button
            onClick={() => navigate('/join')}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3 font-semibold shadow-xl hover:shadow-2xl"
          >
            <Home size={28} />
            Join with Code
          </button>
        </div>
      </div>

      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
