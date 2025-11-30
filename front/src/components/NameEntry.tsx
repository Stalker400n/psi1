import { useState } from 'react';
import api from '../services/api.service';
import type { GlobalUser } from '../services/api.service';
import { fingerprintService } from '../services/fingerprint.service';
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

interface NameEntryProps {
  onSubmit: (user: GlobalUser) => void;
}

export function NameEntry({ onSubmit }: NameEntryProps) {
  const [name, setName] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  const handleSubmit = async () => {
    if (!name.trim()) return;
    
    setIsLoading(true);
    setError('');
    
    try {
      // Get device fingerprint
      const fingerprint = await fingerprintService.getFingerprint();
      const deviceInfo = fingerprintService.getDeviceInfo();
      
      // Try to register or login
      const globalUser = await api.globalUsersApi.registerOrLogin({
        name: name.trim(),
        deviceFingerprint: fingerprint,
        deviceInfo
      });
      
      // Success - proceed
      onSubmit(globalUser);
      
    } catch (err) {
      const errorMessage = err instanceof Error 
        ? err.message 
        : 'Failed to connect';
      setError(errorMessage);
      console.error('Failed to login:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Praises background */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>
      
      {/* Main content */}
      <div className="text-center relative z-10 w-full max-w-sm">
        <h1 className="text-6xl font-bold text-white mb-2">
          komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-8">Connect through music!</p>
        
        {/* Error message */}
        {error && (
          <div className="mb-4 p-3 bg-red-900/30 border border-red-600 rounded-lg text-red-300 text-sm">
            {error}
          </div>
        )}
        
        {/* Name input */}
        <div className="space-y-4">
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
            onClick={handleSubmit}
            disabled={!name.trim() || isLoading}
            className="w-full px-5 py-4 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
          >
            {isLoading ? 'Connecting...' : 'Connect'}
          </button>
        </div>
        
        {/* Device info note */}
        <p className="text-slate-500 text-xs mt-4">
          Your name will be tied to this device
        </p>
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
