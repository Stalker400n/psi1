import { useState, useEffect, useRef } from 'react';
import { Flame } from 'lucide-react';
import { useToast } from '../contexts/toast-context';

interface HeatMeterProps {
  currentRating: number;
  onSubmit: (rating: number) => Promise<void>;
}

export function HeatMeter({ currentRating, onSubmit }: HeatMeterProps) {
  const { showToast } = useToast();
  const [heat, setHeat] = useState(currentRating);
  const [lastSubmittedHeat, setLastSubmittedHeat] = useState(currentRating);
  const [isDragging, setIsDragging] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [submissionCount, setSubmissionCount] = useState(0);
  const [animateButton, setAnimateButton] = useState(false);
  const [isUserControlled, setIsUserControlled] = useState(false);
  
  const isSubmittingRef = useRef(false);

  useEffect(() => {
    if (!isUserControlled) {
      setHeat(currentRating);
    }
    
    if (!isSubmittingRef.current) {
      setLastSubmittedHeat(currentRating);
      if (currentRating !== heat && !isUserControlled) {
        setIsSubmitted(false);
      }
    }
  }, [currentRating, heat, isUserControlled]);

  const getColor = (value: number) => {
    const hue = 240 - (value / 100) * 240; // 240 = blue, 0 = red
    return `hsl(${hue}, 80%, 50%)`;
  };

  const handleMouseDown = () => {
    setIsDragging(true);
    setIsUserControlled(true);
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const updateHeat = (value: number) => {
    setHeat(value);
    setIsUserControlled(true);
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!isDragging) return;
    
    const rect = e.currentTarget.getBoundingClientRect();
    const y = e.clientY - rect.top;
    const percentage = 100 - Math.max(0, Math.min(100, (y / rect.height) * 100));
    updateHeat(Math.round(percentage));
  };

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const y = e.clientY - rect.top;
    const percentage = 100 - Math.max(0, Math.min(100, (y / rect.height) * 100));
    updateHeat(Math.round(percentage));
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    isSubmittingRef.current = true;
    
    try {
      await onSubmit(heat);
      setLastSubmittedHeat(heat);
      setIsSubmitted(true);
      setSubmissionCount(prev => prev + 1);
      
      setAnimateButton(true);
      setTimeout(() => setAnimateButton(false), 300);
      
      showToast('Heat rating submitted successfully!', 'success');
    } catch {
      showToast('Failed to submit rating', 'error');
    } finally {
      setIsSubmitting(false);
      
      setTimeout(() => {
        isSubmittingRef.current = false;
      }, 100);
    }
  };

  return (
    <div className="flex flex-col items-center gap-3 bg-slate-900 p-3 rounded-lg h-full">
      <div className="flex flex-col items-center gap-1">
        <Flame className="text-orange-500" size={20} />
        <h3 className="text-white font-semibold text-xs text-center">HEAT</h3>
      </div>

      <div
        className="relative w-12 h-full min-h-[500px] bg-slate-800 rounded-full cursor-pointer select-none"
        onMouseDown={handleMouseDown}
        onMouseUp={handleMouseUp}
        onMouseMove={handleMouseMove}
        onMouseLeave={handleMouseUp}
        onClick={handleClick}
      >
        <div
          className="absolute bottom-0 left-0 right-0 rounded-full transition-all"
          style={{
            height: `${heat}%`,
            backgroundColor: getColor(heat),
          }}
        />

        <div
          className="absolute left-1/2 -translate-x-1/2 w-16 h-5 rounded-full border-2 border-white shadow-lg transition-all"
          style={{
            bottom: `${heat}%`,
            transform: 'translate(-50%, 50%)',
            backgroundColor: getColor(heat),
          }}
        />

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
        key={`submit-button-${submissionCount}`}
        onClick={handleSubmit}
        disabled={isSubmitting}
        className={`w-full px-2 py-2 text-black rounded-lg hover:opacity-90 transition font-semibold disabled:opacity-50 disabled:cursor-not-allowed text-xs ${
          isSubmitted ? '' : 'bg-yellow-500 hover:bg-yellow-400'
        } ${animateButton ? 'animate-pulse' : ''}`}
        style={{
          backgroundColor: isSubmitted ? getColor(lastSubmittedHeat) : undefined
        }}
      >
        {isSubmitting ? '...' : 'Rate'}
      </button>
    </div>
  );
}
