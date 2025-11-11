import { useState } from 'react';
import { User, UserPlus, Clock } from 'lucide-react';
import { 
  getRandomPraises, 
  generatePraiseStyles, 
  floatingQuotesCSS,
  renderFloatingQuote,
  renderPulsingStar
} from '../utils/praises';

// Get random praises and generate their styles
const praises = getRandomPraises();
const praiseStyles = generatePraiseStyles(praises);

// Define the UserProfile type
interface UserProfile {
  id: string;
  name: string;
  createdAt: string;
  lastUsed: string;
}

interface NameEntryProps {
  onSubmit: (name: string) => void;
  existingProfiles: UserProfile[];
  onSelectProfile: (id: string) => void;
}

export function NameEntry({ onSubmit, existingProfiles, onSelectProfile }: NameEntryProps) {
  const [name, setName] = useState<string>('');
  const [showProfiles, setShowProfiles] = useState<boolean>(existingProfiles.length > 0);

  // Format date for display
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Praises */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>
      
      {/* Main content */}
      <div className="text-center relative z-10 w-full max-w-md">
        <h1 className="text-6xl font-bold text-white mb-2">
            komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-8">Connect through music!</p>
        
        {showProfiles && existingProfiles.length > 0 ? (
          <>
            <h2 className="text-white text-xl mb-4">Select your profile</h2>
            <div className="space-y-3 mb-6">
              {existingProfiles.map(profile => (
                <button
                  key={profile.id}
                  onClick={() => onSelectProfile(profile.id)}
                  className="w-full flex items-center justify-between px-4 py-3 bg-slate-800 text-white rounded-lg hover:bg-slate-700 transition text-left"
                >
                  <div className="flex items-center gap-3">
                    <User className="text-yellow-500" />
                    <div>
                      <div className="font-medium">{profile.name}</div>
                      <div className="text-xs text-slate-400 flex items-center gap-1">
                        <Clock size={12} />
                        Last used: {formatDate(profile.lastUsed)}
                      </div>
                    </div>
                  </div>
                </button>
              ))}
            </div>
            <button
              onClick={() => setShowProfiles(false)}
              className="text-slate-400 hover:text-white text-sm flex items-center gap-1 mx-auto mb-4"
            >
              <UserPlus size={16} />
              Create new profile
            </button>
          </>
        ) : (
          <>
            <input
              type="text"
              placeholder="Enter your name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="px-6 py-3 bg-slate-800 text-white rounded-lg mb-4 w-full focus:outline-none focus:ring-2 focus:ring-blue-500"
              onKeyPress={(e) => e.key === 'Enter' && name && onSubmit(name)}
            />
            <button
              onClick={() => name && onSubmit(name)}
              className="block w-full mx-auto px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition"
            >
              Continue
            </button>
            
            {existingProfiles.length > 0 && (
              <button
                onClick={() => setShowProfiles(true)}
                className="text-slate-400 hover:text-white text-sm mt-4 flex items-center gap-1 mx-auto"
              >
                <User size={16} />
                Use existing profile
              </button>
            )}
          </>
        )}
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
