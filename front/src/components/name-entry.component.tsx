import { useState, useEffect } from 'react';
import api from '../services/api.service';
import type { GlobalUser } from '../services/api.service';
import { fingerprintService } from '../services/fingerprint.service';
import { useToast } from '../contexts/toast-context';
import { 
  getRandomPraises, 
  generatePraiseStyles, 
  floatingQuotesCSS,
  renderFloatingQuote,
  renderPulsingStar
} from '../utils/praise.util';
import { ChevronDown, ChevronUp, Trash2 } from 'lucide-react';

const praises = getRandomPraises();
const praiseStyles = generatePraiseStyles(praises);

interface NameEntryProps {
  onSubmit: (user: GlobalUser) => void;
}

interface ProfileHistory {
  name: string;
  lastUsed: string;
}

export function NameEntry({ onSubmit }: NameEntryProps) {
  const { showToast } = useToast();
  const [name, setName] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [showHistory, setShowHistory] = useState<boolean>(false);
  const [profileHistory, setProfileHistory] = useState<ProfileHistory[]>([]);

  useEffect(() => {
    const stored = localStorage.getItem('profileHistory');
    if (stored) {
      try {
        const history = JSON.parse(stored);
        setProfileHistory(history);
      } catch (e) {
        console.error('Failed to parse profile history:', e);
        setProfileHistory([]);
      }
    }
  }, []);

  const addToHistory = (profileName: string) => {
    const newProfile: ProfileHistory = {
      name: profileName,
      lastUsed: new Date().toISOString()
    };

    const updatedHistory = [
      newProfile,
      ...profileHistory.filter(p => p.name !== profileName)
    ].slice(0, 10);

    setProfileHistory(updatedHistory);
    localStorage.setItem('profileHistory', JSON.stringify(updatedHistory));
  };

  const removeFromHistory = (profileName: string) => {
    const updatedHistory = profileHistory.filter(p => p.name !== profileName);
    setProfileHistory(updatedHistory);
    localStorage.setItem('profileHistory', JSON.stringify(updatedHistory));
  };

  const handleSubmit = async (submittedName?: string) => {
    const nameToUse = submittedName || name;
    if (!nameToUse.trim()) return;
    
    setIsLoading(true);
    
    try {
      const fingerprint = await fingerprintService.getFingerprint();
      const deviceInfo = fingerprintService.getDeviceInfo();
      
      const globalUser = await api.globalUsersApi.registerOrLogin({
        name: nameToUse.trim(),
        deviceFingerprint: fingerprint,
        deviceInfo
      });
      
      addToHistory(nameToUse.trim());
      onSubmit(globalUser);
      
    } catch (err) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Failed to connect';
      
      showToast(errorMessage, 'error');
      console.error('Failed to login:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>
      
      <div className="text-center relative z-10 w-full max-w-sm">
        <h1 className="text-6xl font-bold text-white mb-2">
          komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-8">Connect through music!</p>
        
        <div className="space-y-4 mb-4">
          <input
            type="text"
            placeholder="Enter your name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && !isLoading && name && handleSubmit()}
            className="px-5 py-4 bg-slate-800 text-white rounded-lg w-full focus:outline-none focus:ring-2 focus:ring-yellow-500"
            disabled={isLoading}
            maxLength={50}
          />
          
          <button
            onClick={() => handleSubmit()}
            disabled={!name.trim() || isLoading}
            className="w-full px-5 py-4 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
          >
            {isLoading ? 'Connecting...' : 'Connect'}
          </button>
        </div>

        {profileHistory.length > 0 && (
          <button
            onClick={() => setShowHistory(!showHistory)}
            className="w-full px-4 py-2 bg-slate-800 text-slate-300 rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-2 mb-4"
          >
            {showHistory ? (
              <>
                <ChevronUp size={18} />
                Hide Profile History
              </>
            ) : (
              <>
                <ChevronDown size={18} />
                View Profile History
              </>
            )}
          </button>
        )}

        {showHistory && profileHistory.length > 0 && (
          <div className="bg-slate-800/50 rounded-lg p-4 space-y-2">
            <p className="text-slate-400 text-xs mb-3">Recent profiles on this device</p>
            {profileHistory.map((profile, idx) => (
              <div
                key={idx}
                className="flex items-center justify-between bg-slate-900 p-3 rounded-lg hover:bg-slate-800 transition group"
              >
                <button
                  onClick={() => handleSubmit(profile.name)}
                  disabled={isLoading}
                  className="flex-1 text-left"
                >
                  <p className="text-white font-medium">{profile.name}</p>
                  <p className="text-slate-500 text-xs">
                    Last used: {new Date(profile.lastUsed).toLocaleDateString()}
                  </p>
                </button>
                <button
                  onClick={() => removeFromHistory(profile.name)}
                  className="ml-2 p-2 text-slate-500 hover:text-red-400 opacity-0 group-hover:opacity-100 transition"
                  title="Remove from history"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
