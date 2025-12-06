import { useEffect } from 'react';
import { useParams } from 'react-router-dom';

interface TeamIdCaptureProps {
  onCapture: (teamId: number) => void;
}

export function TeamIdCapture({ onCapture }: TeamIdCaptureProps) {
  const params = useParams<{ teamId: string }>();
  
  useEffect(() => {
    if (params.teamId) {
      const id = parseInt(params.teamId, 10);
      if (!isNaN(id)) {
        onCapture(id);
      }
    }
  }, [params.teamId, onCapture]);
  
  return null;
}
