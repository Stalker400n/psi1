import { useState } from 'react';

interface NameEntryProps {
  onSubmit: (name: string) => void;
}

export function NameEntry({ onSubmit }: NameEntryProps) {
  const [name, setName] = useState<string>('');

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-white mb-2">komcon.</h1>
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
          className="block w-80 mx-auto px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
        >
          Continue
        </button>
      </div>
    </div>
  );
}
