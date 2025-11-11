import { useNavigate } from 'react-router-dom';
import { Home, Users, Plus, LogOut } from 'lucide-react';
import { 
  getRandomPraises, 
  generatePraiseStyles, 
  floatingQuotesCSS,
  renderFloatingQuote,
  renderPulsingStar
} from '../utils/praises';

const praises = getRandomPraises();
const praiseStyles = generatePraiseStyles(praises);

interface MainScreenProps {
  onLogout: () => void;
}

export function MainScreen({ onLogout }: MainScreenProps) {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      
      {/* Praises */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>

      {/* Main content */}
      <div className="text-center relative z-10">
        <div className="flex justify-end mb-4">
          <button
            onClick={onLogout}
            className="px-3 py-2 bg-slate-800 text-white rounded-lg hover:bg-slate-700 transition flex items-center gap-2"
            title="Logout"
          >
            <LogOut size={18} />
            <span>Logout</span>
          </button>
        </div>
        
        <h1 className="text-6xl font-bold text-white mb-2 drop-shadow-2xl">
          komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-12 drop-shadow-lg">Connect through music!</p>
        
        <div className="space-y-4">
          <button
            onClick={() => navigate('/teams/create')}
            className="w-80 px-8 py-6 bg-yellow-500 text-black text-xl rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-3 font-semibold shadow-xl hover:shadow-2xl"
          >
            <Plus size={28} />
            Create Team
          </button>
          
          <button
            onClick={() => navigate('/teams/browse')}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3 font-semibold shadow-xl hover:shadow-2xl"
          >
            <Users size={28} />
            Browse Public Teams
          </button>
          
          <button
            onClick={() => navigate('/teams/join')}
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
