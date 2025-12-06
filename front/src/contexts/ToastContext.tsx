import { createContext, useContext } from 'react';

// Toast Types and Interfaces
export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
}

export interface ToastContextType {
  showToast: (message: string, type: ToastType) => void;
}

// Create Context
export const ToastContext = createContext<ToastContextType | undefined>(undefined);

// Custom Hook
export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within ToastProvider');
  }
  return context;
}
