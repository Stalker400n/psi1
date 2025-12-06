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
  
  // Add a ref to track if we're currently in the submission process
  const isSubmittingRef = useRef(false);

  useEffect(() => {
    // Only set heat from currentRating when not user-controlled
    if (!isUserControlled) {
      setHeat(currentRating);
    }
    
    // Only reset submission states when not actively submitting
    if (!isSubmittingRef.current) {
      setLastSubmittedHeat(currentRating);
      // Only reset isSubmitted if we're getting a new currentRating from parent
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

  // Helper function to update heat (no longer resets submitted state)
  const updateHeat = (value: number) => {
    setHeat(value);
    setIsUserControlled(true); // Mark that user is controlling the slider
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
    isSubmittingRef.current = true; // Mark that we're in submission process
    
    try {
      await onSubmit(heat);
      setLastSubmittedHeat(heat); // Track last submitted heat value
      setIsSubmitted(true); // Mark as successfully submitted
      setSubmissionCount(prev => prev + 1); // Increment counter to force re-render
      
      // Trigger animation effect
      setAnimateButton(true);
      setTimeout(() => setAnimateButton(false), 300);
      
      showToast('Heat rating submitted successfully!', 'success');
    } catch {
      showToast('Failed to submit rating', 'error');
    } finally {
      setIsSubmitting(false);
      
      // Reset the ref after a small delay to ensure state updates complete
      setTimeout(() => {
        isSubmittingRef.current = false;
      }, 100);
    }
  };

  return (
    <div className="flex flex-col items-center gap-3 bg-slate-900 p-3 rounded-lg h-full">
      {/* Compact title */}
      <div className="flex flex-col items-center gap-1">
        <Flame className="text-orange-500" size={20} />
        <h3 className="text-white font-semibold text-xs text-center">HEAT</h3>
      </div>

      {/* Meter - taller */}
      <div
        className="relative w-12 h-full min-h-[500px] bg-slate-800 rounded-full cursor-pointer select-none"
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
          className="absolute left-1/2 -translate-x-1/2 w-16 h-5 rounded-full border-2 border-white shadow-lg transition-all"
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

      {/* Submit button - compact */}
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
