import { useState } from 'react';
import { User, Clock, Trash2 } from 'lucide-react';
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
  onDeleteProfile: (id: string) => void;
}

export function NameEntry({ onSubmit, existingProfiles, onSelectProfile, onDeleteProfile }: NameEntryProps) {
  const [name, setName] = useState<string>('');
  const [showProfiles, setShowProfiles] = useState<boolean>(false);

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
      
      {/* Main content - FIXED positioning with square aspect */}
      <div className="text-center relative z-10 w-full max-w-sm">
        <h1 className="text-6xl font-bold text-white mb-2">
          komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-8">Connect through music!</p>
        
        {/* Form section - FIXED position with square aspect */}
        <div className="space-y-4 mb-2 px-2">
          <input
            type="text"
            placeholder="Enter your name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="px-5 py-4 bg-slate-800 text-white rounded-lg w-full focus:outline-none focus:ring-2 focus:ring-yellow-500"
            onKeyPress={(e) => e.key === 'Enter' && name && onSubmit(name)}
          />
          
          <button
            onClick={() => name && onSubmit(name)}
            disabled={!name.trim()}
            className="w-full px-5 py-4 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
          >
            Continue
          </button>
        </div>
        
        {/* Toggle button - FIXED position */}
        {existingProfiles.length > 0 && (
          <button
            onClick={() => setShowProfiles(!showProfiles)}
            className="text-slate-400 hover:text-white text-sm flex items-center gap-2 mx-auto transition mb-2"
          >
            <Clock size={16} />
            {showProfiles ? 'Hide Profile History' : 'Show Profile History'}
          </button>
        )}
        
        {/* Profile history - absolute overlay with animation */}
        <div className="relative px-2">
          {existingProfiles.length > 0 && (
            <div
              className={`absolute left-0 right-0 top-full mt-2
                          max-h-48 overflow-y-auto overflow-visible
                          bg-slate-900 rounded-lg border border-slate-800 z-50
                          transform transition-all duration-200 origin-top
                          ${showProfiles ? "opacity-100 scale-100 translate-y-0" 
                                        : "opacity-0 scale-95 -translate-y-1 pointer-events-none"}`}
            >
              {existingProfiles.map(profile => (
                <div
                  key={profile.id}
                  className="px-4 py-3 border-b border-slate-800 last:border-0 hover:bg-slate-800 transition flex items-center"
                >
                  {/* Profile name on the left */}
                  <button
                    onClick={() => onSelectProfile(profile.id)}
                    className="flex items-center gap-2 text-left"
                  >
                    <User className="text-yellow-500 flex-shrink-0" size={18} />
                    <span className="text-white font-medium text-sm">{profile.name}</span>
                  </button>
                  
                  {/* Date information in the middle */}
                  <div className="flex-1 ml-4 text-xs">
                    <div className="flex items-center text-slate-400">
                      <span className="text-slate-500 w-16">Created:</span>
                      <span className="text-slate-300">{formatDate(profile.createdAt)}</span>
                    </div>
                    <div className="flex items-center text-slate-400">
                      <span className="text-slate-500 w-16">Last used:</span>
                      <span className="text-slate-300">{formatDate(profile.lastUsed)}</span>
                    </div>
                  </div>
                  
                  {/* Delete button on the right */}
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      onDeleteProfile(profile.id);
                    }}
                    className="p-1.5 text-slate-400 hover:text-red-500 hover:bg-slate-900 rounded transition flex-shrink-0 ml-2"
                    title="Remove profile"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
