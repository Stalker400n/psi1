import { useState } from 'react';
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
  onSubmit: (name: string) => void;
}

export function NameEntry({ onSubmit }: NameEntryProps) {
  const [name, setName] = useState<string>('');

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Praises */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {praises.map((praise, idx) => renderFloatingQuote(praise, praiseStyles[idx], idx))}
      </div>
      
      {/* Main content */}
      <div className="text-center relative z-10">
        <h1 className="text-6xl font-bold text-white mb-2">
            komcon{renderPulsingStar({ className: 'text-yellow-400' })}
        </h1>
        <p className="text-slate-400 mb-8">Connect through music!</p>
        <input
          type="text"
          placeholder="Enter your name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="px-6 py-3 bg-slate-800 text-white rounded-lg mb-4 w-80 focus:outline-none focus:ring-2 focus:ring-blue-500"
          onKeyPress={(e) => e.key === 'Enter' && name && onSubmit(name)}
        />
        <button
          onClick={() => name && onSubmit(name)}
          className="block w-80 mx-auto px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition"
        >
          Continue
        </button>
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
