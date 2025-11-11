import { useState, useEffect } from 'react';
import { Flame } from 'lucide-react';

interface HeatMeterProps {
  currentRating: number;
  onSubmit: (rating: number) => Promise<void>;
}

export function HeatMeter({ currentRating, onSubmit }: HeatMeterProps) {
  const [heat, setHeat] = useState(currentRating);
  const [isDragging, setIsDragging] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    setHeat(currentRating);
  }, [currentRating]);

  const getColor = (value: number) => {
    const hue = 240 - (value / 100) * 240; // 240 = blue, 0 = red
    return `hsl(${hue}, 80%, 50%)`;
  };

  const handleMouseDown = () => {
    setIsDragging(true);
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!isDragging) return;
    
    const rect = e.currentTarget.getBoundingClientRect();
    const y = e.clientY - rect.top;
    const percentage = 100 - Math.max(0, Math.min(100, (y / rect.height) * 100));
    setHeat(Math.round(percentage));
  };

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const y = e.clientY - rect.top;
    const percentage = 100 - Math.max(0, Math.min(100, (y / rect.height) * 100));
    setHeat(Math.round(percentage));
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      await onSubmit(heat);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col items-center gap-4 bg-slate-900 p-6 rounded-lg">
      <div className="flex items-center gap-2 mb-2">
        <Flame className="text-orange-500" size={24} />
        <h3 className="text-white font-semibold text-lg">HEAT METER</h3>
      </div>

      <div
        className="relative w-16 h-80 bg-slate-800 rounded-full cursor-pointer select-none"
        onMouseDown={handleMouseDown}
        onMouseUp={handleMouseUp}
        onMouseMove={handleMouseMove}
        onMouseLeave={handleMouseUp}
        onClick={handleClick}
      >
        {/* Heat fill */}
        <div
          className="absolute bottom-0 left-0 right-0 rounded-full transition-all"
          style={{
            height: `${heat}%`,
            backgroundColor: getColor(heat),
          }}
        />

        {/* Slider thumb */}
        <div
          className="absolute left-1/2 -translate-x-1/2 w-20 h-6 rounded-full border-4 border-white shadow-lg transition-all"
          style={{
            bottom: `${heat}%`,
            transform: 'translate(-50%, 50%)',
            backgroundColor: getColor(heat),
          }}
        />

        {/* Gradient markers */}
        <div className="absolute inset-0 pointer-events-none">
          {[0, 25, 50, 75, 100].map((mark) => (
            <div
              key={mark}
              className="absolute left-0 right-0 h-px bg-slate-600"
              style={{ bottom: `${mark}%` }}
            />
          ))}
        </div>
      </div>

      <button
        onClick={handleSubmit}
        disabled={isSubmitting}
        className="w-full px-6 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition font-semibold disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {isSubmitting ? 'Submitting...' : 'Submit Rating'}
      </button>
    </div>
  );
}